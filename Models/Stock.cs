using System;

namespace tckr.Models
{
    public class Stock : BaseEntity
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public int Shares { get; set; }
        public int Value { get; set; }
    }
}
