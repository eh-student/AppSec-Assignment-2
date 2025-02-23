﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FreshFarmMarket.Areas.Identity.Pages.Account.Manage
{
    
    ///     
    
    /// </summary>
    public class ShowRecoveryCodesModel : PageModel
    {
        
        ///     
        
 
        [TempData]
        public string[] RecoveryCodes { get; set; }

        
        ///     
        
 
        [TempData]
        public string StatusMessage { get; set; }

        
        ///     
        
 
        public IActionResult OnGet()
        {
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return Page();
        }
    }
}
