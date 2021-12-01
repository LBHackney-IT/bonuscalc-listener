using BonusCalcListener.Boundary;
using BonusCalcListener.Infrastructure;

namespace BonusCalcListener.UseCase
{
    public interface IMapPayElements
    {
        PayElement BuildPayElement(WorkOrderOperativeSmvData eventData);
    }
}
