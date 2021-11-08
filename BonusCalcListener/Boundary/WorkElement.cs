using System;
using System.Collections.Generic;

namespace BonusCalcListener.Boundary
{
    public class WorkElement
    {
        public Guid Id { get; set; }
        public int? ServiceChargeSubject { get; set; }
        public bool? ContainsCapitalWork { get; set; }

        public virtual List<RateScheduleItem> RateScheduleItem { get; set; }
        public virtual List<Trade> Trade { get; set; }
    }
}
