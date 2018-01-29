using System;

namespace tckr.Models
{
    public class Stock : BaseEntity
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
