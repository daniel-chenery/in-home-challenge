using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TT.Deliveries.Data.Models;

namespace TT.Deliveries.Data.Repositories
{
    public interface IRepository<TId, TModel>
        where TModel : DataModel<TId>
    {
        public Task<IEnumerable<TModel>> GetAllAsync();

        public Task<TModel> GetAsync(TId id);

        public Task<TModel> GetAsync(Expression<Func<TModel, bool>> expression);

        public Task InsertAsync(TModel model);

        public Task UpdateAsync(TModel model);

        public Task DeleteAsync(TId id);
    }
}