using System;
using System.Collections.Generic;
using System.Text;

namespace BonusCalcListener.Boundary
{
    public class WorkOrderTask
    {
        public double Quantity { get; internal set; }
        public string Code { get; internal set; }
        public double? Cost { get; internal set; }
        public DateTime? DateAdded { get; internal set; }
        public DateTime? DateUpdated { get; set; }
        public string Description { get; internal set; }
        public string Status { get; internal set; }
        public bool Original { get; set; }
        public Guid Id { get; set; }
        public double? OriginalQuantity { get; set; }
        public int StandardMinuteValue { get; set; } = 0;
    }
}
