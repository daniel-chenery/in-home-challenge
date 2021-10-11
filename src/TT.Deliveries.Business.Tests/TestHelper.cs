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
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                // https://github.com/moq/moq4/issues/918
                .Callback(new InvocationAction(inv =>
                {
                    var fn = inv.Arguments[4]
                        .GetType()
                        .GetMethod("Invoke");

                    var msg = fn?.Invoke(inv.Arguments[4], new[] { inv.Arguments[2], inv.Arguments[3] });

                    Console.WriteLine($"{inv.Arguments[0]} - {msg}");
                }));
            //.Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((l, e, t, ex, fn) => Console.WriteLine(fn(t, ex)));

            return mock;
        }

        public static void FreezeClock()
        {
            var now = DateTimeOffset.UtcNow.UtcDateTime.Date;
            Clock.SetUtc(() => now);
        }
    }
}