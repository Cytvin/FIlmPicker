using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public float MinKpRating { get; set; }
        [Required]
        public float MaxKpRating { get; set; }
        [Required]
        public int MinYear { get; set; }
        [Required]
        public int MaxYear { get; set; }
        [Required]
        public int TypeNumber { get; set; }

        public virtual Room Room { get; set; }
    }
}
