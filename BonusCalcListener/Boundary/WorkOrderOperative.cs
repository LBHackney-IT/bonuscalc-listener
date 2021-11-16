using System;
using System.Collections.Generic;
using System.Text;

namespace BonusCalcListener.Boundary
{
    public class WorkOrderOperative
    {
        public int WorkOrderId { get; set; }
        public int OperativeId { get; set; }
        public double? JobPercentage { get; set; }

        public override bool Equals(object obj)
        {
            return obj is WorkOrderOperative other && WorkOrderId == other.WorkOrderId && OperativeId == other.OperativeId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WorkOrderId, OperativeId);
        }
    }
}
