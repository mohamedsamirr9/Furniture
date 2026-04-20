namespace Furniture.shared.Dtos.ComplaintsDto
{
    public class ComplaintReplyDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ResponderId { get; set; } = string.Empty;
        public string ResponderName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
