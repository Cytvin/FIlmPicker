namespace FIlmPicker.Models.DTO
{
    public class RoomDTO
    {
        public string Id { get; set; }
        public UserDTO Owner { get; set; }
        public UserDTO Guest { get; set; }
        public bool InviteAccepted { get; set; }
        public RoomSettingsDTO Settings { get; set; }
        public List<MovieDTO> Movies { get; set; }
    }
}
