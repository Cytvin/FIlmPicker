using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class RoomSettings
    {
        [Key]
        [Column(TypeName = "nvarchar(450)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string RoomId { get; set; }
        [Required]
        [DefaultValue(1.0)]
        [Column(TypeName = "nvarchar(4)")]
        public string MinKpRating { get; set; }
        [Required]
        [DefaultValue(10.0)]
        [Column(TypeName = "nvarchar(4)")]
        public string MaxKpRating { get; set; }
        [Required]
        [DefaultValue(1990)]
        public int MinYear { get; set; }
        [Required]
        [DefaultValue(2024)]
        public int MaxYear { get; set; }
        [Required]
        public int TypeNumber { get; set; }

        public virtual Room Room { get; set; }
    }
}
