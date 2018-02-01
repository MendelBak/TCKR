using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tckr.Models
{
    public class PortfolioStockViewModel : BaseEntity
    {
        [Required]
        public string Symbol { get; set; }
        
        [Required]
        public int Shares { get; set; }

        [Required]
        public double PurchasePrice { get; set; }
    }


    public class WatchlistStockViewModel : BaseEntity
    {
        [Required]
        public string Symbol { get; set; }
    }

    public class AllStockViewModels 
    {
        public PortfolioStockViewModel PortfolioStockViewModel { get; set; }
        public WatchlistStockViewModel WatchlistStockViewModel { get; set; }
    }
}
