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
    //similar to base class
    public class DatabaseFixture : IDisposable
    {
        public AlbumContext dbContext { get; private set; }
        public DatabaseFixture()
        {
            var options = new DbContextOptionsBuilder<AlbumContext>().UseInMemoryDatabase("digitalAlbum").Options;
            dbContext = new AlbumContext(options);
            Photo[] initialEntities = getInitialDbEntities();

            using (dbContext)
            {
                foreach (Photo photo in initialEntities)
                {
                    dbContext.Photos.Add(photo);
                }
                dbContext.SaveChanges();
            }

            // initialize data in the test database
        }

        private Photo[] getInitialDbEntities()
        {
            return new Photo[]
             {
                new Photo {photoId = 1, photoUri="http://testUri1.com", Title="TestPhoto1", uploadDate=new DateTime()},
                new Photo {photoId = 2, photoUri="http://testUri2.com", Title="TestPhoto2", uploadDate=new DateTime()},
                new Photo {photoId = 3, photoUri="http://testUri3.com", Title="TestPhoto3", uploadDate=new DateTime()},
            };
        }

        public void Dispose()
        {
            // clean up test data from the database
            var options = new DbContextOptionsBuilder<AlbumContext>().UseInMemoryDatabase("digitalAlbum").Options;
            dbContext = new AlbumContext(options);
            dbContext.Photos.RemoveRange(getInitialDbEntities());
            dbContext.Dispose();
        }
    }

    //Class where you want to use shared class instance
    //public class MyDatabaseTests : IClassFixture<DatabaseFixture>
    //{
    //    DatabaseFixture dbFixture;

    //    public MyDatabaseTests(DatabaseFixture fixture)
    //    {
    //        this.dbFixture = fixture;
    //    }

    //    // write tests, using dbFixture.Db to get access to the SQL Server
    //}
    public class PhotoApiTests : IClassFixture<DatabaseFixture>
    {
        DatabaseFixture dbFixture;
        public PhotoApiTests(DatabaseFixture fixture)
        {
            this.dbFixture = fixture;
        }

        //private void seedAlbumContext()
        //{
        //    var options = getContextOptions();
        //    AlbumContext context = new AlbumContext(options);
        //    Photo[] initialEntities = getInitialDbEntities();

        //    using (context)
        //    {
        //        foreach (Photo photo in initialEntities)
        //        {
        //            context.Photos.Add(photo);
        //        }
        //        context.SaveChanges();
        //    }
        //}

        private DbContextOptions<AlbumContext> getContextOptions()
        {
            return new DbContextOptionsBuilder<AlbumContext>().UseInMemoryDatabase("digitalAlbum").Options;
        }

        private PhotoController getPhotoController (AlbumContext context)
        {            
            return new PhotoController(context);
        }

        private Photo[] getInitialDbEntities()
        {
            return new Photo[]
             {
                new Photo {photoId = 1, photoUri="http://testUri1.com", Title="TestPhoto1", uploadDate=new DateTime()},
                new Photo {photoId = 2, photoUri="http://testUri2.com", Title="TestPhoto2", uploadDate=new DateTime()},
                new Photo {photoId = 3, photoUri="http://testUri3.com", Title="TestPhoto3", uploadDate=new DateTime()},
            };
        }

        [Fact]
        public async void Get_returns_all_results()
        {
            //arrange
            var options = getContextOptions();

            using (var context = new AlbumContext(options))
            {
                var photoController = getPhotoController(context);
                int count = await context.Photos.CountAsync<Photo>();

                //act
                var result = photoController.GetPhotos().Result;
                List<Photo> value = result.Value as List<Photo>;

                //assert
                Assert.Equal(count, value.Count);
            }
        }



        [Fact]
        public void GetById_returns_correctResult()
        {
            //arrange
            var options = getContextOptions();
            using (var context = new AlbumContext(options))
            {
                var photoController = getPhotoController(context);

                //act
                var result = photoController.GetPhoto(1).Result;
                Photo value = result.Value;


                //assert
                Assert.IsType<Photo>(value);
                Assert.Equal(1, value.photoId);
            }

            
        }

        [Fact]
        public void GetById_returns_NotFound_invalidId()
        {
            //arrange
            var options = getContextOptions();
            using (var context = new AlbumContext(options))
            {
                var photoController = getPhotoController(context);

                //act
                var result = photoController.GetPhoto(4).Result.Result;


                //assert
                Assert.IsType<NotFoundResult>(result);
            }

            
        }

        [Fact]
        public void Put_updatesContext()
        {
            var options = getContextOptions();

            using(var context = new AlbumContext(options))
            {
                Photo tobeUpdated = getInitialDbEntities()[2];
                long id = 3;
                tobeUpdated.Title = "new Title";

                PhotoController photoController = getPhotoController(context);
                var result = photoController.PutPhoto(id, tobeUpdated);
                Assert.Equal(tobeUpdated, context.Photos.Find(id));
            }
            

        }

        [Fact]
        public void Put_returns_badRequest_invalidInput()
        {
            var options = getContextOptions();

            using (var context = new AlbumContext(options))
            {
                Photo tobeUpdated = getInitialDbEntities()[2];
                long id = 2;

                PhotoController photoController = getPhotoController(context);

                //act
                var result = photoController.PutPhoto(id, tobeUpdated).Result;

                //assert
                Assert.IsType<BadRequestResult>(result);
            }
            

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
            var options = getContextOptions();

            using (var context = new AlbumContext(options))
            {
                Photo tobeUpdated = getInitialDbEntities()[2];
                long id = 3;

                PhotoController photoController = getPhotoController(context);

                //act
                var result = photoController.DeletePhoto(id).Result;

                //assert
                Assert.IsType<NoContentResult>(result);
                Assert.Null(context.Photos.Find(id));
            }

            
        }

        [Fact]
        public void Delete_returns_NotFound_InvalidId()
        {
            var options = getContextOptions();

            using (var context = new AlbumContext(options))
            {
                PhotoController photoController = getPhotoController(context);

                long id = 4;

                //act
                var result = photoController.DeletePhoto(id).Result;

                //assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void Post_addsItemToDbContext()
        {
            var options = getContextOptions();

            using (var context = new AlbumContext(options))
            {
                PhotoController photoController = getPhotoController(context);
                Photo toBeAdded = new Photo() { photoId = 4, photoUri = "http://testUri4.com", Title = "TestPhoto4", uploadDate = new DateTime() };

                //act
                var result = await photoController.PostPhoto(toBeAdded);

                //assert
                Assert.Equal(toBeAdded, context.Photos.Find(toBeAdded.photoId));
            }

            

            
        }


        [Fact]
        public async void Post_Returns_CreatedAtActionResult_type()
        {
            var options = getContextOptions();

            using (var context = new AlbumContext(options))
            {
                PhotoController photoController = getPhotoController(context);
                Photo toBeAdded = new Photo() { photoId = 5, photoUri = "http://testUri4.com", Title = "TestPhoto4", uploadDate = new DateTime() };

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
}
