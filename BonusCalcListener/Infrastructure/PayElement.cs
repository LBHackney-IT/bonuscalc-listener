using System;
using System.ComponentModel.DataAnnotations;

namespace BonusCalcListener.Infrastructure
{
    public class PayElement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(17)]
        public string TimesheetId { get; set; }
        public Timesheet Timesheet { get; set; }

        public int PayElementTypeId { get; set; }
        public PayElementType PayElementType { get; set; }

        [StringLength(10)]
        public string WorkOrder { get; set; }

        [StringLength(3)]
        public string TradeCode { get; set; }

        public string Address { get; set; }

        public string Comment { get; set; }

        public decimal Monday { get; set; }
        public decimal Tuesday { get; set; }
        public decimal Wednesday { get; set; }
        public decimal Thursday { get; set; }
        public decimal Friday { get; set; }
        public decimal Saturday { get; set; }
        public decimal Sunday { get; set; }

        public decimal Duration { get; set; }

        public decimal Value { get; set; }
        public bool ReadOnly { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
