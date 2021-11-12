using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway
{
    public interface IPayElementGateway
    {
        Task<IEnumerable<PayElement>> GetPayElementsByWorkOrderId(string workOrderId, int payElementType);
    }
}
