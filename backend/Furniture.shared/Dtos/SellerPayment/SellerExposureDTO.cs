namespace Furniture.shared.Dtos.Seller
{
    public class SellerExposureDTO
    {
        public int SellerId { get; set; }
        public string StoreName { get; set; } = null!;
        public decimal PendingCommission { get; set; }        
        public decimal ReservedCashCommission { get; set; }   
        public decimal CurrentExposure { get; set; }         
        public decimal MaxAllowedCommission { get; set; }    
        public bool IsBlocked { get; set; }                  
        public bool IsOverLimit { get; set; }                 
    }
}