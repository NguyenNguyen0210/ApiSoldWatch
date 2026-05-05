using ShopNN.Entities;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum PaymentMethod
{
    COD,
    VnPay,
    Momo,
    BankTransfer
}

public enum PaymentStatus
{
    Unpaid,
    Paid,
    Failed,
    Refunded
}

public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Payment Information
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public List<OrderItem> Items { get; set; } = new();
}