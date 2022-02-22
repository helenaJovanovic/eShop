using eShop.Authorization;
using eShop.Entities;
using eShop.Helpers;
using eShop.Models.Users;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using BCryptNet = BCrypt.Net.BCrypt;
using eShop.Models;

namespace eShop.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        AuthenticateResponse Register(RegistrationRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }
    public class UserService : IUserService
    {
        private DataContext _context;
        private IJwtUtils _jwtUtils;
        private readonly AppSettings _appSettings;

        public UserService(
            DataContext context,
            IJwtUtils jwtUtils,
            IOptions<AppSettings> appSettings)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _appSettings = appSettings.Value;
           
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

            if (user == null || !BCryptNet.Verify(model.Password, user.PasswordHash))
                throw new AppException("Username or paswword is incorrect");

            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken);
        }

        public AuthenticateResponse Register(RegistrationRequest model)
        {
            var user =  _context.Users.SingleOrDefault(x => x.Username == model.Username);

            if (user != null)
            {
                throw new AppException("Username alrady exists");
            }

            //make a user
            user = new User { FirstName = model.FirstName, LastName = model.LastName, Username = model.Username, PasswordHash = BCryptNet.HashPassword(model.Password), Role = Role.User };

            //save user to DB
            _context.Users.Add(user);
            _context.SaveChanges();

            var cart = new Cart { UserId = user.UserId };
            _context.Carts.Add(cart);
            _context.SaveChanges();

            //make a token for user
            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken);
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }
    }
}
