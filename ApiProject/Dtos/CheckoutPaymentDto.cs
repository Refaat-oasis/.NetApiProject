namespace ApiProject.Dtos
{
    public class CheckoutPaymentDto
    {
        public string? PaymentMethod { get; set; } // CashOnDelivery or CreditCard
        public string? Name { get; set; }
        public string? CardNumber { get; set; }
        public string? Cvv { get; set; }
        public string? ExpiryDate { get; set; }
        public string? StripeToken { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? OrderId { get; set; }
    }
}
