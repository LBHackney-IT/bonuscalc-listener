using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BonusCalcListener.Infrastructure
{
    public class Timesheet
    {
        [Key]
        [StringLength(17)]
        public string Id { get; set; }

        [Required]
        [StringLength(6)]
        public string OperativeId { get; set; }
        public Operative Operative { get; set; }

        public string WeekId { get; set; }
        public Week Week { get; set; }

        public decimal Utilisation { get; set; }

        public List<PayElement> PayElements { get; set; }
    }
}
