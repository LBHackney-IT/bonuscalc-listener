using BonusCalcListener.Boundary;
using System.Threading.Tasks;

namespace BonusCalcListener.UseCase.Interfaces
{
    public interface IMessageProcessing
    {
        Task ProcessMessageAsync(EntityEventSns message);
    }
}
