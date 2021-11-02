using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BonusCalcListener.Infrastructure
{
    public class BonusPeriod
    {
        [Key]
        public string Id { get; set; }

        public DateTime StartAt { get; set; }

        public int Year { get; set; }

        public int Number { get; set; }

        public DateTime? ClosedAt { get; set; }

        public List<Week> Weeks { get; set; }
    }
}
