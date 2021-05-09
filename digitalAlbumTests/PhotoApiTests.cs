using System;
using Xunit;
using digitalAlbumApi.Controllers;
using digitalAlbumApi.Models;
using EntityFrameworkCoreMock;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace digitalAlbumTests
{
    public class PhotoApiTests
    {
        private photoController photoController;

        public PhotoApiTests()
        {
            //arange
            var initialDbEntities = new[]
            {
                new Photo {photoId = 1, photoUri="http://testUri1.com", Title="TestPhoto1", uploadDate=new DateTime()},
                new Photo {photoId = 2, photoUri="http://testUri2.com", Title="TestPhoto2", uploadDate=new DateTime()},
                new Photo {photoId = 3, photoUri="http://testUri3.com", Title="TestPhoto3", uploadDate=new DateTime()},
            };

            var dbContextMock = new DbContextMock<AlbumContext>(new DbContextOptionsBuilder<AlbumContext>().Options);
            var photoSetMock = dbContextMock.CreateDbSetMock(x => x.Photos, initialDbEntities);

            photoController = new photoController(dbContextMock.Object);
        }

        [Fact]
        public void get_all_returns_all_results()
        {
            

            //act
            var result = photoController.GetPhotos().Result;
            List<Photo> value = result.Value as List<Photo>;

            //assert
            Assert.Equal(3, value.Count);
        }
    }
}
