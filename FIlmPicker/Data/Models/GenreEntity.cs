using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class GenreEntity
    {
        [Key]
        [Column(TypeName = "nvarchar(450)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<RoomSettingsEntity> RoomSettings { get; set; } = new List<RoomSettingsEntity>();
        public ICollection<MovieEntity> Movies { get; set; } = new List<MovieEntity>();
    }
}
