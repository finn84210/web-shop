using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace web_shop.Pages
{
    [Authorize]
    public class OrderhistorieModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public OrderhistorieModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IList<Order> Orders { get; private set; } = new List<Order>();

        public string CustomerName { get; private set; } = string.Empty;

        [TempData]
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            if (GetSignedInCustomerId() is null)
            {
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Orderhistorie") });
            }

            LoadData();
            return Page();
        }

        private void LoadData()
        {
            var customerId = GetSignedInCustomerId();
            CustomerName = User.Identity?.Name ?? string.Empty;

            Orders = _orderRepository.GetAllOrders()
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        private int? GetSignedInCustomerId()
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(customerIdClaim, out var customerId) ? customerId : null;
        }
    }
}
