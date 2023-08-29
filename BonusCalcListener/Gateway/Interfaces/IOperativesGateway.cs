using BonusCalcListener.Infrastructure;
using System;
using System.Threading.Tasks;

namespace BonusCalcListener.Gateway.Interfaces
{
    public interface IOperativesGateway
    {
        Task ActivateOperative(string operativePayrollId);
    }
}
