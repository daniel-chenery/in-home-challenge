using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TT.Deliveries.Business.Models;
using TT.Deliveries.Business.Services;
using TT.Deliveries.Web.Api.Models;

namespace TT.Deliveries.Web.Api.Controllers
{
    [Route("deliveries")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<DeliveriesController> _logger;

        public DeliveriesController(
            IDeliveryService deliveryService,
            IAuthenticationService authenticationService,
            ILogger<DeliveriesController> logger)
        {
            _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Delivery>>> Create(CreateDelivery createDelivery)
        {
            try
            {
                var delivery = await _deliveryService.CreateAsync(createDelivery);
                return CreatedAtAction(nameof(Get), new { deliveryId = delivery.Id }, ApiResponse.Success(delivery));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to create delivery", ex);

                return ApiResponse.Failed<Delivery>(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{deliveryId}")]
        public async Task<ActionResult<ApiResponse<Delivery>>> Get(Guid deliveryId)
        {
            try
            {
                return ApiResponse.Success(await _deliveryService.GetAsync(deliveryId));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve delivery {deliveryId}", ex);

                return NotFound();
            }
        }

        [Authorize]
        [HttpPatch]
        public async Task<ActionResult<ApiResponse<Delivery>>> Update(UpdateDelivery updateDelivery)
        {
            try
            {
                if (!await _authenticationService.CanTransition(User, updateDelivery.State))
                {
                    _logger.LogError($"User {User.Identity?.Name} cannot transition delivery {updateDelivery.Guid} to {updateDelivery.State}");
                    return Forbid();
                }

                await _deliveryService.UpdateAsync(updateDelivery);

                return ApiResponse.Success(await _deliveryService.GetAsync(updateDelivery.Guid));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to transition delivery {updateDelivery.Guid}", ex);

                return ApiResponse.Failed<Delivery>(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{deliveryId}")]
        public async Task<IActionResult> Delete(Guid deliveryId)
        {
            try
            {
                await _deliveryService.DeleteAsync(deliveryId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to delete delivery {deliveryId}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}