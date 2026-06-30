using Microsoft.EntityFrameworkCore;
using static Lactosure_api.Models.lacto;

namespace Lactosure_api.DB_Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Otp> Otp { get; set; }
        public DbSet<Society> Society { get; set; }
        public DbSet<MachineType> MachineType { get; set; }
        public DbSet<Machine> Machine { get; set; }
        public DbSet<BleDevice> BleDevice { get; set; }
        public DbSet<UserFace> UserFace { get; set; }
        public DbSet<CorrMethodHistory> CorrMethodHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Machine>()
                .HasOne(m => m.Society)
                .WithMany(s => s.Machines)
                .HasForeignKey(m => m.SID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}