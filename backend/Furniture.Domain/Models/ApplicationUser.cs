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
        public string? Address { get; set; }

        public string? ProfileImage { get; set; }
        public string? NationalIdImage { get; set; }

        public Roles Role { get; set; }

        public bool IsVerified { get; set; } = false;
        public bool IsConfirmed { get; set; } = false;

        public string? OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefrashToken> RefreshTokens { get; set; } = new List<RefrashToken>();


        //rel
        public ICollection<Product> Products { get; set; }=new List<Product>();
        public ICollection<CustomRequest> CustomRequests { get; set; } = new List<CustomRequest>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<ComplaintReply> ComplaintReplies { get; set; } = new List<ComplaintReply>();

        public Cart Cart { get; set; } = null!;

        public ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();
        public SellerProfile? SellerProfile { get; set; }


    }

}
