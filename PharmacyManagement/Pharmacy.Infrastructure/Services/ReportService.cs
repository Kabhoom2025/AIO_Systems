using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly PharmacyDbContext _ctx;

    public ReportService(PharmacyDbContext ctx) => _ctx = ctx;

    public async Task<ProfitLossDto> GetProfitLossAsync(int orgId, DateTime from, DateTime to)
    {
        var revenue = await _ctx.Sales
            .Where(s => s.OrganizationId == orgId && s.SaleDate >= from && s.SaleDate <= to)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

        var cogs = await _ctx.SaleItems
            .Where(si => si.Sale.OrganizationId == orgId && si.Sale.SaleDate >= from && si.Sale.SaleDate <= to)
            .SumAsync(si => (decimal?)(si.Quantity * si.MedicineBatch.PurchasePrice)) ?? 0;

        var expenses = await _ctx.Expenses
            .Where(e => e.OrganizationId == orgId && e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var grossProfit = revenue - cogs;

        return new ProfitLossDto
        {
            From = from,
            To = to,
            Revenue = revenue,
            CostOfGoodsSold = cogs,
            GrossProfit = grossProfit,
            Expenses = expenses,
            NetProfit = grossProfit - expenses
        };
    }

    public async Task<GstReportDto> GetGstReportAsync(int orgId, DateTime from, DateTime to)
    {
        var outputTax = await _ctx.Sales
            .Where(s => s.OrganizationId == orgId && s.SaleDate >= from && s.SaleDate <= to)
            .SumAsync(s => (decimal?)s.TaxAmount) ?? 0;

        var inputTax = await _ctx.GoodsReceiptItems
            .Where(i => i.GoodsReceipt.OrganizationId == orgId
                     && i.GoodsReceipt.ReceivedDate >= from && i.GoodsReceipt.ReceivedDate <= to)
            .SumAsync(i => (decimal?)(i.PurchasePrice * i.QuantityReceived * i.Medicine.GstPercent / 100)) ?? 0;

        return new GstReportDto
        {
            From = from,
            To = to,
            OutputTax = outputTax,
            InputTax = inputTax,
            NetGstPayable = outputTax - inputTax
        };
    }

    public async Task<List<CashBookEntryDto>> GetCashBookAsync(int orgId, DateTime from, DateTime to)
    {
        var cashSales = await _ctx.Sales
            .Where(s => s.OrganizationId == orgId && s.PaymentMethod == "Cash"
                     && s.SaleDate >= from && s.SaleDate <= to)
            .Select(s => new CashBookEntryDto
            {
                Date = s.SaleDate,
                Description = $"Sale {s.InvoiceNumber}",
                Type = "In",
                Amount = s.TotalAmount
            })
            .ToListAsync();

        var cashExpenses = await _ctx.Expenses
            .Where(e => e.OrganizationId == orgId && e.PaymentMethod == "Cash"
                     && e.ExpenseDate >= from && e.ExpenseDate <= to)
            .Select(e => new CashBookEntryDto
            {
                Date = e.ExpenseDate,
                Description = $"{e.Category} expense",
                Type = "Out",
                Amount = e.Amount
            })
            .ToListAsync();

        var entries = cashSales.Concat(cashExpenses).OrderBy(e => e.Date).ToList();

        var balance = 0m;
        foreach (var entry in entries)
        {
            balance += entry.Type == "In" ? entry.Amount : -entry.Amount;
            entry.RunningBalance = balance;
        }

        return entries;
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(int orgId, DateTime from, DateTime to)
    {
        var sales = await _ctx.Sales
            .Where(s => s.OrganizationId == orgId && s.SaleDate >= from && s.SaleDate <= to)
            .ToListAsync();

        var dailyTrend = sales
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new SalesTrendPointDto { Date = g.Key, Amount = g.Sum(s => s.TotalAmount) })
            .OrderBy(p => p.Date)
            .ToList();

        var byPaymentMethod = sales
            .GroupBy(s => s.PaymentMethod)
            .Select(g => new PaymentMethodBreakdownDto { Method = g.Key, Amount = g.Sum(s => s.TotalAmount) })
            .OrderByDescending(p => p.Amount)
            .ToList();

        var total = sales.Sum(s => s.TotalAmount);
        var count = sales.Count;

        return new SalesSummaryDto
        {
            From             = from,
            To               = to,
            TotalSales       = total,
            TransactionCount = count,
            AverageTicket    = count == 0 ? 0 : total / count,
            DailyTrend       = dailyTrend,
            ByPaymentMethod  = byPaymentMethod
        };
    }

    public async Task<List<TopMedicineDto>> GetTopMedicinesAsync(int orgId, DateTime from, DateTime to, int take = 10)
    {
        return await _ctx.SaleItems
            .Where(si => si.Sale.OrganizationId == orgId && si.Sale.SaleDate >= from && si.Sale.SaleDate <= to)
            .GroupBy(si => new { si.MedicineId, si.Medicine.Name })
            .Select(g => new TopMedicineDto
            {
                MedicineId   = g.Key.MedicineId,
                MedicineName = g.Key.Name,
                QuantitySold = g.Sum(si => si.Quantity),
                Revenue      = g.Sum(si => si.LineTotal)
            })
            .OrderByDescending(m => m.QuantitySold)
            .Take(take)
            .ToListAsync();
    }

    public async Task<InventoryValuationReportDto> GetInventoryValuationAsync(int orgId)
    {
        var medicines = await _ctx.Medicines
            .Include(m => m.Batches)
            .Where(m => m.OrganizationId == orgId)
            .ToListAsync();

        var items = medicines
            .Select(m =>
            {
                var stock = m.Batches.Sum(b => b.CurrentQuantity);
                return new InventoryValuationDto
                {
                    MedicineId    = m.Id,
                    MedicineName  = m.Name,
                    TotalStock    = stock,
                    PurchasePrice = m.PurchasePrice,
                    StockValue    = stock * m.PurchasePrice
                };
            })
            .OrderByDescending(i => i.StockValue)
            .ToList();

        return new InventoryValuationReportDto
        {
            Items      = items,
            GrandTotal = items.Sum(i => i.StockValue)
        };
    }
}
