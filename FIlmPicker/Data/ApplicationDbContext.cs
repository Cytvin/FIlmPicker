using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Data.Models;

namespace FIlmPicker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Room> Rooms { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
