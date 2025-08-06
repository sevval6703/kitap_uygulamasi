using EBookMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers.Admin
{
    public abstract class AdminBaseController : Controller
    {
        protected readonly IApiService _apiService;

        public AdminBaseController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Admin authentication kontrol
        protected bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // Admin erişim kontrolü
        protected IActionResult CheckAdminAccess()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "AdminHome");
            }
            return null;
        }

        // Hata mesajı set etme
        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        // Başarı mesajı set etme
        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }
    }
}