using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;

using Furniture.Servises_Abstraction;

using Furniture.shared.Dtos.ShippingRule;

namespace Furniture.Services
{
    public class ShippingService : IShippingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShippingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShippingRuleDto>> GetAllAsync(string? city, int? categoryId)
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

        public async Task<ShippingRuleDto> CreateAsync(ShippingRuleCreateUpdateDto dto)
        {
            var repo = _unitOfWork.GetRepository<ShippingRule, int>();
            
            var existingSpec = new ShippingRuleSpecifications(dto.City, dto.CategoryId);
            var existing = await repo.GetAllAsync(existingSpec);
            if (existing.Any())
            {
                throw new InvalidOperationException("Shipping rule already exists for this City and Category");
            }

            var rule = _mapper.Map<ShippingRule>(dto);
            await repo.AddAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            var spec = new ShippingRuleSpecifications(rule.Id);
            var created = await repo.GetByIdAsync(spec);
            return _mapper.Map<ShippingRuleDto>(created!);
        }

        public async Task UpdateAsync(int id, ShippingRuleCreateUpdateDto dto)
        {
            var repo = _unitOfWork.GetRepository<ShippingRule, int>();
            
            var existingSpec = new ShippingRuleSpecifications(dto.City, dto.CategoryId);
            var existing = await repo.GetAllAsync(existingSpec);
            if (existing.Any(r => r.Id != id))
            {
                throw new InvalidOperationException("Shipping rule already exists for this City and Category");
            }

            var rule = await repo.GetByIdAsync(id);
            if (rule is null) throw new Exception($"ShippingRule with id {id} not found");
            _mapper.Map(dto, rule);
            repo.Update(rule);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<ShippingRule, int>();
            var rule = await repo.GetByIdAsync(id);
            if (rule is null) return;
            repo.Remove(rule);
            await _unitOfWork.SaveChangesAsync();
        }
    }

}