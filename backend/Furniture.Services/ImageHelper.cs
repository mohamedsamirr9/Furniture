using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public static class ImageHelper
    {
        public static string SaveImage(string base64)
        {
            var base64Data = base64.Contains(",") ? base64.Split(',')[1] : base64;

            var bytes = Convert.FromBase64String(base64Data);

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + ".png";
            var path = Path.Combine(folder, fileName);

            File.WriteAllBytes(path, bytes);

            return "/images/" + fileName;
        }
    }
}
