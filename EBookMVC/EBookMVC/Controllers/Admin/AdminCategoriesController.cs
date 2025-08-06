using EBookMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers.Admin
{
    public class AdminCategoriesController : AdminBaseController
    {
        public AdminCategoriesController(IApiService apiService) : base(apiService)
        {
        }

        // Tüm kategorileri listele
        public async Task<IActionResult> Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var categories = await _apiService.GetCategoriesAsync();
                ViewData["PageTitle"] = "Kategori Yönetimi";
                return View(categories);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Kategoriler yüklenirken bir hata oluştu.";
                return View(new List<EBookMVC.Models.Category>());
            }
        }

        // Yeni kategori ekleme formu
        public IActionResult Create()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            ViewData["PageTitle"] = "Yeni Kategori Ekle";
            return View();
        }

        // Yeni kategori ekleme işlemi
        [HttpPost]
        public async Task<IActionResult> Create(EBookMVC.Models.Category model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.CreatedDate = DateTime.Now;
                var createdCategory = await _apiService.CreateCategoryAsync(model);

                if (createdCategory != null)
                {
                    SetSuccessMessage("Kategori başarıyla eklendi.");
                    return RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage("Kategori eklenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kategori ekleme işlemi sırasında bir hata oluştu.");
            }

            return View(model);
        }

        // Kategori düzenleme formu
        public async Task<IActionResult> Edit(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var category = await _apiService.GetCategoryAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                ViewData["PageTitle"] = "Kategori Düzenle";
                return View(category);
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kategori bilgileri yüklenirken bir hata oluştu.");
                return RedirectToAction("Index");
            }
        }

        // Kategori düzenleme işlemi
        [HttpPost]
        public async Task<IActionResult> Edit(EBookMVC.Models.Category model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _apiService.UpdateCategoryAsync(model.Id, model);
                if (success)
                {
                    SetSuccessMessage("Kategori başarıyla güncellendi.");
                    return RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage("Kategori güncellenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kategori güncelleme işlemi sırasında bir hata oluştu.");
            }

            return View(model);
        }

        // Kategori silme işlemi
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var success = await _apiService.DeleteCategoryAsync(id);
                if (success)
                {
                    SetSuccessMessage("Kategori başarıyla silindi.");
                }
                else
                {
                    SetErrorMessage("Kategori silinirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kategori silme işlemi sırasında bir hata oluştu.");
            }

            return RedirectToAction("Index");
        }
    }
}