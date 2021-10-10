using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TT.Deliveries.Business;
using TT.Deliveries.Business.Models;
using TT.Deliveries.Business.Services;
using TT.Deliveries.Core.Models;
using TT.Deliveries.Web.Api.Controllers;
using TT.Deliveries.Web.Api.Models;

namespace TT.Deliveries.Tests.Controllers
{
    [TestFixture]
    public class DeliveriesControllerTests
    {
        private Mock<IDeliveryService> _mockDeliveryService;
        private Mock<IAuthenticationService> _mockAuthenticationService;

        [SetUp]
        public void Setup()
        {
            _mockDeliveryService = new Mock<IDeliveryService>();
            _mockAuthenticationService = new Mock<IAuthenticationService>();
        }

        [Test]
        public async Task GetById_Should_Return_404_If_Delivery_Doesnt_Exist()
        {
            // arrange
            var controller = GetController();
            var deliveryId = Guid.NewGuid();

            _mockDeliveryService
                .Setup(ds => ds.GetAsync(deliveryId))
                .ThrowsAsync(new DeliveryException("Unit Test error"));

            // act
            var result = await controller.Get(deliveryId);

            // assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
                Assert.That(result.Value, Is.Null);
            });
        }

        [Test]
        public async Task GetById_Should_Return_Delivery_Details()
        {
            // arrange
            var faker = new Faker();
            var controller = GetController();
            var deliveryId = Guid.NewGuid();

            var delivery = new Delivery
            {
                Id = deliveryId,
                AccessWindow = new AccessWindow(DateTime.Now, faker.Date.Future()),
                Order = new Order(faker.Random.AlphaNumeric(16), faker.Company.CompanyName()),
                Recipient = new Recipient(faker.Person.FirstName, faker.Person.Address.Street, faker.Person.Email, faker.Person.Phone),
                State = new Faker().PickRandom<DeliveryState>()
            };

            _mockDeliveryService
                .Setup(ds => ds.GetAsync(deliveryId))
                .ReturnsAsync(delivery);

            // act
            var result = await controller.Get(deliveryId);

            // assert
            Assert.That(result.Value, Is.InstanceOf<ApiResponse<Delivery>>()
                .With.Property(nameof(ApiResponse<Delivery>.Result)).SameAs(delivery));
        }

        private DeliveriesController GetController() => new(
            _mockDeliveryService.Object,
            _mockAuthenticationService.Object,
            Mock.Of<ILogger<DeliveriesController>>());
    }
}