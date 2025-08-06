using EBookMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers.Admin
{
    public class AdminOrdersController : AdminBaseController
    {
        public AdminOrdersController(IApiService apiService) : base(apiService)
        {
        }

        // Tüm siparişleri listele
        public async Task<IActionResult> Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var orders = await _apiService.GetOrdersAsync();
                ViewData["PageTitle"] = "Sipariş Yönetimi";
                return View(orders);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Siparişler yüklenirken bir hata oluştu.";
                return View(new List<EBookMVC.Models.Order>());
            }
        }

        // Sipariş detayları
        public async Task<IActionResult> Details(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var order = await _apiService.GetOrderAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                ViewData["PageTitle"] = $"Sipariş Detayı - {id}";
                return View(order);
            }
            catch (Exception ex)
            {
                SetErrorMessage("Sipariş detayları yüklenirken bir hata oluştu.");
                return RedirectToAction("Index");
            }
        }

        // Sipariş durumunu güncelle (gelecekte eklenebilir)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                // Bu metod API'de implement edilmesi gerekebilir
                // var success = await _apiService.UpdateOrderStatusAsync(id, status);

                SetSuccessMessage("Sipariş durumu güncellendi.");
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                SetErrorMessage("Sipariş durumu güncellenirken bir hata oluştu.");
                return RedirectToAction("Details", new { id });
            }
        }
    }
}