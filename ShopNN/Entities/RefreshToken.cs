namespace ShopNN.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
