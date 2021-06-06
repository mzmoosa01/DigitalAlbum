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
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            passwordSalt = _hmac.Key;
            passwordHash = _hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

        }

        //private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        //{
        //    using (var hmac = new System.Security.Cryptography.HMACSHA512())
        //    {
        //        passwordSalt = hmac.Key;
        //        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //    }
        //}
    }
}
