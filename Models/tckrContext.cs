using System;
using Microsoft.EntityFrameworkCore;


namespace tckr.Models
{
    public class tckrContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public tckrContext(DbContextOptions<tckrContext> options) : base(options) { }
        // First variable should mirror the model class name. Second variable should mirror DB table name. (In PostgreSQL it will create schemas and tables that mirror these variables.) //
        public DbSet<User> Users { get; set; }
    }

}
