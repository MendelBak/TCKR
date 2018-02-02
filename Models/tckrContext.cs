using System;
using Microsoft.EntityFrameworkCore;

namespace tckr.Models
{
    public class tckrContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public tckrContext(DbContextOptions<tckrContext> options) : base(options) { }
        // First variable should mirror the model class name. Second variable should mirror DB table name. (In PostgreSQL it will create tables that mirror the second variable.) //
        public DbSet<User> Users { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Watchlist> Watchlists { get; set; }
    }
}
