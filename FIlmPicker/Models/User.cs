using FIlmPicker.Models.DTO;

namespace FIlmPicker.Models
{
    public class User
    {
        private string _id;
        private string _userName;

        public string Id => _id;
        public string UserName => _userName;

        public User(UserDTO user)
        {
            _id = user.Id;
            _userName = user.UserName;
        }
    }
}
