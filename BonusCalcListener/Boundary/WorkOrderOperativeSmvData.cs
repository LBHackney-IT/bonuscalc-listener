using System;

namespace BonusCalcListener.Boundary
{
    public class WorkOrderOperativeSmvData
    {
        public string WorkOrderId { get; set; }
        public int WorkOrderStatusCode { get; set; }
        public string Address { get; set; }

        public string Description { get; set; }

        // The SMV for the whole job
        public double StandardMinuteValue { get; set; }
        public string OperativePrn { get; set; }
        public double? JobPercentage { get; set; }
        public DateTime? ClosedTime { get; set; }
    }
}
