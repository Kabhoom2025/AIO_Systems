using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IDeliveryRepository
{
    Task<List<Delivery>> GetAllByOrgAsync(int orgId);
    Task<Delivery?> GetByIdAsync(int id);
    Task<Sale?> GetSaleAsync(int saleId);
    void Add(Delivery delivery);
    Task SaveChangesAsync();
}
