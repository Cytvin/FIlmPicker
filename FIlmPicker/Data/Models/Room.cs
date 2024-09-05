using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class Room
    {
        [Key]
        [Column(TypeName = "nvarchar(450)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string OwnerId { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string GuestId { get; set; }
        [Required]
        public bool InviteAccepted { get; set; }

        [DeleteBehavior(DeleteBehavior.ClientCascade)]
        public virtual IdentityUser Owner { get; set; }
        [DeleteBehavior(DeleteBehavior.ClientCascade)]
        public virtual IdentityUser Guest { get; set; }
        public virtual RoomSettings RoomSetting { get; set; } 
        public virtual ICollection<RoomMovie> Movies { get; set; }
    }
}
