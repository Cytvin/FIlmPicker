namespace FIlmPicker.Models
{
    public class Room
    {
        private string? _id;
        private User _owner;
        private User _guest;
        private bool _inviteAccepted;
        private bool _ownerIsOut;
        private bool _guestIsOut;
        private RoomSettings _roomSettings;

        public string? Id => _id;
        public User Owner => _owner;
        public User Guest => _guest;
        public bool InviteAccepted => _inviteAccepted;
        public bool OwnerIsOut => _ownerIsOut;
        public bool GuestIsOut => _guestIsOut;
        public RoomSettings RoomSettings => _roomSettings;

        public Room(string id, User owner, User guest, bool inviteAccepted,
            bool ownerIsOut, bool guestIsOut, RoomSettings? roomSettings = null)
        {
            _id = id;
            _owner = owner;
            _guest = guest;
            _inviteAccepted = inviteAccepted;
            _ownerIsOut = ownerIsOut;
            _guestIsOut = guestIsOut;
            _roomSettings = roomSettings ?? new RoomSettings(id);
        }

        private Room(User owner, User guest)
        {
            _id = Guid.NewGuid().ToString();
            _owner = owner;
            _guest = guest;
            _ownerIsOut = false;
            _guestIsOut = false;
            _inviteAccepted = false;
            _roomSettings = new RoomSettings(_id);
        }

        public static Room CreateEmptyRoom(User owner, User guest)
        {
            return new Room(owner, guest);
        }

        public void SetRoomSettings(RoomSettings roomSettings)
        {
            _roomSettings = roomSettings;
        }

        public void AcceptInvite()
        {
            _inviteAccepted = true;
        }

        public void OwnerOut()
        {
            _ownerIsOut = true;
        }

        public void GuestOut()
        {
            _guestIsOut = true;
        }
    }
}
