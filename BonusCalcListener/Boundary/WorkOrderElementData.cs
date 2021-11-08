using System;
using System.Collections.Generic;

namespace BonusCalcListener.Boundary
{
    public class WorkOrderElementData
    {
        public int Id { get; set; }
        public List<WorkElement> CompletedWorkElements { get; set; }
        public DateTime? ClosedTime { get; set; }
    }
}
