using Furniture.Domain.Models;

namespace Furniture.Services.Specifications;

public class SellerRequestByIdSpecification : BaseSpecificationscs<SellerRequest, int>
{
    public SellerRequestByIdSpecification(int id)
        : base(r => r.Id == id)
    {
        AddInclude(r => r.User);
        AddInclude("ReviewedBy");
    }
}
