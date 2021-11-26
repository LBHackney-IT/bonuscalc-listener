using BonusCalcListener.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway
{
    public class PayElementGateway : IPayElementGateway
    {
        private readonly BonusCalcContext _context;

        public PayElementGateway(BonusCalcContext context)
        {
            _context = context;
        }

        public async Task<List<PayElement>> GetPayElementsByWorkOrderId(string workOrderId, int payElementType)
        {
            return await _context.PayElements
                .Where(pe => pe.PayElementTypeId == payElementType)
                .Where(pe => pe.WorkOrder == workOrderId)
                .ToListAsync();
        }
    }
}
