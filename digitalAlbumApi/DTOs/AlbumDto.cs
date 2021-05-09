using System;


namespace digitalAlbumApi.Models
{
    public class UploadPhotoDto
    {
        public string Title { get; set; }
        public DateTime uploadDate { get; set; }
        public string photoUri {get; set;}
    }
}