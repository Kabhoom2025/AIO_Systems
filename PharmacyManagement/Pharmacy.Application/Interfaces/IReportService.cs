using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IReportService
{
    Task<ProfitLossDto> GetProfitLossAsync(int orgId, DateTime from, DateTime to);
    Task<GstReportDto> GetGstReportAsync(int orgId, DateTime from, DateTime to);
    Task<List<CashBookEntryDto>> GetCashBookAsync(int orgId, DateTime from, DateTime to);
    Task<SalesSummaryDto> GetSalesSummaryAsync(int orgId, DateTime from, DateTime to);
    Task<List<TopMedicineDto>> GetTopMedicinesAsync(int orgId, DateTime from, DateTime to, int take = 10);
    Task<InventoryValuationReportDto> GetInventoryValuationAsync(int orgId);
}
