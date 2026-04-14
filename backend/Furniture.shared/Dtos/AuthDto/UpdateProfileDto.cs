using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.AuthDto
{
    public class UpdateProfileDto
    {
        public string Name { get; set; } = null!;
        public string? Address { get; set; }

        public string? ProfileImageBase64 { get; set; }
    }
}
