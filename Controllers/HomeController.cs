using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlinePazar.Models;
using System.Diagnostics;
using System.Security.Cryptography;

namespace OnlinePazar.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index(string? MessageCssClass, string? Message)
        {
            ViewData["Title"] = "Ana Sayfa";
            ViewBag.SepetUrunSayisi = GetSepetUrunSayisi();
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT * FROM products").ToList();

            ViewBag.Message = Message;
            ViewBag.MessageCssClass = MessageCssClass;

            return View(products);
        }

        [HttpPost]
        public IActionResult Sepet(int Id)
        {
            using var connection = new SqlConnection(connectionString);
            var product = connection.QueryFirstOrDefault<Product>("SELECT * FROM products WHERE Id = @Id", new { Id });

            if (product == null)
            {
                return NotFound(); // Ürün bulunamadıysa hata döndür
            }

            var existingItem = connection.QueryFirstOrDefault<Sepet>("SELECT * FROM Sepet WHERE UrunId = @Id", new { Id });
            if (existingItem != null)
            {
                existingItem.UrunAdet++;
                var sqlUpdate = "UPDATE Sepet SET UrunAdet = @UrunAdet WHERE UrunId = @UrunId";
                connection.Execute(sqlUpdate, new { existingItem.UrunAdet, UrunId = Id });
            }
            else
            {
                var sqlInsert = "INSERT INTO Sepet (UrunId, UrunAdi, UrunFiyat, UrunAdet) VALUES (@UrunId, @UrunAdi, @UrunFiyat, @UrunAdet)";
                var data = new
                {
                    UrunId = Id,
                    UrunAdi = product.Name,
                    UrunFiyat = product.Price,
                    UrunAdet = 1,
                };
                connection.Execute(sqlInsert, data);
            }

            product.Stock--;
            var sqlStock = "UPDATE products SET Stock = @Stock WHERE Id = @Id";
            connection.Execute(sqlStock, new { Id, product.Stock });

            return RedirectToAction("Index", new { MessageCssClass = "alert-success" , Message = $"1 Kilo {product.Name} sepete eklenmiştir"});
        }

        public IActionResult SepetIndex()
        {
            ViewData["Title"] = "Sepet";
            ViewBag.SepetUrunSayisi = GetSepetUrunSayisi();
            using var connection = new SqlConnection(connectionString);
            var cartItems = connection.Query<Sepet>("SELECT * FROM Sepet").ToList();
            if (!cartItems.Any())
            {
                ViewBag.Message = "Sepetinizde ürün bulunmamaktadır.";
            }
            return View(cartItems);
        }

        public IActionResult SepetSil(int id)
        {
            using var connection = new SqlConnection(connectionString);

            // Sepetten ürünü getir
            var item = connection.QueryFirstOrDefault<Sepet>("SELECT * FROM Sepet WHERE Id = @Id", new { Id = id });

            if (item != null)
            {
                // Ürünü ürün tablosundan getir
                var product = connection.QueryFirstOrDefault<Product>("SELECT * FROM products WHERE Id = @Id", new { Id = item.UrunId });

                if (product != null)
                {
                    if (item.UrunAdet > 1)
                    {
                        // Sepetteki ürün adedini azalt
                        item.UrunAdet--;
                        var sqlUpdateSepet = "UPDATE Sepet SET UrunAdet = @UrunAdet WHERE Id = @Id";
                        connection.Execute(sqlUpdateSepet, new { item.UrunAdet, Id = id });

                        // Ürün stoğunu bir artır
                        product.Stock++;
                        var sqlUpdateProduct = "UPDATE products SET Stock = @Stock WHERE Id = @Id";
                        connection.Execute(sqlUpdateProduct, new { product.Stock, Id = product.Id });
                    }
                    else
                    {
                        // Sepetten ürünü sil
                        var sqlDelete = "DELETE FROM Sepet WHERE Id = @Id";
                        connection.Execute(sqlDelete, new { Id = id });

                        // Ürün stoğunu bir artır
                        product.Stock++;
                        var sqlUpdateProduct = "UPDATE products SET Stock = @Stock WHERE Id = @Id";
                        connection.Execute(sqlUpdateProduct, new { product.Stock, Id = product.Id });
                    }
                }
            }

            return RedirectToAction("SepetIndex");
        }



        private int GetSepetUrunSayisi()
        {
            using var connection = new SqlConnection(connectionString);
            var sepetUrunSayisi = connection.Query<int>("SELECT COUNT(*) FROM Sepet").FirstOrDefault();
            return sepetUrunSayisi;
        }

        public IActionResult Odeme(string? MessageCssClass, string? Message)
        {
            ViewData["Title"] = "Ödeme";
            using var connection = new SqlConnection(connectionString);
            var cartItems = connection.Query<Sepet>("SELECT * FROM Sepet").ToList();

            ViewBag.Message = Message;
            ViewBag.MessageCssClass = MessageCssClass;

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult Odeme(string OdemeYontemi)
        {

            using var connection = new SqlConnection(connectionString);
            var cartItems = connection.Query<Sepet>("SELECT * FROM Sepet").ToList();
            
            connection.Execute("DELETE FROM Sepet");

            return RedirectToAction("Index", new { MessageCssClass = "alert-success", Message = "Ödeme alınmıştır iyi günler dileriz." });
        }
    }
}
