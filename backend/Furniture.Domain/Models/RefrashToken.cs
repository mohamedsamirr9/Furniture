using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class RefrashToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
