using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Data.Models;

namespace FIlmPicker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<RoomEntity> Rooms { get; set; }
        public DbSet<RoomMovieEntity> RoomMovies { get; set; }
        public DbSet<RoomSettingsEntity> RoomSettings { get; set; }
        public DbSet<MovieEntity> Movies { get; set; }
        public DbSet<GenreEntity> Genres { get; set; }
        public DbSet<MovieTypeEntity> Types { get; set; }
        public DbSet<MovieListOnUpdate> MovieListsOnUpdate { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GenreEntity>()
                .HasMany(e => e.RoomSettings)
                .WithMany(e => e.Genres)
                .UsingEntity("GenreRoomSettings");

            builder.Entity<MovieEntity>()
                .HasMany(e => e.Genres)
                .WithMany(e => e.Movies)
                .UsingEntity("GenreMovie");
        }
    }
}
