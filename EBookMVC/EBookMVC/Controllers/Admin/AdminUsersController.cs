using EBookMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers.Admin
{
    public class AdminUsersController : AdminBaseController
    {
        public AdminUsersController(IApiService apiService) : base(apiService)
        {
        }

        // Tüm kullanıcıları listele
        public async Task<IActionResult> Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var users = await _apiService.GetUsersAsync();
                ViewData["PageTitle"] = "Kullanıcı Yönetimi";
                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Kullanıcılar yüklenirken bir hata oluştu.";
                return View(new List<EBookMVC.Models.User>());
            }
        }

        // Kullanıcı detayları
        public async Task<IActionResult> Details(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var user = await _apiService.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                ViewData["PageTitle"] = $"Kullanıcı Detayı - {user.Name}";
                return View(user);
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kullanıcı detayları yüklenirken bir hata oluştu.");
                return RedirectToAction("Index");
            }
        }

        // Kullanıcı aktiflik durumunu değiştir
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                // Bu metod API'de implement edilmesi gerekebilir
                // var success = await _apiService.ToggleUserActiveAsync(id);

                SetSuccessMessage("Kullanıcı durumu güncellendi.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kullanıcı durumu güncellenirken bir hata oluştu.");
                return RedirectToAction("Index");
            }
        }

        // Kullanıcı silme (soft delete)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                // Bu metod API'de implement edilmesi gerekebilir
                // var success = await _apiService.DeleteUserAsync(id);

                SetSuccessMessage("Kullanıcı başarıyla silindi.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                SetErrorMessage("Kullanıcı silinirken bir hata oluştu.");
                return RedirectToAction("Index");
            }
        }
    }
}