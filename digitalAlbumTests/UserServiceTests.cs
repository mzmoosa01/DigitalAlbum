using AutoMapper;
using digitalAlbumApi.DTOs.UserDtos;
using digitalAlbumApi.Helpers;
using digitalAlbumApi.Models;
using digitalAlbumApi.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using System.Security.Cryptography;

namespace digitalAlbumTests
{
    public class UserServiceTests: IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture dbFixture;
        private byte[] salt;

        public UserServiceTests(DatabaseFixture fixture)
        {
            dbFixture = fixture;
            salt = System.Text.Encoding.UTF8.GetBytes("theTestSalt");
            //userService = new UserService(dbFixture.dbContext);
        }


        [Theory]
        [MemberData(nameof(UserServiceTestData.CreateUserNullData), MemberType =typeof(UserServiceTestData))]
        public void CreateUser_throws_ArgumentNullException_NullValues(User user)
        {
            UserService userService = new UserService(new AlbumContext(dbFixture.getContextOptions()), new HMACSHA512(salt));
            //Act
            Action CreateAction = () => userService.CreateUser(user, "12345");

            //Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(CreateAction);
            Assert.Equal("User", exception.ParamName);
            Assert.Equal("No user fields can be null or empty (Parameter 'User')", exception.Message);
        }

        [Fact]
        public void CreateUser_throws_ArgumentNullException_BlankPassword()
        {
            UserService userService = new UserService(new AlbumContext(dbFixture.getContextOptions()), new HMACSHA512(salt));
            User user = new User() { Email = "abc@test.com", FirstName = "abc", LastName = "def" };
            //Act
            Action CreateAction = () => userService.CreateUser(user, "");

            //Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(CreateAction);
            Assert.Equal("Password", exception.ParamName);
            Assert.Equal("Password cannot be null or empty (Parameter 'Password')", exception.Message);
        }

        [Fact]
        public void CreateUser_throws_exception_EmailAlreadyExists()
        {
            //Arrange
            UserService userService = new UserService(new AlbumContext(dbFixture.getContextOptions()), new HMACSHA512(salt));
            User user = new User() { Email = dbFixture.getInitialUserEntries()[0].Email, FirstName = "abc", LastName = "def" };

            //Act
            Action CreateAction = () => userService.CreateUser(user, "12345");

            //Assert
            AppException exception = Assert.Throws<AppException>(CreateAction);
            Assert.Equal("Email already exists", exception.Message);
        }

        [Fact]
        public void CreateUser_returns_user_with_Salt()
        {
            //Arrange
            UserService userService = new UserService(new AlbumContext(dbFixture.getContextOptions()), new HMACSHA512(salt));
            User user = new User() { Email = "user@saltTest.com", FirstName = "abc", LastName = "def"};

            //Act
            User returnedUser = userService.CreateUser(user, "12345");

            //Assert
            Assert.Equal(salt, returnedUser.PasswordSalt);
        }

        [Fact]
        public void CreateUser_returns_user_with_HashedPassword()
        {
            //Arrange
            var hmac = new HMACSHA512(salt);
            UserService userService = new UserService(new AlbumContext(dbFixture.getContextOptions()), hmac);

            User user = new User() { Email = "user@hashTest.com", FirstName = "abc", LastName = "def" };
            string password = "abc123";
            byte[] expectedHash;

            expectedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            //Act
            User returnedUser = userService.CreateUser(user, password);

            //Assert
            Assert.Equal(expectedHash, returnedUser.PasswordHash);
        }

        [Fact]
        public void CreateUser_saves_user_to_context()
        {
            //Arrange
            UserService userService = new UserService(new AlbumContext(dbFixture.getContextOptions()), new HMACSHA512(salt));

            User user = new User() { Email = "user@Test.com", FirstName = "abc", LastName = "def" };
            string password = "abc123";


            //Act
            User returnedUser = userService.CreateUser(user, password);
            User expectedUser = dbFixture.dbContext.Users.Find(returnedUser.Id);

            //Assert
            Assert.Equal(expectedUser.Email, returnedUser.Email);
            Assert.Equal(expectedUser.FirstName, returnedUser.FirstName);
            Assert.Equal(expectedUser.LastName, returnedUser.LastName);
            Assert.Equal(expectedUser.PasswordHash, returnedUser.PasswordHash);
            Assert.Equal(expectedUser.PasswordSalt, returnedUser.PasswordSalt);

        }

        [Theory]
        [MemberData(nameof(UserServiceTestData.AuthenticateUserNullData), MemberType = typeof(UserServiceTestData))]
        public void AuthenticateUser_returns_null_missing_values(string username, string password)
        {
            UserService userService = new UserService(new AlbumContext(dbFixture.getContextOptions()), new HMACSHA512(salt));
            //Act
            var result = userService.AuthenticateUser(username, password);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void AuthenticateUser_returns_null_NonExistentUser()
        {
            UserService userService = new UserService(dbFixture.dbContext, new HMACSHA512(salt));
            //Act
            var result = userService.AuthenticateUser("WeirdUsernameNoOneHas", "Random");

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void AuthenticateUser_returns_null_failed_verification()
        {
            //Arrange
            var hmac = new HMACSHA512(salt);
            var password = "reallyGoodPassword1";
            User user = new User() { Email = "successTest@test.com", FirstName = "firstName1", LastName = "LastName1", PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)), PasswordSalt = hmac.Key };
            dbFixture.dbContext.Add(user);
            dbFixture.dbContext.SaveChanges();
            UserService userService = new UserService(dbFixture.dbContext, hmac);

            //Act
            var result = userService.AuthenticateUser(user.Email, "incorrectPassword");

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void AuthenticateUser_returns_user_successful_verification()
        {
            //Arrange
            var hmac = new HMACSHA512(salt);
            var password = "reallyGoodPassword1";
            User user = new User() { Email = "failTest@test.com", FirstName = "firstName1", LastName = "LastName1", PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)), PasswordSalt = hmac.Key};
            dbFixture.dbContext.Add(user);
            dbFixture.dbContext.SaveChanges();
            UserService userService = new UserService(dbFixture.dbContext, hmac);

            //Act
            var result = userService.AuthenticateUser(user.Email, password);

            //Assert
            Assert.IsType<User>(result);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async void GetUserById_returns_null_invalid_Id()
        {
            //Arrange
            UserService userService = new UserService(dbFixture.dbContext, new HMACSHA512(salt));

            //Act
            var result = await userService.GetById(100);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async void GetUserById_returns_user_validId()
        {
            //Arrange
            User expectedUser = dbFixture.getInitialUserEntries()[0];
            UserService userService = new UserService(dbFixture.dbContext, new HMACSHA512(salt));


            //Act
            var result = await userService.GetById(1);

            //Assert
            Assert.IsType<User>(result);
            Assert.Equal(expectedUser.Email, result.Email);
        }




    }

    public class UserServiceTestData
    {
        public static IEnumerable<Object[]> CreateUserNullData =>
            new List<Object[]>
            {
                new object[] {new User() { Email = "", FirstName = "testName", LastName = "testLast"} },
                new object[] {new User() { Email = "testUser", FirstName = "", LastName = "testLast"} },
                new object[] {new User() { Email = "testUser", FirstName = "testName", LastName = ""} }
            };

        public static IEnumerable<Object[]> AuthenticateUserNullData =>
            new List<Object[]>
            {
                new object[] {"testEmail", "" },
                new object[] {"", "testPassword" },
            };
    }
}
