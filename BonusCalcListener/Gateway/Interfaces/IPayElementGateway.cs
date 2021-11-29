using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway
{
    public interface IPayElementGateway
    {
        Task<List<PayElement>> GetPayElementsByWorkOrderId(string workOrderId, int payElementType);
    }
}
