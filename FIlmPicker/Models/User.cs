namespace FIlmPicker.Models
{
    public class User
    {
        private string _id;
        private string _userName;

        public string Id => _id;
        public string UserName => _userName;

        public User(string id, string userName)
        {
            _id = id;
            _userName = userName;
        }
    }
}
