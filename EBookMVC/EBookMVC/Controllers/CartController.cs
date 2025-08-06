using EBookMVC.Models;
using EBookMVC.Services;
using EBookMVC.ViewModels;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EBookMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly IApiService _apiService;

        public CartController(CartService cartService, IApiService apiService)
        {
            _cartService = cartService;
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewData["PageTitle"] = "Sepetim";
            ViewBag.CartTotal = cart.TotalAmount;

            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int bookId, int quantity)
        {
            _cartService.UpdateQuantity(bookId, quantity);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RemoveItem(int bookId)
        {
            _cartService.RemoveFromCart(bookId);
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                cartCount = cart.TotalItems,
                cartTotal = cart.TotalAmount
            });
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş. Önce ürün ekleyin.";
                return RedirectToAction("Index", "Books");
            }

            var viewModel = new CheckoutViewModel
            {
                Cart = cart
            };

            ViewData["PageTitle"] = "Siparişi Tamamla";
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Cart = _cartService.GetCart();
                return View(model);
            }

            try
            {
                var cart = _cartService.GetCart();

                // Sipariş oluştur
                var order = new Order
                {
                    UserId = 1, // Geçici olarak 1 kullanıyoruz
                    TotalAmount = cart.TotalAmount,
                    Status = "Pending",
                    DeliveryAddress = model.Address,
                    OrderDate = DateTime.Now,
                    OrderDetails = cart.Items.Select(item => new OrderDetail
                    {
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                };

                var createdOrder = await _apiService.CreateOrderAsync(order);
                if (createdOrder != null)
                {
                    _cartService.ClearCart();
                    TempData["SuccessMessage"] = $"Siparişiniz başarıyla oluşturuldu. Sipariş No: {createdOrder.Id}";
                    ViewBag.OrderId = createdOrder.Id;

                    return View("OrderSuccess", createdOrder);
                }
                else
                {
                    TempData["ErrorMessage"] = "Sipariş oluşturulurken bir hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sipariş işlemi sırasında bir hata oluştu.";
            }

            model.Cart = _cartService.GetCart();
            return View(model);
        }

        public IActionResult GetCartCount()
        {
            var cart = _cartService.GetCart();
            return Json(new { count = cart.TotalItems });
        }
    }
}