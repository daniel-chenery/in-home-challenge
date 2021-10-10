using Dapper.Contrib.Extensions;

namespace TT.Deliveries.Data.Models
{
    public abstract class DataModel<TId> : DataModel
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [ExplicitKey]
        public TId Id { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}