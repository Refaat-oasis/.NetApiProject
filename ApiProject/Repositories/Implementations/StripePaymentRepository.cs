using ApiProject.Dtos;
using ApiProject.Repositories.Interfaces;
using Stripe;

namespace ApiProject.Repositories.Implementations
{
    public class StripePaymentRepository : IStripePaymentRepository
    {
        public bool ProcessStripePayment(CheckoutPaymentDto dto)
        {
            try
            {
                // 1. Create a Customer using StripeCustomerService
                var customerService = new StripeCustomerService();
                var customerOptions = new StripeCustomerCreateOptions
                {
                    Email = dto.Name?.Replace(" ", "").ToLower() + "@example.com",
                    Description = dto.Name, // Using Description instead of Name for v10
                    SourceToken = dto.StripeToken 
                };
                var customer = customerService.Create(customerOptions);

                // 4. Create a Charge using StripeChargeService
                var chargeService = new StripeChargeService();
                var chargeOptions = new StripeChargeCreateOptions
                {
                    Amount = (int)(dto.Amount * 100), // v10 uses int for Amount
                    Currency = dto.Currency?.ToLower() ?? "usd",
                    CustomerId = customer.Id,
                    Description = $"Payment for Order ID: {dto.OrderId}"
                };

                var charge = chargeService.Create(chargeOptions);

                // 5. Handle success/failure
                return charge.Status == "succeeded";
            }
            catch (StripeException ex)
            {
                // Handle exceptions properly
                // Log exception here if a logger is available
                Console.WriteLine($"Stripe error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                return false;
            }
        }
    }
}
