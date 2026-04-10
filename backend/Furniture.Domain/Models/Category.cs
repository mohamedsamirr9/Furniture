using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = null!;
        public string? NameAr { get; set; }
        public string DescriptionEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? Image { get; set; }
        public DateTime Created_At { get; set; }

        //rel
        public ICollection<Product> Products { get; set; }= new List<Product>();

    }
}
