using EBookMVC.Models;
using EBookMVC.Services;
using EBookMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;

        public HomeController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                var categories = await _apiService.GetCategoriesAsync();
                var books = await _apiService.GetBooksAsync();

                viewModel.FeaturedCategories = categories.Take(4).ToList();
                viewModel.FeaturedBooks = books.OrderByDescending(b => b.CreatedDate).Take(8).ToList();
                viewModel.NewBooks = books.OrderByDescending(b => b.CreatedDate).Take(4).ToList();

                ViewBag.WelcomeMessage = "E-Kitap Dünyasýna Hoþ Geldiniz!";
                ViewData["PageTitle"] = "Ana Sayfa";
                TempData["LastVisit"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Veriler yüklenirken bir hata oluþtu.";
            }

            return View(viewModel);
        }

        public IActionResult About()
        {
            ViewData["PageTitle"] = "Hakkýmýzda";
            ViewBag.CompanyInfo = "E-Kitap Maðazamýz 2020 yýlýndan beri hizmet vermektedir.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["PageTitle"] = "Bize Ulaþýn";
            ViewBag.ContactInfo = new
            {
                Email = "info@ekitap.com",
                Phone = "+90 555 123 45 67",
                Address = "Ýstanbul, Türkiye"
            };

            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = "Mesajýnýz baþarýyla gönderildi. En kýsa sürede geri döneceðiz.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanlarý doldurun.";
            }

            return RedirectToAction("Contact");
        }
    }
}