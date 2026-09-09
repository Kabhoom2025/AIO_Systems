using FoodOrder.Application.DTOs.Customer;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _service.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<CustomerDto>>.SuccessResult(customers));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _service.GetByIdAsync(id);
        if (customer is null)
            return NotFound(ApiResponse<object>.FailureResult($"Customer {id} not found."));
        return Ok(ApiResponse<CustomerDto>.SuccessResult(customer));
    }

    [HttpGet("phone/{phone}")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetByPhone(string phone)
    {
        var customer = await _service.GetByPhoneAsync(phone);
        if (customer is null)
            return NotFound(ApiResponse<object>.FailureResult($"No customer found for phone '{phone}'."));
        return Ok(ApiResponse<CustomerDto>.SuccessResult(customer));
    }

    [HttpGet("{id:int}/transactions")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetTransactions(int id)
    {
        var txns = await _service.GetTransactionsAsync(id);
        return Ok(ApiResponse<IReadOnlyList<PointsTransactionDto>>.SuccessResult(txns));
    }

    [HttpGet("{id:int}/orders")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetOrders(int id)
    {
        var orders = await _service.GetOrdersAsync(id);
        return Ok(ApiResponse<IReadOnlyList<CustomerOrderSummaryDto>>.SuccessResult(orders));
    }

    [HttpGet("{id:int}/addresses")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetAddresses(int id)
    {
        var addresses = await _service.GetAddressesAsync(id);
        return Ok(ApiResponse<IReadOnlyList<SavedAddressDto>>.SuccessResult(addresses));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest dto)
    {
        try
        {
            var customer = await _service.CreateAsync(dto);
            return Ok(ApiResponse<CustomerDto>.SuccessResult(customer));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest dto)
    {
        var customer = await _service.UpdateAsync(id, dto);
        if (customer is null)
            return NotFound(ApiResponse<object>.FailureResult($"Customer {id} not found."));
        return Ok(ApiResponse<CustomerDto>.SuccessResult(customer));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null));
    }

    [HttpPost("{id:int}/adjust-points")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdjustPoints(int id, [FromBody] AdjustPointsRequest dto)
    {
        var customer = await _service.AdjustPointsAsync(id, dto);
        if (customer is null)
            return NotFound(ApiResponse<object>.FailureResult($"Customer {id} not found."));
        return Ok(ApiResponse<CustomerDto>.SuccessResult(customer));
    }

    [HttpPost("{id:int}/addresses")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> AddAddress(int id, [FromBody] AddSavedAddressRequest dto)
    {
        var address = await _service.AddAddressAsync(id, dto);
        if (address is null)
            return NotFound(ApiResponse<object>.FailureResult($"Customer {id} not found."));
        return Ok(ApiResponse<SavedAddressDto>.SuccessResult(address));
    }

    [HttpDelete("{id:int}/addresses/{addressId:int}")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> DeleteAddress(int id, int addressId)
    {
        await _service.DeleteAddressAsync(id, addressId);
        return Ok(ApiResponse<object>.SuccessResult(null));
    }
}
