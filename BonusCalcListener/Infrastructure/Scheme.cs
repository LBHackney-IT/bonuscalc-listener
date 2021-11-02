using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BonusCalcListener.Infrastructure
{
    public class Scheme
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public decimal ConversionFactor { get; set; }

        public List<Operative> Operatives { get; set; }

        public List<PayBand> PayBands { get; set; }
    }
}
