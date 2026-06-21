using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace web_shop.Pages
{
    public class ProductDetailsModel : PageModel
    {
        private readonly IProductRepository _productRepository;

        public ProductDetailsModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Product Product { get; private set; } = null!;

        public IActionResult OnGet(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product is null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }
    }
}
