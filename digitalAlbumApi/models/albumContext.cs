using Microsoft.EntityFrameworkCore;

namespace digitalAlbumApi.Models
{
    public class AlbumContext : DbContext
    {
        public AlbumContext(DbContextOptions<AlbumContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Photo> Photos { get; set; }
    }
}