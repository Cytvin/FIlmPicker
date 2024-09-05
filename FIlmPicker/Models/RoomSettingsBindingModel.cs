using System.ComponentModel.DataAnnotations;

namespace FIlmPicker.Models
{
    public class RoomSettingsBindingModel
    {
        [Required]
        public string Id { get; set; }
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
    }
}
