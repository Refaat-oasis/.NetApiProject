using ApiProject.Dtos;
using ApiProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IStripePaymentRepository _paymentRepository;

        public PaymentController(IStripePaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        [HttpPost("ProcessPayment")]
        public IActionResult ProcessPayment(CheckoutPaymentDto dto)
        {
            if (dto.PaymentMethod == "CashOnDelivery")
            {
                // Skip Stripe processing.
                // Create the order normally (Logic for order creation would go here, e.g., saving to DB)
                // Redirect to OrderConfirmed
                return RedirectToAction(nameof(OrderConfirmed));
            }
            else if (dto.PaymentMethod == "CreditCard")
            {
                // Send the DTO to the payment repository/service.
                var result = _paymentRepository.ProcessStripePayment(dto);

                if (result)
                {
                    // If payment succeeds
                    return RedirectToAction(nameof(OrderConfirmed));
                }
                else
                {
                    // If payment fails
                    // In a Web API, we typically return an error status or object
                    return BadRequest(new { message = "Payment failed. Please check your card details and try again." });
                }
            }

            return BadRequest(new { message = "Invalid payment method." });
        }

        [HttpGet("OrderConfirmed")]
        public IActionResult OrderConfirmed()
        {
            // This action returns a confirmation view/message informing the user that the order was successfully placed.
            return Ok(new { message = "Order successfully placed and confirmed." });
        }
    }
}
