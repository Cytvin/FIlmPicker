using FIlmPicker.Data;
using FIlmPicker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Models.Converters;

namespace FIlmPicker.Services.DatabaseServices
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;

        public UserService(ILogger<UserService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<User?> GetUserByUserNameAsync(string userName)
        {
            string userNameNormalized = userName.Trim().ToUpper();

            IdentityUser? user = await _context.Users.FirstOrDefaultAsync(u => string.Equals(u.NormalizedUserName, userNameNormalized));

            if (user == null)
            {
                return null;
            }

            return user.ToModel();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            IdentityUser? user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            return user.ToModel();
        }
    }
}
