using digitalAlbumApi.DTOs.UserDtos;
using digitalAlbumApi.Helpers;
using digitalAlbumApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace digitalAlbumApi.Services
{
    public interface IUserService
    {
        User CreateUser(User user, string password);
        User AuthenticateUser(string email, string password);
        ValueTask<User> GetById(long id);
    }
    public class UserService :IUserService
    {
        private readonly AlbumContext _context;
        private HMACSHA512 _hmac;

        public UserService(AlbumContext context, HMACSHA512 hmac)
        {
            _context = context;
            _hmac = hmac;
        }

        public User CreateUser(User user, string Password)
        {
            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentNullException("Password", "Password cannot be null or empty");
            }

            if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.Email))
            {
                throw new ArgumentNullException("User", "No user fields can be null or empty");
            }

            if(_context.Users.Any(x => x.Email == user.Email))
            {
                throw new AppException("Email already exists");
            }
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(Password, out passwordHash, out passwordSalt);
            user.PasswordSalt = passwordSalt;
            user.PasswordHash = passwordHash;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public User AuthenticateUser(string email, string password)
        {
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null; 
            }

            User user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user != null)
            {
                if (verifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    return user;
                }
            }

            return null;
        }

        public ValueTask<User> GetById(long id)
        {
            var result = _context.Users.FindAsync(id);
            return result;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            passwordSalt = _hmac.Key;
            passwordHash = _hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

        }

        private bool verifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using(var verifyHmac = new HMACSHA512(storedSalt))
            {
                var computedHash = verifyHmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }


    }
}
