using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class RoomSettingsEntity
    {
        [Key]
        [Column(TypeName = "nvarchar(450)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string RoomId { get; set; }
        [Required]
        public float MinKpRating { get; set; } = 1;
        [Required]
        public float MaxKpRating { get; set; } = 10;
        [Required]
        public int MinYear { get; set; } = 1990;
        [Required]
        public int MaxYear { get; set; } = 2050;
        [Required]
        public int TypeNumber { get; set; } = 1;
        [Required]
        public int MinVotes { get; set; } = 5000;
        [Required]
        public int MoviesReceived { get; set; }

        public virtual RoomEntity Room { get; set; }
        public virtual ICollection<GenreEntity> Genres { get; set; }
    }
}
