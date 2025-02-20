using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace FreshFarmMarket.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;
        public ErrorViewModel ErrorViewModel { get; set; }

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? statusCode = null)
        {
            ErrorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode ?? 500
            };

            switch (ErrorViewModel.StatusCode)
            {
                case 404:
                    ErrorViewModel.Title = "Page Not Found";
                    ErrorViewModel.Message = "The page you are looking for doesn't exist or has been moved.";
                    break;
                case 403:
                    ErrorViewModel.Title = "Access Denied";
                    ErrorViewModel.Message = "You do not have permission to access this resource.";
                    break;
                case 400:
                    ErrorViewModel.Title = "Bad Request";
                    ErrorViewModel.Message = "Your request could not be understood by the server.";
                    break;
                default:
                    ErrorViewModel.Title = "Error";
                    ErrorViewModel.Message = "An unexpected error has occurred.";
                    break;
            }

            _logger.LogError($"Error {ErrorViewModel.StatusCode} occurred. RequestId: {ErrorViewModel.RequestId}");
        }
    }

}
