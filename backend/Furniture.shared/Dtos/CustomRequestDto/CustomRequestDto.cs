using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.CustomRequestDto
{
    public class CustomRequestDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal Budget { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Status { get; set; } = null!;

    }
}
