using Microsoft.Extensions.Logging;
using Moq;
using System;
using TT.Deliveries.Core;

namespace TT.Deliveries.Business.Tests
{
    public static class TestHelper
    {
        public static Mock<ILogger<TClass>> CreateLogger<TClass>()
        {
            var mock = new Mock<ILogger<TClass>>();

            mock
                .Setup(l => l.IsEnabled(It.IsAny<LogLevel>()))
                .Returns(true);

            mock
                .Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((l, e, t, ex, fn) => Console.WriteLine(fn(t, ex)));

            return mock;
        }

        public static void FreezeClock()
        {
            var now = DateTimeOffset.UtcNow.UtcDateTime.Date;
            Clock.SetUtc(() => now);
        }
    }
}