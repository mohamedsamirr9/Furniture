using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ShippingRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class ShippingService : IShippingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShippingService(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        } 

        public  async Task<IEnumerable<ShippingRuleDto>> GetAllAsync(string? city, int? categoryId)
        {
             var repo = _unitOfWork.GetRepository<ShippingRule, int>();
               var spec = new ShippingRuleSpecifications(city, categoryId);

               var rules = await repo.GetAllAsync(spec);

             return _mapper.Map<IEnumerable<ShippingRuleDto>>(rules);
        }

        public async Task<ShippingRuleDto?> GetByIdAsync(int id)
        {

            var repo = _unitOfWork.GetRepository<ShippingRule, int>();

            var spec = new ShippingRuleSpecifications(id);

            var rule = await repo.GetByIdAsync(spec);
          if (rule is null) return null;

            return _mapper.Map<ShippingRuleDto>(rule);
        }

        public Task<ShippingRuleDto> CreateAsync(ShippingRuleCreateUpdateDto dto)
        {
            throw new NotImplementedException();
        }
        public Task UpdateAsync(int id, ShippingRuleCreateUpdateDto dto)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
