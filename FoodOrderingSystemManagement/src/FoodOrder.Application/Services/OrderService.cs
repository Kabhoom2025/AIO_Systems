using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs.Order;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Constants;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IAddOnRepository _addOnRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILedgerRepository    _ledgerRepository;
    private readonly IDeliveryRepository  _deliveryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ISmsService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        IFoodItemRepository foodItemRepository,
        IUserRepository userRepository,
        ISettingsRepository settingsRepository,
        ITableRepository tableRepository,
        IAddOnRepository addOnRepository,
        ICustomerRepository customerRepository,
        ILedgerRepository ledgerRepository,
        IDeliveryRepository deliveryRepository,
        IBranchRepository branchRepository,
        ICurrentUserContext currentUser,
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        IMapper mapper)
    {
        _orderRepository    = orderRepository;
        _foodItemRepository = foodItemRepository;
        _userRepository     = userRepository;
        _settingsRepository = settingsRepository;
        _tableRepository    = tableRepository;
        _addOnRepository    = addOnRepository;
        _customerRepository = customerRepository;
        _ledgerRepository   = ledgerRepository;
        _deliveryRepository = deliveryRepository;
        _branchRepository   = branchRepository;
        _currentUser        = currentUser;
        _smsService         = smsService;
        _whatsAppService    = whatsAppService;
        _mapper             = mapper;
    }

    public async Task<ReceiptDto> CreateOrderAsync(CreateOrderDto dto, int cashierId)
    {
        // 1. Validate the authenticated cashier exists.
        var cashier = await _userRepository.GetByIdAsync(cashierId)
            ?? throw new AppException("Cashier session is invalid. Please log in again.", statusCode: 401);

        // 2. Load restaurant settings for tax calculation.
        var settings = await _settingsRepository.GetSettingsAsync()
            ?? throw new AppException("Restaurant settings are not configured. Please set up settings first.");

        // 2b. Load customer if provided and validate points redemption.
        Customer? customer = null;
        if (dto.CustomerId.HasValue)
        {
            customer = await _customerRepository.GetByIdAsync(dto.CustomerId.Value)
                ?? throw new AppException($"Customer {dto.CustomerId.Value} not found.");
            if (!customer.IsActive)
                throw new AppException("Customer account is inactive.");
            if (dto.PointsRedeemed > customer.LoyaltyPoints)
                throw new AppException($"Insufficient loyalty points. Available: {customer.LoyaltyPoints}, Requested: {dto.PointsRedeemed}.");
        }

        // 3. Consolidate duplicate food item entries by summing their quantities.
        var consolidatedItems = dto.Items
            .GroupBy(i => i.FoodItemId)
            .Select(g => new OrderItemRequestDto
            {
                FoodItemId = g.Key,
                Quantity = g.Sum(i => i.Quantity),
                AddOnIds = g.First().AddOnIds
            })
            .ToList();

        // 4. Validate each item against business rules and lock prices.
        var lineItems = new List<(FoodItem FoodItem, int Quantity, decimal EffectivePrice, string? AddOnNotes)>();

        foreach (var request in consolidatedItems)
        {
            var foodItem = await _foodItemRepository.GetByIdWithCategoryAsync(request.FoodItemId)
                ?? throw new NotFoundException("Food item", request.FoodItemId);

            if (!foodItem.IsAvailable)
                throw new AppException($"'{foodItem.ItemName}' is currently unavailable and cannot be ordered.");

            if (foodItem.Category != null && !foodItem.Category.IsActive)
                throw new AppException($"The category '{foodItem.Category.CategoryName}' is inactive. Cannot place order.");

            if (request.Quantity > foodItem.AvailableQuantity)
                throw new AppException(
                    $"Insufficient stock for '{foodItem.ItemName}'. " +
                    $"Available: {foodItem.AvailableQuantity}, Requested: {request.Quantity}.");

            var (addOnPrice, addOnNotes) = await ResolveAddOnsAsync(request.AddOnIds);
            lineItems.Add((foodItem, request.Quantity, foodItem.Price + addOnPrice, addOnNotes));
        }

        // 5. Calculate bill — prices are locked at order time, not looked up at payment time.
        var subTotal = lineItems.Sum(x => x.EffectivePrice * x.Quantity);
        var taxAmount = Math.Round(subTotal * settings.TaxPercentage / 100, 2, MidpointRounding.AwayFromZero);
        var discount = OrderConstants.DefaultDiscount;
        var redemptionDiscount = dto.PointsRedeemed > 0
            ? Math.Round(dto.PointsRedeemed * settings.PointsRedeemRate, 2)
            : 0m;
        var grandTotal = Math.Max(0m, subTotal + taxAmount - discount - redemptionDiscount);

        // 6. Generate the next sequential order number.
        var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

        // 7. Validate table if provided.
        if (dto.TableId.HasValue)
        {
            var table = await _tableRepository.GetByIdAsync(dto.TableId.Value)
                ?? throw new AppException($"Table {dto.TableId.Value} not found.");
            if (!table.IsActive)
                throw new AppException($"Table {table.TableNumber} is inactive.");
        }

        // 8. Build the Order aggregate with all OrderItems.
        var orderType = Enum.TryParse<OrderType>(dto.OrderType, out var parsed) ? parsed : OrderType.DineIn;
        var isDelivery = orderType == OrderType.Delivery;
        var isTakeaway = orderType == OrderType.Takeaway;

        // Delivery orders stay Pending until delivered; takeaway completes immediately.
        var orderStatus = (dto.TableId.HasValue || isDelivery)
            ? OrderStatus.Pending
            : OrderStatus.Completed;

        var order = new Order
        {
            BranchId        = await TenantResolution.ResolveWriteBranchIdAsync(_currentUser, _branchRepository),
            OrderNumber     = orderNumber,
            CashierId       = (int?)cashierId,
            TableId         = dto.TableId,
            CustomerPhone   = dto.CustomerPhone,
            CustomerId      = dto.CustomerId,
            PointsRedeemed  = dto.PointsRedeemed,
            OrderDate       = DateTime.UtcNow,
            CreatedDate     = DateTime.UtcNow,
            SubTotal        = subTotal,
            Tax             = taxAmount,
            Discount        = discount + redemptionDiscount,
            GrandTotal      = grandTotal + (isDelivery ? dto.DeliveryCharge : 0),
            OrderType       = orderType,
            DeliveryAddress = dto.DeliveryAddress?.Trim(),
            DeliveryCharge  = isDelivery ? dto.DeliveryCharge : 0,
            Status          = orderStatus,
            OrderItems     = lineItems.Select(x => new OrderItem
            {
                FoodItemId = x.FoodItem.Id,
                Price      = x.EffectivePrice,
                Quantity   = x.Quantity,
                LineTotal  = x.EffectivePrice * x.Quantity,
                AddOnNotes = x.AddOnNotes
            }).ToList()
        };

        // 9. Deduct sold quantities from stock.
        foreach (var (foodItem, quantity, _, _) in lineItems)
        {
            foodItem.AvailableQuantity -= quantity;
            _foodItemRepository.Update(foodItem);
        }

        // 10. Persist order + items + stock updates in a single SaveChanges call (one transaction).
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        // 10a. Auto-record ledger entry for takeaway orders (completed immediately).
        if (order.Status == OrderStatus.Completed)
            await RecordOrderLedgerEntryAsync(order);

        // 10b-delivery. Auto-create a DeliveryOrder record for delivery orders.
        if (isDelivery && !string.IsNullOrWhiteSpace(dto.DeliveryAddress))
        {
            var deliveryOrder = new DeliveryOrder
            {
                OrderId         = order.Id,
                DeliveryAddress = dto.DeliveryAddress.Trim(),
                DeliveryCharge  = dto.DeliveryCharge,
                CustomerPhone   = dto.CustomerPhone?.Trim(),
                Status          = DeliveryStatus.Pending,
                CreatedDate     = DateTime.UtcNow,
            };
            await _deliveryRepository.AddDeliveryOrderAsync(deliveryOrder);
            await _deliveryRepository.SaveChangesAsync();
        }

        // 10b. Handle loyalty points after order is saved (OrderId is now available).
        if (customer != null)
        {
            var pointsEarned = (int)Math.Floor(grandTotal * settings.PointsPerRupee);
            order.PointsEarned = pointsEarned;

            if (dto.PointsRedeemed > 0)
            {
                customer.LoyaltyPoints -= dto.PointsRedeemed;
                await _customerRepository.AddTransactionAsync(new PointsTransaction
                {
                    CustomerId  = customer.Id,
                    Points      = -dto.PointsRedeemed,
                    Type        = "Redeem",
                    Description = $"Redeemed {dto.PointsRedeemed} pts on order {orderNumber}",
                    OrderId     = order.Id,
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (pointsEarned > 0)
            {
                customer.LoyaltyPoints += pointsEarned;
                await _customerRepository.AddTransactionAsync(new PointsTransaction
                {
                    CustomerId  = customer.Id,
                    Points      = pointsEarned,
                    Type        = "Earn",
                    Description = $"Earned {pointsEarned} pts on order {orderNumber}",
                    OrderId     = order.Id,
                    CreatedDate = DateTime.UtcNow
                });
            }

            customer.TotalSpend  += grandTotal;
            customer.TotalVisits += 1;
            await _customerRepository.UpdateAsync(customer);
        }

        // 11. Send SMS + WhatsApp confirmation for takeaway/delivery orders with a phone number.
        if ((isTakeaway || isDelivery) && !string.IsNullOrWhiteSpace(dto.CustomerPhone))
        {
            var itemsSummary = string.Join(", ", lineItems.Select(x => $"{x.FoodItem.ItemName} x{x.Quantity}"));
            _ = _smsService.SendOrderConfirmationAsync(
                dto.CustomerPhone, orderNumber, itemsSummary, grandTotal, settings.RestaurantName);
            _ = _whatsAppService.SendOrderConfirmationAsync(
                dto.CustomerPhone, orderNumber, itemsSummary, grandTotal, settings.RestaurantName);
        }

        // 12. Build and return the receipt DTO.
        return BuildReceiptFromLineItems(order, lineItems, cashier, settings, customer);
    }

    public async Task<IReadOnlyList<OrderDto>> GetPendingOrdersAsync()
    {
        var orders = await _orderRepository.GetAllWithDetailsAsync();
        var pending = orders.Where(o =>
            o.Status == OrderStatus.Pending ||
            o.Status == OrderStatus.KitchenReady ||
            o.Status == OrderStatus.BillPending).ToList();
        return _mapper.Map<IReadOnlyList<OrderDto>>(pending);
    }

    public async Task<OrderDto> AddItemsToOrderAsync(int orderId, AddItemsToOrderDto dto)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status != OrderStatus.Pending &&
            order.Status != OrderStatus.KitchenReady)
            throw new AppException("Items can only be added to an open dine-in order.");

        // New items arriving while the order is KitchenReady means the kitchen
        // needs to prepare them — pull the order back to Preparing.
        if (order.Status == OrderStatus.KitchenReady)
            order.Status = OrderStatus.Pending;

        var consolidatedItems = dto.Items
            .GroupBy(i => i.FoodItemId)
            .Select(g => new OrderItemRequestDto
            {
                FoodItemId = g.Key,
                Quantity = g.Sum(i => i.Quantity),
                AddOnIds = g.First().AddOnIds
            })
            .ToList();

        foreach (var request in consolidatedItems)
        {
            var foodItem = await _foodItemRepository.GetByIdWithCategoryAsync(request.FoodItemId)
                ?? throw new NotFoundException("Food item", request.FoodItemId);

            if (!foodItem.IsAvailable)
                throw new AppException($"'{foodItem.ItemName}' is currently unavailable.");

            if (request.Quantity > foodItem.AvailableQuantity)
                throw new AppException(
                    $"Insufficient stock for '{foodItem.ItemName}'. " +
                    $"Available: {foodItem.AvailableQuantity}, Requested: {request.Quantity}.");

            var (addOnPrice, addOnNotes) = await ResolveAddOnsAsync(request.AddOnIds);
            var effectivePrice = foodItem.Price + addOnPrice;

            // Only merge with a non-delivered item of the same type.
            // If the previous entry for this food item was already delivered, add a fresh row
            // so the KDS can see it (delivered items are filtered out on the kitchen screen).
            var existing = order.OrderItems.FirstOrDefault(oi =>
                oi.FoodItemId == foodItem.Id && !oi.IsDelivered);
            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                existing.LineTotal = existing.Price * existing.Quantity;
            }
            else
            {
                order.OrderItems.Add(new OrderItem
                {
                    FoodItemId = foodItem.Id,
                    Price = effectivePrice,
                    Quantity = request.Quantity,
                    LineTotal = effectivePrice * request.Quantity,
                    AddOnNotes = addOnNotes
                });
            }

            foodItem.AvailableQuantity -= request.Quantity;
            _foodItemRepository.Update(foodItem);
        }

        var settings = await _settingsRepository.GetSettingsAsync()
            ?? throw new AppException("Restaurant settings are not configured.");

        order.SubTotal = order.OrderItems.Sum(oi => oi.LineTotal);
        order.Tax = Math.Round(order.SubTotal * settings.TaxPercentage / 100, 2, MidpointRounding.AwayFromZero);
        if (dto.Discount.HasValue) order.Discount = dto.Discount.Value;
        order.GrandTotal = order.SubTotal + order.Tax - order.Discount;

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<ReceiptDto> GenerateBillAsync(int orderId, decimal? discount = null)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status != OrderStatus.Pending &&
            order.Status != OrderStatus.KitchenReady &&
            order.Status != OrderStatus.BillPending)
            throw new AppException("Only open orders can be finalized.");

        var cashier = order.CashierId.HasValue
            ? await _userRepository.GetByIdAsync(order.CashierId.Value)
            : null;

        var settings = await _settingsRepository.GetSettingsAsync()
            ?? throw new AppException("Restaurant settings are not configured.");

        // Allow cashier/admin to override discount at bill time.
        if (discount.HasValue && discount.Value >= 0)
        {
            order.Discount = discount.Value;
            order.GrandTotal = order.SubTotal + order.Tax - order.Discount;
        }

        order.Status = OrderStatus.BillPending;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return new ReceiptDto
        {
            OrderId = order.Id,
            RestaurantName = settings.RestaurantName,
            Address = settings.Address,
            Phone = settings.Phone,
            GSTNumber = settings.GSTNumber,
            UpiId = settings.UpiId,
            OrderNumber = order.OrderNumber,
            TableNumber = order.Table?.TableNumber,
            OrderDate = order.OrderDate,
            CashierName = cashier?.Name,
            CustomerPhone = order.CustomerPhone,
            Items = order.OrderItems.Select(oi => new ReceiptItemDto
            {
                ItemName = oi.FoodItem?.ItemName ?? "Unknown",
                Quantity = oi.Quantity,
                UnitPrice = oi.Price,
                LineTotal = oi.LineTotal
            }).ToList(),
            SubTotal = order.SubTotal,
            TaxPercentage = settings.TaxPercentage,
            Tax = order.Tax,
            Discount = order.Discount,
            GrandTotal = order.GrandTotal,
            FooterMessage = AppConstants.DefaultFooterMessage
        };
    }

    public async Task ConfirmPaymentAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status != OrderStatus.BillPending)
            throw new AppException("Payment can only be confirmed for orders awaiting payment.");

        order.Status = OrderStatus.Completed;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        // Auto-record ledger entry when dine-in payment is confirmed.
        await RecordOrderLedgerEntryAsync(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(int? branchId = null)
    {
        var orders = await _orderRepository.GetAllWithDetailsAsync();
        if (branchId.HasValue)
            orders = orders.Where(o => o.BranchId == branchId.Value).ToList();
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersByUserAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserAsync(userId);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<OrderDto> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException("Order", id);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task CancelOrderAsync(int orderId, int requestingUserId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Order is already cancelled.");

        // Mark cancelled and restore stock for each line item.
        order.Status = OrderStatus.Cancelled;

        foreach (var item in order.OrderItems)
        {
            var foodItem = await _foodItemRepository.GetByIdAsync(item.FoodItemId);
            if (foodItem is not null)
            {
                foodItem.AvailableQuantity += item.Quantity;
                _foodItemRepository.Update(foodItem);
            }
        }

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        // Send SMS + WhatsApp cancellation for takeaway orders with a phone number.
        if (order.TableId is null && !string.IsNullOrWhiteSpace(order.CustomerPhone))
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            var name = settings?.RestaurantName ?? "Restaurant";
            _ = _smsService.SendOrderCancellationAsync(
                order.CustomerPhone, order.OrderNumber, order.GrandTotal, name);
            _ = _whatsAppService.SendOrderCancellationAsync(
                order.CustomerPhone, order.OrderNumber, order.GrandTotal, name);
        }
    }

    public async Task<(bool orderCancelled, List<string> removedItemNames)> RejectOrderItemsAsync(
        int orderId, List<int> foodItemIds, string? reason)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Order is already cancelled.");

        var toRemove = order.OrderItems
            .Where(oi => foodItemIds.Contains(oi.FoodItemId) && !oi.IsDelivered)
            .ToList();

        if (!toRemove.Any())
            throw new AppException("None of the specified items were found in this order.");

        var removedNames = toRemove
            .Select(oi => $"{oi.FoodItem.ItemName} ×{oi.Quantity}")
            .ToList();

        // Restore inventory for each removed item
        foreach (var item in toRemove)
        {
            var foodItem = await _foodItemRepository.GetByIdAsync(item.FoodItemId);
            if (foodItem is not null)
            {
                foodItem.AvailableQuantity += item.Quantity;
                _foodItemRepository.Update(foodItem);
            }
        }

        _orderRepository.RemoveOrderItems(toRemove);

        var remaining = order.OrderItems.Except(toRemove).ToList();

        bool orderCancelled;
        if (!remaining.Any())
        {
            // No items left — cancel the whole order
            order.Status = OrderStatus.Cancelled;
            order.SubTotal   = 0;
            order.Tax        = 0;
            order.GrandTotal = 0;
            _orderRepository.Update(order);
            orderCancelled = true;
        }
        else
        {
            // Recalculate totals from remaining items
            var settings = await _settingsRepository.GetSettingsAsync()
                ?? throw new AppException("Restaurant settings are not configured.");
            order.SubTotal   = remaining.Sum(oi => oi.LineTotal);
            order.Tax        = Math.Round(order.SubTotal * settings.TaxPercentage / 100, 2, MidpointRounding.AwayFromZero);
            order.GrandTotal = order.SubTotal + order.Tax - order.Discount;
            _orderRepository.Update(order);
            orderCancelled = false;
        }

        await _orderRepository.SaveChangesAsync();
        return (orderCancelled, removedNames);
    }

    public async Task KitchenRejectOrderAsync(int orderId, string? reason)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Order is already cancelled.");

        order.Status = OrderStatus.Cancelled;

        foreach (var item in order.OrderItems)
        {
            var foodItem = await _foodItemRepository.GetByIdAsync(item.FoodItemId);
            if (foodItem is not null)
            {
                foodItem.AvailableQuantity += item.Quantity;
                _foodItemRepository.Update(foodItem);
            }
        }

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();
    }

    public async Task TransferToTakeawayAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Cannot transfer a cancelled order.");

        order.TableId = null;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();
    }

    public async Task TransferTableAsync(int orderId, int newTableId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Cannot transfer a cancelled order.");

        var table = await _tableRepository.GetByIdAsync(newTableId)
            ?? throw new AppException($"Table {newTableId} not found.");

        if (!table.IsActive)
            throw new AppException($"Table {table.TableNumber} is inactive.");

        if (order.TableId == newTableId)
            throw new AppException($"Order is already assigned to Table {table.TableNumber}.");

        order.TableId = newTableId;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();
    }

    public async Task<OrderDto> MarkOrderReadyAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        // Mark only items that still need preparation (not yet kitchen-ready, not yet delivered).
        var toMark = order.OrderItems.Where(i => !i.IsDelivered && !i.IsKitchenReady).ToList();
        if (!toMark.Any())
            throw new AppException("No items to mark as ready.");

        foreach (var item in toMark)
            item.IsKitchenReady = true;

        order.Status = OrderStatus.KitchenReady;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> MarkOrderServedAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        // Pick up only the items the kitchen has already marked ready.
        var readyToPickUp = order.OrderItems.Where(i => i.IsKitchenReady && !i.IsDelivered).ToList();
        if (!readyToPickUp.Any())
            throw new AppException("No kitchen-ready items available for pickup.");

        foreach (var item in readyToPickUp)
            item.IsDelivered = true;

        // Reset to Pending — new items may still be in preparation, or the table may order again.
        order.Status = OrderStatus.Pending;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task ReopenOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status != OrderStatus.Cancelled)
            throw new AppException("Only cancelled orders can be reopened.");

        // Re-deduct stock for every line item.
        foreach (var item in order.OrderItems)
        {
            var foodItem = await _foodItemRepository.GetByIdAsync(item.FoodItemId);
            if (foodItem is not null)
            {
                if (item.Quantity > foodItem.AvailableQuantity)
                    throw new AppException(
                        $"Cannot reopen: insufficient stock for '{foodItem.ItemName}'. " +
                        $"Available: {foodItem.AvailableQuantity}, Required: {item.Quantity}.");

                foodItem.AvailableQuantity -= item.Quantity;
                _foodItemRepository.Update(foodItem);
            }
        }

        order.Status = OrderStatus.Pending;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status == OrderStatus.Pending ||
            order.Status == OrderStatus.KitchenReady ||
            order.Status == OrderStatus.BillPending)
            throw new AppException("Active orders cannot be deleted. Cancel the order first.");

        _orderRepository.Delete(order);
        await _orderRepository.SaveChangesAsync();
    }

    private static ReceiptDto BuildReceiptFromLineItems(
        Order order,
        List<(FoodItem FoodItem, int Quantity, decimal EffectivePrice, string? AddOnNotes)> lineItems,
        User? cashier,
        Domain.Entities.Settings settings,
        Customer? customer = null)
    {
        return new ReceiptDto
        {
            OrderId = order.Id,
            RestaurantName = settings.RestaurantName,
            Address = settings.Address,
            Phone = settings.Phone,
            GSTNumber = settings.GSTNumber,

            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            CashierName = cashier?.Name ?? "Online Order",
            CustomerPhone = order.CustomerPhone,

            Items = lineItems.Select(x => new ReceiptItemDto
            {
                ItemName  = x.FoodItem.ItemName,
                Quantity  = x.Quantity,
                UnitPrice = x.EffectivePrice,
                LineTotal = x.EffectivePrice * x.Quantity,
                AddOnNotes = x.AddOnNotes
            }).ToList(),

            SubTotal = order.SubTotal,
            TaxPercentage = settings.TaxPercentage,
            Tax = order.Tax,
            Discount = order.Discount,
            GrandTotal = order.GrandTotal,

            CustomerName     = customer?.Name,
            CustomerPhone2   = customer?.Phone,
            CustomerTier     = customer?.Tier,
            PointsEarned     = order.PointsEarned,
            PointsRedeemed   = order.PointsRedeemed,
            NewPointsBalance = customer?.LoyaltyPoints ?? 0,

            FooterMessage = AppConstants.DefaultFooterMessage
        };
    }

    private async Task RecordOrderLedgerEntryAsync(Order order)
    {
        var tableInfo = order.TableId.HasValue
            ? $"Table {order.Table?.TableNumber.ToString() ?? order.TableId.ToString()}"
            : "Takeaway";
        await _ledgerRepository.AddAsync(new Domain.Entities.LedgerEntry
        {
            Date        = DateTime.UtcNow,
            Type        = "Credit",
            Amount      = order.GrandTotal,
            Category    = "Order Sale",
            Note        = $"Order #{order.OrderNumber} – {tableInfo}",
            CreatedById = order.CashierId,
            CreatedDate = DateTime.UtcNow,

        });
        await _ledgerRepository.SaveChangesAsync();
    }

    private async Task<(decimal Price, string? Notes)> ResolveAddOnsAsync(List<int> addOnIds)
    {
        if (addOnIds.Count == 0) return (0m, null);

        var names = new List<string>();
        var total = 0m;

        foreach (var id in addOnIds)
        {
            var addOn = await _addOnRepository.GetByIdAsync(id);
            if (addOn is { IsAvailable: true })
            {
                total += addOn.Price;
                names.Add(addOn.Name);
            }
        }

        return (total, names.Count > 0 ? string.Join(", ", names) : null);
    }

    // ── Public (unauthenticated) online order ──────────────────────────────────
    public async Task<PublicOrderConfirmationDto> PlacePublicOrderAsync(PublicOrderDto dto)
    {
        var settings = await _settingsRepository.GetSettingsAsync()
            ?? throw new AppException("Restaurant settings are not configured.");

        // Consolidate duplicate items.
        var consolidatedItems = dto.Items
            .GroupBy(i => i.FoodItemId)
            .Select(g => new { FoodItemId = g.Key, Quantity = g.Sum(x => x.Quantity), g.First().AddOnIds })
            .ToList();

        var lineItems = new List<(FoodItem FoodItem, int Quantity, decimal EffectivePrice, string? AddOnNotes)>();
        foreach (var req in consolidatedItems)
        {
            // Anonymous caller — look up scoped to the claimed branch explicitly rather
            // than trusting ambient claims (there are none) or a bare unscoped lookup.
            var foodItem = await _foodItemRepository.GetByIdWithCategoryForBranchAsync(req.FoodItemId, dto.BranchId)
                ?? throw new AppException($"Food item {req.FoodItemId} not found.");

            if (!foodItem.IsAvailable)
                throw new AppException($"'{foodItem.ItemName}' is currently unavailable.");

            if (req.Quantity > foodItem.AvailableQuantity)
                throw new AppException($"Insufficient stock for '{foodItem.ItemName}'. Available: {foodItem.AvailableQuantity}.");

            var (addOnPrice, addOnNotes) = await ResolveAddOnsAsync(req.AddOnIds);
            lineItems.Add((foodItem, req.Quantity, foodItem.Price + addOnPrice, addOnNotes));
        }

        var subTotal  = lineItems.Sum(x => x.EffectivePrice * x.Quantity);
        var taxAmount = Math.Round(subTotal * settings.TaxPercentage / 100, 2, MidpointRounding.AwayFromZero);
        var grandTotal = subTotal + taxAmount;

        var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

        var orderType = Enum.TryParse<OrderType>(dto.OrderType, out var parsed) ? parsed : OrderType.Delivery;
        var isDelivery = orderType == OrderType.Delivery;

        if (dto.TableId.HasValue)
        {
            var table = await _tableRepository.GetByIdForBranchAsync(dto.TableId.Value, dto.BranchId)
                ?? throw new AppException($"Table {dto.TableId.Value} not found.");
            if (!table.IsActive)
                throw new AppException($"Table {table.TableNumber} is inactive.");
        }

        var order = new Order
        {
            BranchId        = dto.BranchId,
            OrderNumber     = orderNumber,
            CashierId       = null,
            TableId         = dto.TableId,
            CustomerPhone   = dto.CustomerPhone?.Trim(),
            OrderDate       = DateTime.UtcNow,
            CreatedDate     = DateTime.UtcNow,
            SubTotal        = subTotal,
            Tax             = taxAmount,
            Discount        = 0,
            GrandTotal      = grandTotal + (isDelivery ? dto.DeliveryCharge : 0),
            OrderType       = orderType,
            DeliveryAddress = dto.DeliveryAddress?.Trim(),
            DeliveryCharge  = isDelivery ? dto.DeliveryCharge : 0,
            Status          = OrderStatus.Pending,
            OrderItems      = lineItems.Select(x => new OrderItem
            {
                FoodItemId = x.FoodItem.Id,
                Price      = x.EffectivePrice,
                Quantity   = x.Quantity,
                LineTotal  = x.EffectivePrice * x.Quantity,
                AddOnNotes = x.AddOnNotes
            }).ToList()
        };

        foreach (var (foodItem, quantity, _, _) in lineItems)
        {
            foodItem.AvailableQuantity -= quantity;
            _foodItemRepository.Update(foodItem);
        }

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        if (isDelivery && !string.IsNullOrWhiteSpace(dto.DeliveryAddress))
        {
            var deliveryOrder = new DeliveryOrder
            {
                OrderId         = order.Id,
                DeliveryAddress = dto.DeliveryAddress.Trim(),
                DeliveryCharge  = dto.DeliveryCharge,
                CustomerPhone   = dto.CustomerPhone?.Trim(),
                Status          = DeliveryStatus.Pending,
                CreatedDate     = DateTime.UtcNow,
            };
            await _deliveryRepository.AddDeliveryOrderAsync(deliveryOrder);
            await _deliveryRepository.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(dto.CustomerPhone))
        {
            var itemsSummary = string.Join(", ", lineItems.Select(x => $"{x.FoodItem.ItemName} x{x.Quantity}"));
            _ = _smsService.SendOrderConfirmationAsync(
                dto.CustomerPhone, orderNumber, itemsSummary, grandTotal, settings.RestaurantName);
            _ = _whatsAppService.SendOrderConfirmationAsync(
                dto.CustomerPhone, orderNumber, itemsSummary, grandTotal, settings.RestaurantName);
        }

        return new PublicOrderConfirmationDto
        {
            OrderNumber   = orderNumber,
            OrderId       = order.Id,
            GrandTotal    = order.GrandTotal,
            OrderType     = orderType.ToString(),
            CustomerName  = dto.CustomerName,
            EstimatedTime = isDelivery ? "30-45 minutes" : null,
        };
    }
}
