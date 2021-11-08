using System;
using System.Collections.Generic;
using System.Text;

namespace BonusCalcListener.Boundary
{
    public class RateScheduleItem
    {
        public Guid Id { get; set; }
        public string M3NHFSORCode { get; set; }
        public string CustomCode { get; set; }
        public string CustomName { get; set; }
        public virtual Quantity Quantity { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public double? CodeCost { get; set; }

        // extensions
        public bool Original { get; set; } = false;
        public double? OriginalQuantity { get; set; } = null;

        public Guid OriginalId { get; set; }
    }
}
