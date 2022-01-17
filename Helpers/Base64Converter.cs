using System;
using System.IO;

namespace HAFD.Helpers
{
    public static class Base64Converter
    {
        public static byte[] ToBase64(string path)
        {
            return File.ReadAllBytes(path);
            //byte[] imageArray = File.ReadAllBytes(path);
            //return Convert.ToBase64String(imageArray);
        }

        public static byte[] FromBase64(string base64String)
        {
            return  Convert.FromBase64String(base64String);
        }
    }
}
