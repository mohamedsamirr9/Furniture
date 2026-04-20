namespace Furniture.Domain.Models
{
    public class ComplaintReply
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; } = null!;

        public string ResponderId { get; set; } = null!;
        public ApplicationUser Responder { get; set; } = null!;

        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
