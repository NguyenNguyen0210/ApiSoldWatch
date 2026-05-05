using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNN.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public string TransactionId { get; set; } = string.Empty; 
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "VnPay";
        public string Status { get; set; } = "Pending"; 
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaymentDate { get; set; }
    }
}
