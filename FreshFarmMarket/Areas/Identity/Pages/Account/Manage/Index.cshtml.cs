// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Models;
using FreshFarmMarket.Services;

namespace FreshFarmMarket.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<UserOfTheFreshestFarmestMarket> _userManager;
        private readonly SignInManager<UserOfTheFreshestFarmestMarket> _signInManager;
        private readonly IEncryptionService _encryptionService;

        public IndexModel(
            UserManager<UserOfTheFreshestFarmestMarket> userManager,
            SignInManager<UserOfTheFreshestFarmestMarket> signInManager,
            IEncryptionService encryptionService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _encryptionService = encryptionService;
        }

        
        ///     
     
  
        public string Username { get; set; }

        
        ///     
     
  
        [TempData]
        public string StatusMessage { get; set; }

        
        ///     
     
  
        [BindProperty]
        public InputModel Input { get; set; }

        
        ///     
     
  
        public class InputModel
        {
            [Display(Name = "Current Photo")]
            public string PhotoPath { get; set; }

            [Display(Name = "New Photo")]
            [AllowedExtensions(new string[] { ".jpg" })]
            public IFormFile Photo { get; set; }

            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [Display(Name = "About Me")]
            [DataType(DataType.MultilineText)]
            public string AboutMe { get; set; }

            [Display(Name = "Mobile Number")]
            [RegularExpression(@"^[0-9]*$", ErrorMessage = "Mobile number must contain only digits")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Credit Card Number")]
            [DataType(DataType.CreditCard)]
            public string CreditCardNo { get; set; }


            [Display(Name = "Delivery Address")]
            public string DeliveryAddress { get; set; }


        }

        private async Task LoadAsync(UserOfTheFreshestFarmestMarket user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = phoneNumber,
                CreditCardNo = _encryptionService.DecryptData(user.CreditCardNo),
                DeliveryAddress = user.DeliveryAddress,
                AboutMe = user.AboutMe,
                PhotoPath = user.PhotoPath
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }

            if (Input.Gender != user.Gender)
            {
                user.Gender = Input.Gender;
            }

            if (!string.IsNullOrEmpty(Input.CreditCardNo) && Input.CreditCardNo != _encryptionService.DecryptData(user.CreditCardNo))
            {
                user.CreditCardNo = _encryptionService.EncryptData(Input.CreditCardNo);
            }

            if (Input.DeliveryAddress != user.DeliveryAddress)
            {
                user.DeliveryAddress = Input.DeliveryAddress ?? user.DeliveryAddress;
            }

            if (Input.AboutMe != user.AboutMe)
            {
                user.AboutMe = !string.IsNullOrEmpty(Input.AboutMe) ?
                    HtmlEncoder.Default.Encode(Input.AboutMe) : user.AboutMe;
            }

            // Handle photo upload
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

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
