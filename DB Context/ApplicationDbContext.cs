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

    }
}
