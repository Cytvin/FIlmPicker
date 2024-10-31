#nullable disable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FIlmPicker.Data.Models
{
    [PrimaryKey(nameof(RoomSettingsId), nameof(GenreId))]
    public class RoomSettingsGenre
    {
        [Required]
        public string RoomSettingsId { get; set; }
        [Required]
        public string GenreId { get; set; }

        public RoomSettings Settings { get; set; }
        public Genre Genre { get; set; }
    }
}
