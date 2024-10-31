#nullable disable

using Microsoft.EntityFrameworkCore;

namespace FIlmPicker.Data.Models
{
    [PrimaryKey(nameof(MovieId), nameof(GenreId))]
    public class MovieGenre
    {
        public int MovieId { get; set; }
        public string GenreId { get; set; }

        public virtual Movie Movie { get; set; }
        public virtual Genre Genre { get; set; }
    }
}
