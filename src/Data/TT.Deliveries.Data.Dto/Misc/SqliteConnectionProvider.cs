using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using TT.Deliveries.Core.Configuration;

namespace TT.Deliveries.Data
{
    /// <summary>
    /// Abstract out the DB connection so we can swap providers
    /// </summary>
    public class SqliteConnectionProvider : IConnectionProvider
    {
        private readonly IOptions<DatabaseOptions> _options;

        public SqliteConnectionProvider(IOptions<DatabaseOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_options.Value.ConnectionString))
            {
                throw new ArgumentException("ConnectionString cannot be null or empty.", nameof(options));
            }
        }

        public IDbConnection GetDbConnection() => new SqliteConnection(_options.Value.ConnectionString);
    }
}