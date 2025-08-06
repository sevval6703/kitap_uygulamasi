using EBookMVC.Services;
using EBookMVC.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers.Admin
{
    public class AdminBooksController : AdminBaseController
    {
        public AdminBooksController(IApiService apiService) : base(apiService)
        {
        }

        // Tüm kitapları listele
        public async Task<IActionResult> Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var books = await _apiService.GetBooksAsync();
                ViewData["PageTitle"] = "Kitap Yönetimi";
                return View(books);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Kitaplar yüklenirken bir hata oluştu.";
                return View(new List<EBookMVC.Models.Book>());
            }
        }

        // Yeni kitap ekleme formu
        public async Task<IActionResult> Create()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var categories = await _apiService.GetCategoriesAsync();
            var viewModel = new BookCreateViewModel
            {
                Categories = categories
            };

            ViewData["PageTitle"] = "Yeni Kitap Ekle";
            return View(viewModel);
        }

        // Yeni kitap ekleme işlemi
        [HttpPost]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (!ModelState.IsValid)
            {
                model.Categories = await _apiService.GetCategoriesAsync();
                return View(model);
            }

            try
            {
                string imageUrl = "/images/books/default.jpg";

                // Resim upload işlemi
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageUrl = await UploadImageAsync(model.ImageFile);
                }

                var book = new EBookMVC.Models.Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    Stock = model.Stock,
                    ImageUrl = imageUrl,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                var createdBook = await _apiService.CreateBookAsync(book);
                if (createdBook != null)
                {
                    SetSuccessMessage("Kitap başarıyla eklendi.");
                    return RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage("Kitap eklenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kitap ekleme işlemi sırasında bir hata oluştu.");
            }

            model.Categories = await _apiService.GetCategoriesAsync();
            return View(model);
        }

        // Kitap düzenleme formu
        public async Task<IActionResult> Edit(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var book = await _apiService.GetBookAsync(id);
                if (book == null)
                {
                    return NotFound();
                }

                var categories = await _apiService.GetCategoriesAsync();
                var viewModel = new BookEditViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Description = book.Description,
                    Price = book.Price,
                    CategoryId = book.CategoryId,
                    Stock = book.Stock,
                    CurrentImageUrl = book.ImageUrl,
                    Categories = categories
                };

                ViewData["PageTitle"] = "Kitap Düzenle";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kitap bilgileri yüklenirken bir hata oluştu.");
                return RedirectToAction("Index");
            }
        }

        // Kitap düzenleme işlemi
        [HttpPost]
        public async Task<IActionResult> Edit(BookEditViewModel model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (!ModelState.IsValid)
            {
                model.Categories = await _apiService.GetCategoriesAsync();
                return View(model);
            }

            try
            {
                var existingBook = await _apiService.GetBookAsync(model.Id);
                if (existingBook == null)
                {
                    return NotFound();
                }

                string imageUrl = model.CurrentImageUrl;

                // Yeni resim yüklendiyse
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageUrl = await UploadImageAsync(model.ImageFile);

                    // Eski resmi sil (default değilse)
                    if (!string.IsNullOrEmpty(model.CurrentImageUrl) && !model.CurrentImageUrl.Contains("default.jpg"))
                    {
                        DeleteImage(model.CurrentImageUrl);
                    }
                }

                var book = new EBookMVC.Models.Book
                {
                    Id = model.Id,
                    Title = model.Title,
                    Author = model.Author,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    Stock = model.Stock,
                    ImageUrl = imageUrl,
                    IsActive = existingBook.IsActive,
                    CreatedDate = existingBook.CreatedDate
                };

                var success = await _apiService.UpdateBookAsync(model.Id, book);
                if (success)
                {
                    SetSuccessMessage("Kitap başarıyla güncellendi.");
                    return RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage("Kitap güncellenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kitap güncelleme işlemi sırasında bir hata oluştu.");
            }

            model.Categories = await _apiService.GetCategoriesAsync();
            return View(model);
        }

        // Kitap silme işlemi
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var success = await _apiService.DeleteBookAsync(id);
                if (success)
                {
                    SetSuccessMessage("Kitap başarıyla silindi.");
                }
                else
                {
                    SetErrorMessage("Kitap silinirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kitap silme işlemi sırasında bir hata oluştu.");
            }

            return RedirectToAction("Index");
        }

        // Resim upload işlemi
        private async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/books/{uniqueFileName}";
        }

        // Resim silme işlemi
        private void DeleteImage(string imageUrl)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}