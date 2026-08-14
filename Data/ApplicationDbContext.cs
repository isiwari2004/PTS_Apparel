using Microsoft.EntityFrameworkCore;
using PTS_Apparel.Models;

namespace PTS_Apparel.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ScreenPrivilege> ScreenPrivileges { get; set; }
        public DbSet<FactoryMaster> FactoryMasters { get; set; }
        public DbSet<FactoryDetail> FactoryDetails { get; set; }
        public DbSet<FactoryProductionLine> FactoryProductionLines { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Style> Styles { get; set; }
        public DbSet<PO> POs { get; set; }

        // 👇 මේ DbSet එක තමයි හරියට එකතු කරන්න ඕනේ
        public DbSet<Forecast> Forecasts { get; set; }

        public DbSet<InputRecorder> InputRecorders { get; set; }
    }
}