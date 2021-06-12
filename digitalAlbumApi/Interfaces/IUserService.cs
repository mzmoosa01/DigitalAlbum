using digitalAlbumApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitalAlbumApi.Interfaces
{
    public interface IUserService
    {
        User CreateUser(User user, string password);
        User AuthenticateUser(string email, string password);
        User GetById(long id);
    }
}
