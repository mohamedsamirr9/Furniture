using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.AuthDto
{
    public class RegisterDto
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Address { get; set; }

        public string? NationalIdImageBase64 { get; set; }
    }
}
