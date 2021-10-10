using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TT.Deliveries.Business.Models;

namespace TT.Deliveries.Business.Services
{
    public interface IDeliveryService
    {
        Task<Delivery> CreateAsync(CreateDelivery delivery);

        Task UpdateAsync(UpdateDelivery delivery);

        Task<Delivery> GetAsync(Guid deliveryId);

        Task<IEnumerable<Delivery>> GetExpiredSinceAsync(DateTime expiration);

        Task DeleteAsync(Guid deliveryId);
    }
}