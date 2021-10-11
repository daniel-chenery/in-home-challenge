using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TT.Deliveries.Business.Models;
using TT.Deliveries.Core;
using TT.Deliveries.Core.Models;
using TT.Deliveries.Data;
using TT.Deliveries.Data.Repositories;

namespace TT.Deliveries.Business.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IRepository<Guid, Data.Models.Delivery> _deliveryRepository;
        private readonly IRepository<Guid, Data.Models.AccessWindow> _accessWindowRepository;
        private readonly IRepository<string, Data.Models.Order> _orderRepository;
        private readonly IRepository<Guid, Data.Models.Recipient> _recipientRepository;
        private readonly IRepository<Guid, Data.Models.RecipientDelievery> _recipientDeliveryRepository;
        private readonly IOrderNumberService _orderNumberService;
        private readonly IMapper _mapper;
        private readonly ILogger<DeliveryService> _logger;

        public DeliveryService(
            IRepository<Guid, Data.Models.Delivery> deliveryRepository,
            IRepository<Guid, Data.Models.AccessWindow> accessWindowRepository,
            IRepository<string, Data.Models.Order> orderRepository,
            IRepository<Guid, Data.Models.Recipient> recipientRepository,
            IRepository<Guid, Data.Models.RecipientDelievery> recipientDeliveryRepository,
            IOrderNumberService orderNumberService,
            IMapper mapper,
            ILogger<DeliveryService> logger)
        {
            _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
            _accessWindowRepository = accessWindowRepository ?? throw new ArgumentNullException(nameof(accessWindowRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _recipientRepository = recipientRepository ?? throw new ArgumentNullException(nameof(recipientRepository));
            _recipientDeliveryRepository = recipientDeliveryRepository ?? throw new ArgumentNullException(nameof(recipientDeliveryRepository));
            _orderNumberService = orderNumberService ?? throw new ArgumentNullException(nameof(orderNumberService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Delivery> CreateAsync(CreateDelivery createDelivery)
        {
            _logger.LogDebug($"Creating delivery for {createDelivery.RecipientId}");

            if (string.IsNullOrWhiteSpace(createDelivery.SenderName))
            {
                throw new ArgumentException($"{nameof(createDelivery.SenderName)} cannot be null or empty.");
            }

            try
            {
                // Find the recipient
                var recipient = await _recipientRepository.GetAsync(createDelivery.RecipientId);

                if (recipient is null)
                {
                    throw new DeliveryException("Invalid recipient");
                }

                var delivery = new Data.Models.Delivery
                {
                    Id = Guid.NewGuid(),
                    State = createDelivery.State
                };

                // Insert the delivery
                await _deliveryRepository.InsertAsync(delivery);

                // Create the FKs
                await _recipientDeliveryRepository.InsertAsync(new Data.Models.RecipientDelievery
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    RecipientId = recipient.Id
                });

                var accessWindow = new Data.Models.AccessWindow
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    StartTime = Clock.UtcNow.DateTime,
                    EndTime = Clock.UtcNow.AddDays(7).DateTime // This should be provided from somewhere
                };

                await _accessWindowRepository.InsertAsync(accessWindow);

                var order = new Data.Models.Order
                {
                    Id = _orderNumberService.CreateOrderNumber(createDelivery.SenderName),
                    DeliveryId = delivery.Id,
                    Sender = createDelivery.SenderName
                };

                await _orderRepository.InsertAsync(order);

                // Covert to app models & return
                var result = _mapper.Map<Data.Models.Delivery, Delivery>(delivery);
                result.AccessWindow = _mapper.Map<Data.Models.AccessWindow, AccessWindow>(accessWindow);
                result.Recipient = _mapper.Map<Data.Models.Recipient, Recipient>(recipient);
                result.Order = _mapper.Map<Data.Models.Order, Order>(order);

                return result;
            }
            catch (DatabaseException<Data.Models.Recipient> ex)
            {
                _logger.LogError(ex, $"Unable to find recipient {createDelivery.RecipientId}");

                throw new DeliveryException($"Unable to find recipient {createDelivery.RecipientId}", ex);
            }
            catch (DatabaseException<Data.Models.Delivery> ex)
            {
                _logger.LogError(ex, $"Unable to create delivery for: {createDelivery.RecipientId}");

                throw new DeliveryException($"Unable to create delivery for: {createDelivery.RecipientId}", ex);
            }
            catch (DatabaseException<Data.Models.Order> ex)
            {
                _logger.LogError(ex, $"Unable to create order for: {createDelivery.SenderName}");

                throw new DeliveryException($"Unable to create order for: {createDelivery.SenderName}", ex);
            }
            catch (Exception ex) when (ex.GetType() != typeof(DeliveryException))
            {
                _logger.LogError(ex, "An unknown error occured. See the inner exception for details");
                throw new DeliveryException("An unknown error occured. See the inner exception for details", ex);
            }
        }

        public async Task DeleteAsync(Guid deliveryId)
        {
            try
            {
                _logger.LogDebug($"Retrieving delivery: {deliveryId}");
                await _deliveryRepository.DeleteAsync(deliveryId);
            }
            catch (DatabaseException<Data.Models.Delivery> ex)
            {
                _logger.LogError($"Unable to delete delivery: {deliveryId}", ex);

                throw new DeliveryException(deliveryId, $"Unable to delete delivery: {deliveryId}");
            }
        }

        public async Task<Delivery> GetAsync(Guid deliveryId)
        {
            try
            {
                _logger.LogDebug($"Retrieving delivery: {deliveryId}");
                var dataDelivery = await _deliveryRepository.GetAsync(deliveryId);
                var accessWindow = await _accessWindowRepository.GetAsync(aw => aw.DeliveryId == deliveryId);
                var recipientDelieveries = await _recipientDeliveryRepository.GetAsync(rdr => rdr.DeliveryId == deliveryId);
                var recipient = await _recipientRepository.GetAsync(recipientDelieveries.RecipientId);
                var order = await _orderRepository.GetAsync(o => o.DeliveryId == deliveryId);

                // Covert to app models & return
                var delivery = _mapper.Map<Data.Models.Delivery, Delivery>(dataDelivery);
                delivery.AccessWindow = _mapper.Map<Data.Models.AccessWindow, AccessWindow>(accessWindow);
                delivery.Recipient = _mapper.Map<Data.Models.Recipient, Recipient>(recipient);
                delivery.Order = _mapper.Map<Data.Models.Order, Order>(order);

                return delivery;
            }
            catch (DatabaseException<Data.Models.Delivery> ex)
            {
                _logger.LogError($"Unable to find delivery: {deliveryId}", ex);

                throw new DeliveryException(deliveryId, $"Unable to find delivery.");
            }
            catch (DatabaseException<Data.Models.AccessWindow> ex)
            {
                _logger.LogError(ex, $"Unable to find order access window for: {deliveryId}");

                throw new DeliveryException(deliveryId, "Unable to find access window.");
            }
            catch (DatabaseException<Data.Models.Recipient> ex)
            {
                _logger.LogError(ex, $"Unable to find recipient for delivery: {deliveryId}");

                throw new DeliveryException(deliveryId, "Unable to find recipient for delivery.");
            }
            catch (DatabaseException<Data.Models.Order> ex)
            {
                _logger.LogError(ex, $"Unable to find order for delivery: {deliveryId}");

                throw new DeliveryException(deliveryId, "Unable to find order.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error occured. See the inner exception for details");
                throw new DeliveryException("An unknown error occured. See the inner exception for details", ex);
            }
        }

        public async Task<IEnumerable<Delivery>> GetExpiredSinceAsync(DateTime expiration)
        {
            // As per other methods, this is bad and should be refactored to use predicates & joins

            var deliveries = (await _deliveryRepository.GetAllAsync()).Where(d => d.State != DeliveryState.Expired);
            var expiredAccessWindows = (await _accessWindowRepository.GetAllAsync())
                .Where(aw => aw.EndTime <= expiration);

            var expiredDeliveries = deliveries.Where(d => expiredAccessWindows.Any(aw => aw.DeliveryId == d.Id));

            return _mapper.Map<IEnumerable<Data.Models.Delivery>, IEnumerable<Delivery>>(expiredDeliveries);
        }

        public async Task UpdateAsync(UpdateDelivery updateDelivery)
        {
            try
            {
                var delivery = await _deliveryRepository.GetAsync(updateDelivery.Guid);

                if (delivery.State > updateDelivery.State)
                {
                    throw new DeliveryException(updateDelivery.Guid, "A delivery cannot transition backwards.");
                }

                if (updateDelivery.State == DeliveryState.Completed && delivery.State != DeliveryState.Approved)
                {
                    throw new DeliveryException(updateDelivery.Guid, "A delivery must be approved before it is completed.");
                }

                if (updateDelivery.State == DeliveryState.Cancelled && delivery.State == DeliveryState.Completed)
                {
                    throw new DeliveryException(updateDelivery.Guid, "A completed delivery cannot be cancelled.");
                }

                delivery.State = updateDelivery.State;
                await _deliveryRepository.UpdateAsync(delivery);
            }
            catch (DatabaseException<Data.Models.Delivery> ex)
            {
                _logger.LogError(ex, $"Unable to update delivery: {updateDelivery.Guid}");

                throw new DeliveryException(updateDelivery.Guid, "Unable to update the delivery state.");
            }
        }
    }
}