using System;
using System.Runtime.Serialization;
using TT.Deliveries.Data.Models;

namespace TT.Deliveries.Data
{
    public class DatabaseException<TModel> : Exception
        where TModel : DataModel
    {
        public DatabaseException()
        {
            Model = typeof(TModel);
        }

        public DatabaseException(string message)
            : base(message)
        {
            Model = typeof(TModel);
        }

        public DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
            Model = typeof(TModel);
        }

        protected DatabaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Model = typeof(TModel);
        }

        public Type Model { get; }
    }
}