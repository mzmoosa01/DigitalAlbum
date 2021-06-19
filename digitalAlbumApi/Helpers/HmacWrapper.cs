using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace digitalAlbumApi.Helpers
{
    public interface IHmacSha512Wrapper
    {
        public HMACSHA512 hMACSHA512 { get; }
    }
    public class HmacSha512Wrapper : IHmacSha512Wrapper
    {
        public HMACSHA512 hMACSHA512 { get; }

        public HmacSha512Wrapper()
        {
            hMACSHA512 = new HMACSHA512();
        }

        public HmacSha512Wrapper(byte[] key)
        {
            hMACSHA512 = new HMACSHA512(key);
        }
    }
}
