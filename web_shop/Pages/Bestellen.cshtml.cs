using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace web_shop.Pages
{
    [Authorize]
    public class BestellenModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public BestellenModel(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public IList<Product> Products { get; private set; } = new List<Product>();

        public IList<string> Categories { get; private set; } = new List<string>();

        public string CustomerName { get; private set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(0, 999999.99, ErrorMessage = "Minimumprijs mag niet negatief zijn.")]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(0, 999999.99, ErrorMessage = "Maximumprijs mag niet negatief zijn.")]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "name";

        [BindProperty]
        public List<int> SelectedProductIds { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            LoadFormData();
        }

        public IActionResult OnPost()
        {
            LoadFormData();

            // Eerst controleren of er een klant is ingelogd.
            var customerId = GetSignedInCustomerId();
            if (customerId is null)
            {
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Bestellen") });
            }

            if (!SelectedProductIds.Any())
            {
                ModelState.AddModelError(nameof(SelectedProductIds), "Kies minimaal een product.");
            }

            if (!ModelState.IsValid)
            {
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

            // De gekozen producten worden aan de bestelling gekoppeld.
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

            // Voor elk besteld product gaat de voorraad met 1 omlaag.
            foreach (var product in order.Products)
            {
                _productRepository.UpdateProduct(product);
            }

            SuccessMessage = $"Bestelling #{order.Id} is geplaatst voor {customer.Name}.";

            return RedirectToPage("/Orderhistorie");
        }

        private void LoadFormData()
        {
            var allProducts = _productRepository.GetAllProducts().ToList();
            Categories = allProducts
                .Select(product => product.Category)
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(category => category)
                .ToList();

            var products = allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                products = products.Where(product =>
                    product.Name.Contains(SearchTerm.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    product.Description.Contains(SearchTerm.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(Category))
            {
                products = products.Where(product =>
                    string.Equals(product.Category, Category.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (MinPrice.HasValue)
            {
                products = products.Where(product => product.Price >= MinPrice.Value);
            }

            if (MaxPrice.HasValue)
            {
                products = products.Where(product => product.Price <= MaxPrice.Value);
            }

            Products = SortBy switch
            {
                "price-asc" => products.OrderBy(product => product.Price).ThenBy(product => product.Name).ToList(),
                "price-desc" => products.OrderByDescending(product => product.Price).ThenBy(product => product.Name).ToList(),
                "stock" => products.OrderByDescending(product => product.Stock).ThenBy(product => product.Name).ToList(),
                _ => products.OrderBy(product => product.Name).ToList()
            };

            CustomerName = User.Identity?.Name ?? string.Empty;
        }

        private int? GetSignedInCustomerId()
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(customerIdClaim, out var customerId) ? customerId : null;
        }
    }
}
