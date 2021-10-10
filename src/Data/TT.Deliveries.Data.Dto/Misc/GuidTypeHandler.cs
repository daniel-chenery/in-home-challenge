using Dapper;
using System;
using System.Data;

namespace TT.Deliveries.Data
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value) => Guid.Parse(value as string);

        public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();
    }
}