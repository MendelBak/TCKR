using System;

namespace tckr.Models
{
    public class Stock : BaseEntity
    {
        public string Symbol { get; set; }
        public int Shares { get; set; }
        public double PurchasePrice { get; set; }
        
        // The following values are not stored in the database
        // but are necessary for rendering views
        public string Name { get; set; }
        public double PurchaseValue { get; set; }
        public double CurrentPrice { get; set; }
        public double CurrentValue { get; set; }
        public double GainLossPrice { get; set; }
        public double GainLossValue { get; set; }
        public double GainLossPercent { get; set; }
        public double Week52Low { get; set; }
        public double Week52High { get; set; }
    }
}
