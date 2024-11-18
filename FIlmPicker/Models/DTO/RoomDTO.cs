#nullable disable

namespace FIlmPicker.Models.DTO
{
    public class RoomDTO
    {
        public string Id { get; set; }
        public UserDTO Owner { get; set; }
        public UserDTO Guest { get; set; }
        public bool InviteAccepted { get; set; }
        public bool OwnerIsOut {  get; set; }
        public bool GuestIsOut { get; set; }
        public RoomSettingsDTO Settings { get; set; }
    }
}
