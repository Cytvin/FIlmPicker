#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int TypeId { get; set; }
        public int MovieLength { get; set; }
        public int Year { get; set; }
        public double KpRaiting { get; set; }
        public double ImdbRating { get; set; }
        public string Poster { get; set; }

        public virtual MovieType Type { get; set; }
        public virtual ICollection<Genre> Genres { get; set; }
    }
}
