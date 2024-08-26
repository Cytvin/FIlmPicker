using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class MoviesInRoom
    {
        [Key]
        [Column(TypeName = "nvarchar(450)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string RoomId { get; set; }
        [Required]
        public int MovieKpId { get; set; }
        [Required]
        public int OwnerScore { get; set; }
        [Required]
        public int GuestScore { get; set; }

        public virtual Room Room { get; set; }
    }
}
