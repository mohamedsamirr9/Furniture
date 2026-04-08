using Furniture.Domain.Models.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string Name { get; set; } = null!;
        public Roles Role { get; set; }
        public string? Address { get; set; }
        public DateTime RegisterdAt { get; set; } = DateTime.UtcNow;
        public string? OTP { get; set; }
        public DateTime? OTPExpiry { get; set; } 
        public bool IsConfirmed { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }


        //rel
        public ICollection<Product> Products { get; set; }=new List<Product>();
        public ICollection<CustomRequest> CustomRequests { get; set; } = new List<CustomRequest>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

        public Cart Cart { get; set; } = null!;

        public ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();
        public SellerProfile? SellerProfile { get; set; }


    }

}
