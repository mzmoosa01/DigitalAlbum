using System;
using Xunit;
using digitalAlbumApi.Controllers;
using digitalAlbumApi.Models;
using EntityFrameworkCoreMock;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;

namespace digitalAlbumTests
{
    public class PhotoApiTests : IClassFixture<DatabaseFixture>
    {
        DatabaseFixture dbFixture;
        public PhotoApiTests(DatabaseFixture fixture)
        {
            this.dbFixture = fixture;
        }

        //private DbContextOptions<AlbumContext> getContextOptions()
        //{
        //    return new DbContextOptionsBuilder<AlbumContext>().UseInMemoryDatabase("digitalAlbum").Options;
        //}

        private PhotoController getPhotoController ()
        {            
            return new PhotoController(dbFixture.dbContext);
        }

        

        [Fact]
        public async void Get_returns_all_results()
        {
            //arrange
            var photoController = getPhotoController();
            int count = await dbFixture.dbContext.Photos.CountAsync<Photo>();

            //act
            var result = photoController.GetPhotos().Result;
            List<Photo> value = result.Value as List<Photo>;

            //assert
            Assert.Equal(count, value.Count);
        }



        [Fact]
        public void GetById_returns_correctResult()
        {
            //arrange
            var photoController = getPhotoController();

            //act
            var result = photoController.GetPhoto(1).Result;
            Photo value = result.Value;


            //assert
            Assert.IsType<Photo>(value);
            Assert.Equal(1, value.photoId);

        }

        [Fact]
        public void GetById_returns_NotFound_invalidId()
        {
            //arrange
            var photoController = getPhotoController();

            //act
            var result = photoController.GetPhoto(100).Result.Result;


            //assert
            Assert.IsType<NotFoundResult>(result);


        }

        [Fact]
        public void Put_updatesContext()
        {
            long id = 1;

            Photo tobeUpdated = dbFixture.dbContext.Photos.Find(id);
            tobeUpdated.Title = "new Title";

            PhotoController photoController = getPhotoController();
            var result = photoController.PutPhoto(id, tobeUpdated);
            Assert.Equal(tobeUpdated.Title, dbFixture.dbContext.Photos.Find(id).Title);
        }

        [Fact]
        public void Put_returns_badRequest_invalidInput()
        {
            Photo tobeUpdated = dbFixture.getInitialPhotoEntries()[2];
            long id = 2;

            PhotoController photoController = getPhotoController();

            //act
            var result = photoController.PutPhoto(id, tobeUpdated).Result;

            //assert
            Assert.IsType<BadRequestResult>(result);
        }

        //[Fact]
        //public void Put_returns_notFound_for_deleted_object()
        //{
        //    //arrange
        //    DbContextMock<AlbumContext> dbContextMock = getDbContext(getInitialDbEntities());
        //    PhotoController photoController = PhotoControllerInit(dbContextMock);
        //    Photo tobeUpdated = getInitialDbEntities()[2];
        //    tobeUpdated.photoId = 4;
        //    long id = 4;
        //    dbContextMock.Setup(c => c.SaveChangesAsync(default)).Throws(new DbUpdateConcurrencyException());

        //    //act
        //    var result = photoController.PutPhoto(id, tobeUpdated).Result;


        //    //assert
        //    Assert.IsType<NotFoundResult>(result);

        //}

        [Fact]
        public void Delete_removesEntryFromContext()
        {
            long id = 3;

            PhotoController photoController = getPhotoController();

            //act
            var result = photoController.DeletePhoto(id).Result;

            //assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(dbFixture.dbContext.Photos.Find(id));
        }

        [Fact]
        public void Delete_returns_NotFound_InvalidId()
        {
            PhotoController photoController = getPhotoController();

            long id = 100;

            //act
            var result = photoController.DeletePhoto(id).Result;

            //assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Post_addsItemToDbContext()
        {
            PhotoController photoController = getPhotoController();
            Photo toBeAdded = new Photo() { photoUri = "http://testUri4.com", Title = "TestPhoto4", uploadDate = new DateTime() };

            //act
            var result = await photoController.PostPhoto(toBeAdded);

            //assert
            Assert.Equal(toBeAdded, dbFixture.dbContext.Photos.Find(toBeAdded.photoId));
        }


        [Fact]
        public async void Post_Returns_CreatedAtActionResult_type()
        {
            PhotoController photoController = getPhotoController();
            Photo toBeAdded = new Photo() { photoUri = "http://testUri4.com", Title = "TestPhoto4", uploadDate = new DateTime() };

            //act
            var result = await photoController.PostPhoto(toBeAdded);

            //assert
            var actionResult = Assert.IsType<ActionResult<Photo>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Photo>(createdAtActionResult.Value);
            Assert.Equal(toBeAdded, returnValue);
        }

    }
}
