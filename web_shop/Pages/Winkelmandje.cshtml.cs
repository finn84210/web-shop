using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace web_shop.Pages
{
    [Authorize]
    public class WinkelmandjeModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public WinkelmandjeModel(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public IList<Product> Products { get; private set; } = new List<Product>();

        [BindProperty]
        public List<int> SelectedProductIds { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            Products = _productRepository.GetAllProducts().OrderBy(product => product.Name).ToList();
        }

        public IActionResult OnPost()
        {
            Products = _productRepository.GetAllProducts().OrderBy(product => product.Name).ToList();

            // Zonder ingelogde klant kan er geen bestelling geplaatst worden.
            var customerId = GetSignedInCustomerId();
            if (customerId is null)
            {
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Winkelmandje") });
            }

            if (!SelectedProductIds.Any())
            {
                ModelState.AddModelError(nameof(SelectedProductIds), "Je winkelmandje is leeg.");
                return Page();
            }

            var customer = _customerRepository.GetCustomerById(customerId.Value);
            if (customer is null)
            {
                ModelState.AddModelError(string.Empty, "Je klantaccount bestaat niet meer.");
                return Page();
            }

            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                Status = "Nieuw",
                Source = "Webshop",
                ExternalReference = $"WEB-{DateTime.Now:yyyyMMddHHmmss}"
            };

            // Elk gekozen product komt in dezelfde bestelling.
            foreach (var productId in SelectedProductIds.Distinct())
            {
                var product = _productRepository.GetProductById(productId);
                if (product is not null)
                {
                    if (product.Stock <= 0)
                    {
                        ModelState.AddModelError(nameof(SelectedProductIds), $"{product.Name} is niet meer op voorraad.");
                        return Page();
                    }

                    product.Stock -= 1;
                    order.Products.Add(product);
                }
            }

            if (!order.Products.Any())
            {
                ModelState.AddModelError(nameof(SelectedProductIds), "De gekozen producten bestaan niet meer.");
                return Page();
            }

            _orderRepository.AddOrder(order);
            foreach (var product in order.Products)
            {
                _productRepository.UpdateProduct(product);
            }

            SuccessMessage = $"Bestelling #{order.Id} is geplaatst voor {customer.Name}.";

            return RedirectToPage("/Orderhistorie");
        }

        private int? GetSignedInCustomerId()
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(customerIdClaim, out var customerId) ? customerId : null;
        }
    }
}
