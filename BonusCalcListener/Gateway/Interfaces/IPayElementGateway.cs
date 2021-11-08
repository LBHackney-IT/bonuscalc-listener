using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway
{
    public interface IPayElementGateway
    {
        Task<PayElement> GetEntityAsync(Guid id);
        Task<IEnumerable<PayElement>> GetReactiveRepairsPayElementsByWorkOrderId(string workOrderId);
        Task SaveEntityAsync(PayElement entity);
    }
}
