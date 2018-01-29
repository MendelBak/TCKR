using System;
using System.Collections.Generic;

namespace tckr.Models
{
    public abstract class StockList : BaseEntity
    {
        public User User { get; set; }
        public List<Stock> Stocks { get; set; }
    }
}
