using System;
using System.Collections.Generic;

namespace BonusCalcListener.Boundary
{
    public class WorkOrderElementData
    {
        public string WorkOrderId { get; set; }

        //Combine these for a work order operative task?
        public List<WorkOrderTask> WorkOrderTasks { get; set; }
        public List<WorkOrderOperative> WorkOrderOperatives { get; set; }
        public DateTime? ClosedTime { get; set; }
    }
}
