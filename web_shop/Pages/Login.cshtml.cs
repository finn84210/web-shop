using DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace web_shop.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private const string DemoPassword = "matrix123";
        private readonly ICustomerRepository _customerRepository;

        public LoginModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vul je klantnaam in.")]
        [Display(Name = "Klantnaam")]
        public string CustomerName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vul je wachtwoord in.")]
        [DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;

        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = GetSafeReturnUrl(returnUrl);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = GetSafeReturnUrl(returnUrl);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var customer = _customerRepository.GetAllCustomers()
                .FirstOrDefault(c => c.Active && string.Equals(c.Name, CustomerName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (customer is null || Password != DemoPassword)
            {
                ModelState.AddModelError(string.Empty, "Deze klantnaam of dit wachtwoord klopt niet.");
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new(ClaimTypes.Name, customer.Name)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return LocalRedirect(ReturnUrl);
        }

        private string GetSafeReturnUrl(string? returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : "/";
        }
    }
}
