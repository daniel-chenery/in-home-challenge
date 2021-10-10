using System.Data;

namespace TT.Deliveries.Data
{
    public interface IConnectionProvider
    {
        IDbConnection GetDbConnection();
    }
}