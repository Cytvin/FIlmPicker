#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class Genre
    {
        [Key]
        [Column(TypeName = "nvarchar(450)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }

        public ICollection<RoomSettings> RoomSettings { get; set; }
        public ICollection<Movie> Movies { get; set; }
    }
}
