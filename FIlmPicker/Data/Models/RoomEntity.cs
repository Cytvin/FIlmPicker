using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIlmPicker.Data.Models
{
    public class RoomEntity
    {
        [Key]
        [Column(TypeName = "nvarchar(450)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public required string OwnerId { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public required string GuestId { get; set; }
        [Required]
        public bool InviteAccepted { get; set; }
        [Required]
        public bool OwnerIsOut { get; set; }
        [Required]
        public bool GuestIsOut { get; set; }

        [DeleteBehavior(DeleteBehavior.ClientCascade)]
        public virtual IdentityUser Owner { get; set; }
        [DeleteBehavior(DeleteBehavior.ClientCascade)]
        public virtual IdentityUser Guest { get; set; }
        public virtual RoomSettingsEntity RoomSetting { get; set; }
        public virtual ICollection<RoomMovieEntity> Movies { get; set; } = new List<RoomMovieEntity>();
    }
}
