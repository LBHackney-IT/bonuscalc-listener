using BonusCalcListener.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway.Interfaces
{
    public interface IPayElementGateway
    {
        Task<List<PayElement>> GetPayElementsByWorkOrderId(string workOrderId, int payElementType);
    }
}
