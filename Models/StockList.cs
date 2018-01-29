using System;
using System.Collections.Generic;

namespace tckr.Models
{
    public abstract class StockList : BaseEntity
    {
        public int Id { get; set; }
        public User User { get; set; }
        public List<Stock> Stocks { get; set; }     
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
