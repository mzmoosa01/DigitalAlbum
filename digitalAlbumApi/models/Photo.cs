using System;
using System.ComponentModel.DataAnnotations;


namespace digitalAlbumApi.Models
{
    public class Photo
    {
        [Key]
        public long photoId { get; set; }
        public string Title { get; set; }
        public DateTime uploadDate { get; set; }
        public string photoUri {get; set;}
    }
}