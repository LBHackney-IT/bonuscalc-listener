using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BonusCalcListener.Infrastructure
{
    public class Trade
    {
        [Key]
        [StringLength(3)]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public List<Operative> Operatives { get; set; }
    }
}
