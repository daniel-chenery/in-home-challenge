using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TT.Deliveries.Data.Models;

namespace TT.Deliveries.Data.Repositories
{
    public class Repository<TId, TModel> : IRepository<TId, TModel>
        where TModel : DataModel<TId>
    {
        private readonly IConnectionProvider _connectionProvider;

        public Repository(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public Task<IEnumerable<TModel>> GetAllAsync() => Query(con => con.GetAllAsync<TModel>());

        // DapperId tracking isn't working properly with GUIDs
        // public Task<TModel> GetAsync(TId id) => Query(con => con.GetAsync<TModel>(id));

        // As a workaround, query all
        public Task<TModel> GetAsync(TId id)
            => Query(async con => (await con.GetAllAsync<TModel>()).Single(m => m.Id!.Equals(id)));

        // This method is *extremely* ineffecient.  A proper repository layer (such as EF) would use a predicate.
        public Task<TModel> GetAsync(Expression<Func<TModel, bool>> expression)
            => Query(async con => (await con.GetAllAsync<TModel>()).Single(expression.Compile()));

        public Task InsertAsync(TModel model) => Execute(con => con.InsertAsync(model));

        public Task UpdateAsync(TModel model) => Execute(con => con.UpdateAsync(model));

        public async Task DeleteAsync(TId id) => await Execute(async con =>
        {
            var model = await con.GetAsync<TModel>(id);

            await con.DeleteAsync(model);
        });

        private async Task Execute(Func<IDbConnection, Task> action)
        {
            using var connection = _connectionProvider.GetDbConnection();

            try
            {
                await action(connection);
            }
            catch (DataException ex)
            {
                throw new DatabaseException<TModel>("Unable to execute query", ex);
            }
        }

        private Task<TResult> Query<TResult>(Func<IDbConnection, Task<TResult>> func)
        {
            using var connection = _connectionProvider.GetDbConnection();

            try
            {
                return func(connection);
            }
            catch (DataException ex)
            {
                throw new DatabaseException<TModel>("Unable to query database.", ex);
            }
        }
    }
}