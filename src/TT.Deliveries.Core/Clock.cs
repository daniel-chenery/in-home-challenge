using System;

namespace TT.Deliveries.Core
{
    public static class Clock
    {
        private readonly static Func<DateTimeOffset> _defaultProvider = () => DateTimeOffset.UtcNow;
        private static Func<DateTimeOffset> _provider = _defaultProvider;

        public static void SetUtc(Func<DateTimeOffset> provider) => _provider = provider;

        public static void ResetProvider() => _provider = _defaultProvider;

        public static DateTimeOffset UtcNow => _provider();

        public static DateTimeOffset Now => _provider().ToLocalTime();
    }
}