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

        // (Mendel) Actually, I think that this model definition is not the best way to display the data. 
        // Instead of temporarily writing (overwriting) the API info to the DB (in order to be able to return the entire User object to ViewBag with all the containing stocks).
        // Perhaps we can loop through each Stock (in the Main controller) and save that data to a list of stocks which we will then pass into ViewBag and then iterate through on the front end. 
        // I managed to do something like this when retreiving the Most Active Stocks data. It came prepackaged in a list which I then iterated through on the frontend (landing page).
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
