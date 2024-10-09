using FIlmPicker.Models.DTO;

namespace FIlmPicker.Models
{
    public class Room
    {
        private string? _id;
        private User _owner;
        private User _guest;
        private bool _inviteAccepted;
        private RoomSettings _roomSettings;

        public string? Id => _id;
        public User Owner => _owner;
        public User Guest => _guest;
        public bool InviteAccepted => _inviteAccepted;
        public RoomSettings RoomSettings => _roomSettings;

        public Room(RoomDTO room)
        {
            _id = room.Id;
            _owner = new User(room.Owner);
            _guest = new User(room.Guest);
            _inviteAccepted = room.InviteAccepted;
            _roomSettings = new RoomSettings(room.Settings);
        }

        public Room(User owner, User guest)
        {
            _owner = owner;
            _guest = guest;
            _inviteAccepted = false;
        }

        public void SetRoomSettings(RoomSettings roomSettings)
        {
            _roomSettings = roomSettings;
        }

        public void AcceptInvite()
        {
            _inviteAccepted = true;
        }
    }
}
