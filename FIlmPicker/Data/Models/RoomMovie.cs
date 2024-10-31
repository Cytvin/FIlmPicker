#nullable disable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    [PrimaryKey(nameof(RoomId), nameof(MovieId))]
    public class RoomMovie
    {
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string RoomId { get; set; }
        [Required]
        public int MovieId { get; set; }
        [Required]
        public int OwnerScore { get; set; }
        [Required]
        public int GuestScore { get; set; }

        public virtual Movie Movie { get; set; }
        public virtual Room Room { get; set; }
    }
}
