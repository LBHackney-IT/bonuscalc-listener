using BonusCalcListener.Gateway.Interfaces;
using BonusCalcListener.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BonusCalcListener.Gateway
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

            var weekStart = WeekStartForClosedTime(closedTime);

            var week = await _context.Weeks
                .Where(w => w.StartAt >= weekStart)
                .Where(w => w.ClosedAt == null)
                .OrderBy(w => w.StartAt)
                .FirstOrDefaultAsync();

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

        private DateTime WeekStartForClosedTime(DateTime? closedTime)
        {
            // Convert the UTC timestamp to GMT/BST at the beginning of the day
            var closedDate = (((DateTime) closedTime).ToLocalTime()).Date;

            // Calculate the number of days we need to subtract to get back to Monday
            var startOfWeek = (int) DayOfWeek.Monday;
            var daysSinceStartOfWeek = ((int) closedDate.DayOfWeek + 7 - startOfWeek) % 7;

            // Convert back to UTC after subtracting the days for the database
            return closedDate.AddDays(-daysSinceStartOfWeek).ToUniversalTime();
        }
    }
}
