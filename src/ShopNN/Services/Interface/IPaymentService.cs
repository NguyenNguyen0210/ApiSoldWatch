using Microsoft.AspNetCore.Http;
using ShopNN.Entities;

namespace ShopNN.Services.Interface
{
    public interface IPaymentService
    {
        string CreatePaymentUrl(Order order, HttpContext context);
        Task<bool> ProcessVnPayReturn(IQueryCollection collections);
    }
}
