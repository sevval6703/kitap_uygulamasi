using EBookMVC.Models;
using EBookMVC.Services;
using EBookMVC.ViewModels;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly IApiService _apiService;
        private readonly CartService _cartService;

        public BooksController(IApiService apiService, CartService cartService)
        {
            _apiService = apiService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index(int? categoryId, string searchTerm, string sortBy)
        {
            var viewModel = new BookListViewModel();

            try
            {
                var categories = await _apiService.GetCategoriesAsync();
                var books = await _apiService.GetBooksAsync();

                // Filter by category
                if (categoryId.HasValue)
                {
                    books = books.Where(b => b.CategoryId == categoryId.Value).ToList();
                    viewModel.SelectedCategoryId = categoryId;
                }

                // Search
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    books = books.Where(b =>
                        b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                    viewModel.SearchTerm = searchTerm;
                }

                // Sort
                switch (sortBy)
                {
                    case "price_asc":
                        books = books.OrderBy(b => b.Price).ToList();
                        break;
                    case "price_desc":
                        books = books.OrderByDescending(b => b.Price).ToList();
                        break;
                    case "name_asc":
                        books = books.OrderBy(b => b.Title).ToList();
                        break;
                    case "name_desc":
                        books = books.OrderByDescending(b => b.Title).ToList();
                        break;
                    default:
                        books = books.OrderByDescending(b => b.CreatedDate).ToList();
                        break;
                }

                viewModel.Books = books;
                viewModel.Categories = categories;
                viewModel.SortBy = sortBy;

                ViewData["PageTitle"] = "Kitaplar";
                ViewBag.TotalBooks = books.Count();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Kitaplar yüklenirken bir hata oluştu.";
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var book = await _apiService.GetBookAsync(id);
                if (book == null)
                {
                    return NotFound();
                }

                var relatedBooks = await _apiService.GetBooksByCategoryAsync(book.CategoryId);
                relatedBooks = relatedBooks.Where(b => b.Id != id).Take(4).ToList();

                var viewModel = new BookDetailViewModel
                {
                    Book = book,
                    RelatedBooks = relatedBooks,
                    IsFavorite = false // Bu kısmı kullanıcı sistemi ile entegre edebilirsiniz
                };

                ViewData["PageTitle"] = book.Title;
                ViewBag.BookId = id;
                TempData["ViewedBook"] = $"{book.Title} kitabını görüntülediniz.";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kitap detayları yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            try
            {
                var book = await _apiService.GetBookAsync(bookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "Kitap bulunamadı." });
                }

                var cartItem = new CartItem
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Price = book.Price,
                    ImageUrl = book.ImageUrl,
                    Quantity = quantity
                };

                _cartService.AddToCart(cartItem);

                var cart = _cartService.GetCart();

                return Json(new
                {
                    success = true,
                    message = "Kitap sepete eklendi.",
                    cartCount = cart.TotalItems
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int bookId)
        {
            try
            {
                // Burada kullanıcı ID'sini session'dan alabilirsiniz
                int userId = 1; // Geçici olarak 1 kullanıyoruz

                var favorite = new Favorite
                {
                    UserId = userId,
                    BookId = bookId,
                    CreatedDate = DateTime.Now
                };

                var result = await _apiService.AddToFavoritesAsync(favorite);
                if (result != null)
                {
                    return Json(new { success = true, message = "Kitap favorilere eklendi." });
                }
                else
                {
                    return Json(new { success = false, message = "Kitap zaten favorilerde." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }
    }
}