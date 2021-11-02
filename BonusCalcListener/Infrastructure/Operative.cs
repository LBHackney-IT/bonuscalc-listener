using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BonusCalcListener.Infrastructure
{
    public class Operative
    {
        [Key]
        [StringLength(6)]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(3)]
        public string TradeId { get; set; }
        public Trade Trade { get; set; }

        public int? SchemeId { get; set; }
        public Scheme Scheme { get; set; }

        [Required]
        [StringLength(10)]
        public string Section { get; set; }

        public int SalaryBand { get; set; }

        public bool FixedBand { get; set; }

        public bool IsArchived { get; set; }

        public List<Timesheet> Timesheets { get; set; }
    }
}
