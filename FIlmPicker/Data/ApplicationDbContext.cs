using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Data.Models;

namespace FIlmPicker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomMovie> RoomMovies { get; set; }
        public DbSet<RoomSettings> RoomSettings { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<MovieType> Types { get; set; }
        public DbSet<RoomSettingsGenre> RoomSettingsGenre { get; set; }
        public DbSet<MovieListOnUpdate> MovieListsOnUpdate { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
