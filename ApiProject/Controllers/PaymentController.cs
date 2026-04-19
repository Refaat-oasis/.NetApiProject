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
                // Logic for order creation would normally be handled before this, 
                // but if it reaches here for COD, we just confirm it's fine.
                return Ok(new { success = true, message = "Order successfully placed with Cash on Delivery." });
            }
            else if (dto.PaymentMethod == "CreditCard")
            {
                var result = _paymentRepository.ProcessStripePayment(dto);

                if (result)
                {
                    return Ok(new { success = true, message = "Payment processed successfully." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Payment failed. Please check your card details and try again." });
                }
            }

            return BadRequest(new { success = false, message = "Invalid payment method." });
        }

        [HttpGet("OrderConfirmed")]
        public IActionResult OrderConfirmed()
        {
            return Ok(new { success = true, message = "Order successfully placed and confirmed." });
        }
    }
}
