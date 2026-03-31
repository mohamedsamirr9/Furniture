using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Cart
    {
        public int Id { get; set; }
        //public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        //rel
        public ApplicationUser User { get; set; } = null!;
        public string UserId { get; set; } = null!;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    }
}
