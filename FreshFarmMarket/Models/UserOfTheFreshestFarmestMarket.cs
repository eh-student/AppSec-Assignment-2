using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FreshFarmMarket.Models

{
    public class UserOfTheFreshestFarmestMarket : IdentityUser
    {
        [Required]
        [PersonalData]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [PersonalData]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [PersonalData]
        [Display(Name = "Credit Card Number")]
        public string CreditCardNo { get; set; }

        [PersonalData]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; }

        [PersonalData]
        [Display(Name = "Photo")]
        public string PhotoPath { get; set; }

        [PersonalData]
        [Display(Name = "About Me")]
        public string AboutMe { get; set; }

    }
}
