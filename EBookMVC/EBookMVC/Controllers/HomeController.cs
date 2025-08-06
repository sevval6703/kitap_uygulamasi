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

                ViewBag.WelcomeMessage = "E-Kitap D�nyas�na Ho� Geldiniz!";
                ViewData["PageTitle"] = "Ana Sayfa";
                TempData["LastVisit"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Veriler y�klenirken bir hata olu�tu.";
            }

            return View(viewModel);
        }

        public IActionResult About()
        {
            ViewData["PageTitle"] = "Hakk�m�zda";
            ViewBag.CompanyInfo = "E-Kitap Ma�azam�z 2020 y�l�ndan beri hizmet vermektedir.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["PageTitle"] = "Bize Ula��n";
            ViewBag.ContactInfo = new
            {
                Email = "info@ekitap.com",
                Phone = "+90 555 123 45 67",
                Address = "�stanbul, T�rkiye"
            };

            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = "Mesaj�n�z ba�ar�yla g�nderildi. En k�sa s�rede geri d�nece�iz.";
            }
            else
            {
                TempData["ErrorMessage"] = "L�tfen t�m alanlar� doldurun.";
            }

            return RedirectToAction("Contact");
        }
    }
}