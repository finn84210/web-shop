using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace web_shop.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public IList<Customer> Customers { get; set; }
        public IList<Product> FeaturedProducts { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            Customers = new List<Customer>();
            FeaturedProducts = new List<Product>();
        }

        public void OnGet()
        {            
            Customers = _customerRepository.GetAllCustomers().ToList();
            FeaturedProducts = _productRepository.GetAllProducts().OrderBy(p => p.Name).ToList();
            ProductCount = FeaturedProducts.Count;
            OrderCount = _orderRepository.GetAllOrders().Count();
            _logger.LogInformation($"getting all {Customers.Count} customers");
        }
    }
}
