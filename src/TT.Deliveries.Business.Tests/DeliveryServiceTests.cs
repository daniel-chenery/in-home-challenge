using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using TT.Deliveries.Business.Models;
using TT.Deliveries.Business.Services;
using TT.Deliveries.Core;
using TT.Deliveries.Core.Models;
using TT.Deliveries.Data.Repositories;

namespace TT.Deliveries.Business.Tests
{
    internal class DeliveryServiceTests
    {
        private const int DefaultDeliveryDays = 7;

        private Mock<IRepository<Guid, Data.Models.Delivery>> _mockDeliveryRepository;
        private Mock<IRepository<Guid, Data.Models.Recipient>> _mockRecipientRepository;
        private Mock<IRepository<Guid, Data.Models.RecipientDelievery>> _mockRecipientDeliveryRepository;
        private Mock<IRepository<Guid, Data.Models.AccessWindow>> _mockAccessWindowRepository;
        private Mock<IRepository<string, Data.Models.Order>> _mockOrderRepository;
        private Mock<IOrderNumberService> _mockOrderNumberService;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<DeliveryService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockDeliveryRepository = new Mock<IRepository<Guid, Data.Models.Delivery>>();
            _mockRecipientRepository = new Mock<IRepository<Guid, Data.Models.Recipient>>();
            _mockRecipientDeliveryRepository = new Mock<IRepository<Guid, Data.Models.RecipientDelievery>>();
            _mockAccessWindowRepository = new Mock<IRepository<Guid, Data.Models.AccessWindow>>();
            _mockOrderRepository = new Mock<IRepository<string, Data.Models.Order>>();
            _mockOrderNumberService = new Mock<IOrderNumberService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = TestHelper.CreateLogger<DeliveryService>();
        }

        [Test]
        public async Task Create_Inserts_Into_Database([Values] DeliveryState state)
        {
            // arrange
            const string recipientName = "Unit Test";
            const string recipientEmail = "unit.test@tdd.com";
            const string recipientAddress = "123, Unit Test, NUnit";
            const string recipientPhone = "0123 456 789";
            const string orderNumber = "ORD123456";
            var recipientId = Guid.NewGuid();
            var senderName = "OrderSender";
            var deliveryId = default(Guid);
            var service = GetService();
            var model = new CreateDelivery
            {
                RecipientId = recipientId,
                State = state,
                SenderName = senderName
            };

            TestHelper.FreezeClock();
            MockMapper();

            _mockDeliveryRepository
                .Setup(dr => dr.InsertAsync(It.Is<Data.Models.Delivery>(d => d.State == state)))
                .Returns(Task.CompletedTask)
                .Callback<Data.Models.Delivery>(d => deliveryId = d.Id)
                .Verifiable();

            _mockAccessWindowRepository
                .Setup(awr => awr.InsertAsync(It.Is<Data.Models.AccessWindow>(aw =>
                        aw.DeliveryId == deliveryId &&
                        aw.StartTime == Clock.UtcNow.DateTime &&
                        aw.EndTime == Clock.UtcNow.AddDays(DefaultDeliveryDays).DateTime
                    )))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockOrderRepository
                .Setup(or => or.InsertAsync(It.Is<Data.Models.Order>(o =>
                    o.DeliveryId == deliveryId &&
                    o.Sender == senderName)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockRecipientRepository
                .Setup(rr => rr.GetAsync(recipientId))
                .ReturnsAsync(new Data.Models.Recipient
                {
                    Id = recipientId,
                    Name = recipientName,
                    Email = recipientEmail,
                    Address = recipientAddress,
                    PhoneNumber = recipientPhone
                })
                .Verifiable();

            _mockRecipientDeliveryRepository
                .Setup(rdr => rdr.InsertAsync(It.Is<Data.Models.RecipientDelievery>(rd =>
                    rd.DeliveryId == deliveryId &&
                    rd.RecipientId == recipientId
                )))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockOrderNumberService
                .Setup(ons => ons.CreateOrderNumber(It.IsAny<string>()))
                .Returns(orderNumber);

            var expectedResult = new Delivery
            {
                Id = deliveryId,
                State = state,
            };

            // act
            var delivery = await service.CreateAsync(model);

            // assert
            _mockDeliveryRepository.VerifyAll();
            _mockAccessWindowRepository.VerifyAll();
            _mockOrderRepository.VerifyAll();
            _mockRecipientDeliveryRepository.VerifyAll();
            _mockRecipientRepository.VerifyAll();

            Assert.That(delivery.AccessWindow, Is.Not.Null);
            Assert.That(delivery.Order, Is.Not.Null);
            Assert.That(delivery.Recipient, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(delivery.Id, Is.EqualTo(deliveryId));
                Assert.That(delivery.State, Is.EqualTo(state));
                Assert.That(delivery.AccessWindow.StartTime, Is.EqualTo(Clock.UtcNow.DateTime));
                Assert.That(delivery.AccessWindow.EndTime, Is.EqualTo(Clock.UtcNow.AddDays(DefaultDeliveryDays).DateTime));
                Assert.That(delivery.Order.OrderNumber, Is.EqualTo(orderNumber));
                Assert.That(delivery.Order.Sender, Is.EqualTo(senderName));
                Assert.That(delivery.Recipient.Name, Is.EqualTo(recipientName));
                Assert.That(delivery.Recipient.Email, Is.EqualTo(recipientEmail));
                Assert.That(delivery.Recipient.Address, Is.EqualTo(recipientAddress));
                Assert.That(delivery.Recipient.PhoneNumber, Is.EqualTo(recipientPhone));
            });
        }

        [Test]
        public void Create_DatabaseException_Is_Thrown_To_Caller()
        {
            // arrange
            var createdModel = new CreateDelivery
            {
                SenderName = "UnitTest",
                RecipientId = Guid.NewGuid(),
                State = DeliveryState.Created
            };
            var service = GetService();

            // act
            Task Act() => service.CreateAsync(createdModel);

            // assert
            Assert.That(Act, Throws.InstanceOf<DeliveryException>()
                .With.Property(nameof(DeliveryException.DeliveryId)).EqualTo(default(Guid)));
        }

        private IDeliveryService GetService() => new DeliveryService(
            _mockDeliveryRepository.Object,
            _mockAccessWindowRepository.Object,
            _mockOrderRepository.Object,
            _mockRecipientRepository.Object,
            _mockRecipientDeliveryRepository.Object,
            _mockOrderNumberService.Object,
            _mockMapper.Object,
            _mockLogger.Object);

        /// <summary>
        /// Configures the mapper.
        /// Whilst we could use the AutoMapper config, it isn't sensible to make a test reliant on a third-party tool
        /// </summary>
        private void MockMapper()
        {
            _mockMapper
                .Setup(m => m.Map<Data.Models.Delivery, Delivery>(It.IsAny<Data.Models.Delivery>()))
                .Returns<Data.Models.Delivery>(dm => new Delivery
                {
                    Id = dm.Id,
                    State = dm.State
                });

            _mockMapper
                .Setup(m => m.Map<Data.Models.AccessWindow, AccessWindow>(It.IsAny<Data.Models.AccessWindow>()))
                .Returns<Data.Models.AccessWindow>(dm => new AccessWindow(dm.StartTime, dm.EndTime));

            _mockMapper
                .Setup(m => m.Map<Data.Models.Recipient, Recipient>(It.IsAny<Data.Models.Recipient>()))
                .Returns<Data.Models.Recipient>(dm => new Recipient(dm.Name!, dm.Address!, dm.Email!, dm.PhoneNumber!));

            _mockMapper
                .Setup(m => m.Map<Data.Models.Order, Order>(It.IsAny<Data.Models.Order>()))
                .Returns<Data.Models.Order>(dm => new Order(dm.Id, dm.Sender!));
        }
    }
}