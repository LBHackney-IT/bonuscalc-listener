using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway
{
    public class PayElementGateway : IPayElementGateway
    {
        public Task<PayElement> GetEntityAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PayElement>> GetReactiveRepairsPayElementsByWorkOrderId(string workOrderId)
        {
            throw new NotImplementedException();
        }

        public Task SaveEntityAsync(PayElement entity)
        {
            throw new NotImplementedException();
        }
    }
}
