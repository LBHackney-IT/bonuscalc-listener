using BonusCalcListener.Infrastructure;
using System;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway.Interfaces
{
    public interface ITimesheetGateway
    {
        Task<Timesheet> GetCurrentTimeSheetForOperative(string operativeId, DateTime? closedTime);
    }
}
