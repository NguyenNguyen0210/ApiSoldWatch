using System.ComponentModel.DataAnnotations;
using ShopNN.Shared.Enums;

namespace ShopNN.DTOs
{
    public class OrderQueryDTO
    {
        public OrderStatus? Status { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? SortBy { get; set; } = "date";

        public string? SortOrder { get; set; } = "desc";

        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
    }
}
