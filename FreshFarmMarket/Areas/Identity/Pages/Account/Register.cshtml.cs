// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FreshFarmMarket.Models;
using FreshFarmMarket.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;


namespace FreshFarmMarket.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<UserOfTheFreshestFarmestMarket> _signInManager;
        private readonly UserManager<UserOfTheFreshestFarmestMarket> _userManager;
        private readonly IUserStore<UserOfTheFreshestFarmestMarket> _userStore;
        private readonly IUserEmailStore<UserOfTheFreshestFarmestMarket> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IEncryptionService _encryptionService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public RegisterModel(
            UserManager<UserOfTheFreshestFarmestMarket> userManager,
            IUserStore<UserOfTheFreshestFarmestMarket> userStore,
            SignInManager<UserOfTheFreshestFarmestMarket> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IEncryptionService encryptionService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _encryptionService = encryptionService;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        
        ///     
     
  
        [BindProperty]
        public InputModel Input { get; set; }

        
        ///     
     
  
        public string ReturnUrl { get; set; }

        
        ///     
     
  
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        
        ///     
     
  
        public class InputModel
        {

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Full Name")]
            [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Full Name can only contain letters and spaces")]
            public string FullName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [DataType(DataType.CreditCard)]
            [Display(Name = "Credit Card Number")]
            public string CreditCardNo { get; set; }

            [Display(Name = "Mobile Number")]
            [RegularExpression(@"^[0-9]*$", ErrorMessage = "Mobile number must contain only digits")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Delivery Address")]
            public string DeliveryAddress { get; set; }

            [Display(Name = "Photo")]
            [AllowedExtensions(new string[] { ".jpg" })]
            [MaxFileSize(5 * 1024 * 1024)] // 5MB max
            public IFormFile Photo { get; set; }

            [Display(Name = "About Me")]
            [MaxLength(100)]
            [DataType(DataType.MultilineText)]
            public string AboutMe { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }


            [Required]
            public string RecaptchaToken { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // Validate reCAPTCHA
                if (!await ValidateRecaptcha(Input.RecaptchaToken))
                {
                    ModelState.AddModelError(string.Empty, "Invalid reCAPTCHA verification.");
                    return Page();
                }

                var user = CreateUser();
                // Set required fields
                user.FullName = Input.FullName;
                user.Gender = Input.Gender;

                // Handle file upload
                if (Input.Photo != null)
                {
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.Photo.CopyToAsync(stream);
                    }
                    user.PhotoPath = fileName;
                }
                else
                {
                    user.PhotoPath = "";
                }

                // Encrypt credit card only if provided
                if (!string.IsNullOrEmpty(Input.CreditCardNo))
                {
                    user.CreditCardNo = _encryptionService.EncryptData(Input.CreditCardNo);
                }
                else
                {
                    user.CreditCardNo = string.Empty;
                }

                // Sanitize AboutMe
                if (!string.IsNullOrEmpty(Input.AboutMe))
                {
                    user.AboutMe = HtmlEncoder.Default.Encode(Input.AboutMe);
                }

                // Set optional fields with default values if null
                user.PhoneNumber = Input.PhoneNumber ?? "";
                user.DeliveryAddress = Input.DeliveryAddress ?? "";
                user.AboutMe = !string.IsNullOrEmpty(Input.AboutMe) ?
                    HtmlEncoder.Default.Encode(Input.AboutMe) : "";

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private UserOfTheFreshestFarmestMarket CreateUser()
        {
            try
            {
                return Activator.CreateInstance<UserOfTheFreshestFarmestMarket>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(UserOfTheFreshestFarmestMarket)}'. " +
                    $"Ensure that '{nameof(UserOfTheFreshestFarmestMarket)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<UserOfTheFreshestFarmestMarket> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<UserOfTheFreshestFarmestMarket>)_userStore;
        }

        private async Task<bool> ValidateRecaptcha(string token)
        {
            var response = await _httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                {"secret", _configuration["RecaptchaSettings:SecretKey"]},
                {"response", token}
                }));

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(jsonString);
                return recaptchaResponse.Success && recaptchaResponse.Score >= 0.5;
            }

            return false;
        }

        public class RecaptchaResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public float Score { get; set; }

            [JsonPropertyName("action")]
            public string Action { get; set; }

            [JsonPropertyName("error-codes")]
            public string[] ErrorCodes { get; set; }
        }
    }


}
