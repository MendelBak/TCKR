using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tckr.Models
{
    public class StockViewModel : BaseEntity
    {
        [Required]
        public string Symbol { get; set; }

        public string Name { get; set; }
        
        [Required]
        public int Shares { get; set; }

        public int Value { get; set; }
    }
}
