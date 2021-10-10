using System;
using System.Collections.Generic;
using System.Text;

namespace TT.Deliveries.Business.Services
{
    public interface IOrderNumberService
    {
        string CreateOrderNumber(string sender);
    }
}