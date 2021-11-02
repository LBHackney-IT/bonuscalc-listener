using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BonusCalcListener.Infrastructure
{
    public class Week
    {
        [Key]
        public string Id { get; set; }

        public string BonusPeriodId { get; set; }

        public BonusPeriod BonusPeriod { get; set; }

        public DateTime StartAt { get; set; }

        public int Number { get; set; }

        public DateTime? ClosedAt { get; set; }

        public List<Timesheet> Timesheets { get; set; }
    }
}
