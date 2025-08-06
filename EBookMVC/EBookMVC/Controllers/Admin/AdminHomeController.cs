using EBookMVC.Services;
using EBookMVC.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers.Admin
{
    public class AdminHomeController : AdminBaseController
    {
        public AdminHomeController(IApiService apiService) : base(apiService)
        {
        }

        public async Task<IActionResult> Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var users = await _apiService.GetUsersAsync();
                var books = await _apiService.GetBooksAsync();
                var categories = await _apiService.GetCategoriesAsync();
                var orders = await _apiService.GetOrdersAsync();

                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsers = users.Count,
                    TotalBooks = books.Count,
                    TotalCategories = categories.Count,
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.TotalAmount),
                    RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList(),
                    LowStockBooks = books.Where(b => b.Stock < 10).ToList()
                };

                ViewData["PageTitle"] = "Admin Paneli";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Dashboard verileri yüklenirken bir hata oluştu.";
                return View(new AdminDashboardViewModel());
            }
        }

        public IActionResult Login()
        {
            ViewData["PageTitle"] = "Admin Girişi";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Basit admin kontrolü - gerçek uygulamada hash'lenmiş şifre kontrolü yapın
            if (email == "admin@ebook.com" && password == "admin123")
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("UserEmail", email);
                SetSuccessMessage("Başarıyla giriş yaptınız.");
                return RedirectToAction("Index");
            }

            SetErrorMessage("Geçersiz email veya şifre.");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            SetSuccessMessage("Başarıyla çıkış yaptınız.");
            return RedirectToAction("Login");
        }
    }
}