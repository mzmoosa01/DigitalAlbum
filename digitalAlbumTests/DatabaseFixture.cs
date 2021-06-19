using digitalAlbumApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace digitalAlbumTests
{
    public class DatabaseFixture : IDisposable
    {
        public AlbumContext dbContext { get; private set; }
        private Photo[] _initialPhotos;
        private User[] _initialUsers;
        public DatabaseFixture()
        {
            // initialize data in the test database
            _initialPhotos = getInitialPhotoEntries();
            _initialUsers = getInitialUserEntries();
            seedDatabase();

        }

        private void seedDatabase()
        {
            var options = getContextOptions();
            dbContext = new AlbumContext(options);

            foreach (Photo photo in _initialPhotos)
            {
                dbContext.Photos.Add(photo);
            }

            foreach (User user in _initialUsers)
            {
                dbContext.Users.Add(user);
            }

            dbContext.SaveChanges();
        }

        public Photo[] getInitialPhotoEntries()
        {
            return new Photo[]
             {
                new Photo {photoUri="http://testUri1.com", Title="TestPhoto1", uploadDate=new DateTime()},
                new Photo {photoUri="http://testUri2.com", Title="TestPhoto2", uploadDate=new DateTime()},
                new Photo {photoUri="http://testUri3.com", Title="TestPhoto3", uploadDate=new DateTime()},
            };
        }

        public User[] getInitialUserEntries()
        {
            return new User[]
             {
                new User() { Email = "firstUser@test.com", FirstName = "testName", LastName = "testLast" }
            };
        }

        public DbContextOptions<AlbumContext> getContextOptions()
        {
            return new DbContextOptionsBuilder<AlbumContext>().UseInMemoryDatabase("digitalAlbum").Options;
        }

        public void Dispose()
        {
            // clean up test data from the database
            var options = getContextOptions();
            dbContext.Photos.RemoveRange(_initialPhotos);
            dbContext.Users.RemoveRange(_initialUsers);

        }
    }
}
