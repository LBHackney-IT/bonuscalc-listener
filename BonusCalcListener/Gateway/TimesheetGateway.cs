using BonusCalcListener.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BonusCalcListener.Gateway.Interfaces
{
    public class TimesheetGateway : ITimesheetGateway
    {
        private readonly BonusCalcContext _context;
        private readonly ILogger<TimesheetGateway> _logger;

        public TimesheetGateway(BonusCalcContext context, ILogger<TimesheetGateway> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Timesheet> GetCurrentTimeSheetForOperative(string operativeId, DateTime? closedTime)
        {
            if (!closedTime.HasValue) throw new ArgumentNullException(nameof(closedTime));

            var week = await _context.Weeks
                .Where(w => w.StartAt <= closedTime)
                .Where(w => w.ClosedAt < closedTime)
                .SingleOrDefaultAsync();

            if (week == null)
            {
                _logger.LogError($"Could not find current week for date {closedTime.Value.Date.ToShortDateString()}");
                throw new ArgumentException("Closed time is invalid");
            }

            return await _context.Timesheets
                .Include(t => t.Week)
                .Include(t => t.PayElements)
                    .ThenInclude(pe => pe.PayElementType)
                .Include(t => t.Operative)
                .Where(x => x.OperativeId == operativeId && x.WeekId == week.Id)
                .SingleOrDefaultAsync();
        }
    }
}
