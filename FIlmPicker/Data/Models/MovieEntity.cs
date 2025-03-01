using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class MovieEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int TypeId { get; set; }
        public int MovieLength { get; set; }
        public int Year { get; set; }
        public double KpRaiting { get; set; }
        public double ImdbRating { get; set; }
        public string? Poster { get; set; } = string.Empty;

        public virtual MovieTypeEntity Type { get; set; }
        public virtual ICollection<GenreEntity> Genres { get; set; } = new List<GenreEntity>();
    }
}
