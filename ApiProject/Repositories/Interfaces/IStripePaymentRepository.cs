using ApiProject.Dtos;

namespace ApiProject.Repositories.Interfaces
{
    public interface IStripePaymentRepository
    {
        bool ProcessStripePayment(CheckoutPaymentDto dto);
    }
}
