using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway
{
    public class OperativesGateway : IOperativesGateway
    {
        private readonly BonusCalcContext _context;
        private readonly ILogger<OperativesGateway> _logger;
        public OperativesGateway(BonusCalcContext context, ILogger<OperativesGateway> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ActivateOperative(string operativePayrollId)
        {
            if (operativePayrollId == null)
            {
                _logger.LogError("Can't activate an operative Id of null");
                throw new ArgumentNullException(nameof(operativePayrollId));
            }

            var op = await _context.Operatives
                .Where(x => x.Id == operativePayrollId)
                .SingleOrDefaultAsync();

            if (op == null)
            {
                _logger.LogWarning($"We recieved a request to unarchive operative {operativePayrollId} who doesn't exist here");
                return;
            }

            if (op.IsArchived)
            {
                op.IsArchived = false;

                var result = await _context.SaveChangesAsync();

                _logger.LogInformation($"Operative {operativePayrollId} was archived but recieved job data, {result} items automatically un-archived");
            }
        }
    }
}
