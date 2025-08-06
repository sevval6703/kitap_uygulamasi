using EBookMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IApiService _apiService;

        public CategoriesController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _apiService.GetCategoriesAsync();

                ViewData["PageTitle"] = "Kategoriler";
                ViewBag.TotalCategories = categories.Count();

                return View(categories);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Kategoriler yüklenirken bir hata oluştu.";
                return View(new List<EBookMVC.Models.Category>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _apiService.GetCategoryAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                var books = await _apiService.GetBooksByCategoryAsync(id);
                category.Books = books;

                ViewData["PageTitle"] = category.Name;
                ViewBag.CategoryId = id;
                ViewBag.BookCount = books.Count();

                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kategori detayları yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }
    }
}