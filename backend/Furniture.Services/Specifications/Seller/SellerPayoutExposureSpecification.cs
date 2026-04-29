using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Furniture.Services.Specifications
{
    public class SellerPayoutExposureSpecification : BaseSpecificationscs<SellerPayout, int>
    {
        public SellerPayoutExposureSpecification(int sellerProfileId)
            : base(p =>
                p.SellerProfileId == sellerProfileId &&
                (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing))
        {
            AddInclude(q => q.Include(p => p.Order)
                .ThenInclude(o => o.Payment));
        }
    }
}