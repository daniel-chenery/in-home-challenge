using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TT.Deliveries.Core;
using TT.Deliveries.Core.Models;

namespace TT.Deliveries.Business.Services
{
    public class DeliveryExpirationService : IHostedService, IDisposable, IAsyncDisposable
    {
        private bool _disposedValue;
        private Timer? _timer;
        private readonly IDeliveryService _deliveryService;
        private readonly ILogger<DeliveryExpirationService> _logger;

        public DeliveryExpirationService(
            IDeliveryService deliveryService,
            ILogger<DeliveryExpirationService> logger
            )
        {
            _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Delivery Expiration Service.");

            // This should be generated from a factory for UnitTesting ease
            _timer = new Timer(async (_) => await CheckAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Delivery Expiration Service.");

            _timer?.Dispose();

            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                }

                _disposedValue = true;
            }
        }

        private async Task CheckAsync()
        {
            try
            {
                var expiredDeliveries = (await _deliveryService.GetExpiredSinceAsync(Clock.UtcNow.DateTime))
                    .ToList();

                _logger.LogInformation($"Found {expiredDeliveries.Count} deliveries to expire.");

                foreach (var expiredDelivery in expiredDeliveries)
                {
                    _logger.LogDebug($"Expiring delivery: {expiredDelivery.Id}");

                    await _deliveryService.UpdateAsync(new Models.UpdateDelivery(expiredDelivery.Id, DeliveryState.Expired));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to expire deliveries");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(false);

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            // Suppress finalization.
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }

        protected ValueTask DisposeAsyncCore() => _timer?.DisposeAsync() ?? default;
    }
}