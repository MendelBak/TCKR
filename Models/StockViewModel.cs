using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tckr.Models
{
    public class StockViewModel : BaseEntity
    {
        [Required]
        public string Symbol { get; set; }
        
        [Required]
        public int Shares { get; set; }

        [Required]
        public double PurchasePrice { get; set; }
    }
}
