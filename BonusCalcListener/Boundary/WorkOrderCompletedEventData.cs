using System;
using System.Collections.Generic;

namespace BonusCalcListener.Boundary
{
    public class WorkOrderCompletedEventData
    {
        public int Id { get; set; }
        public List<WorkElement> CompletedWorkElements { get; set; }
        public DateTime? ClosedTime { get; set; }
    }
}
