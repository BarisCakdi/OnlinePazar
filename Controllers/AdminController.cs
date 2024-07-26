using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlinePazar.Models;
using System.Net;
using OnlinePazar.Models;

namespace OnlinePazar.Controllers;

public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Admin Panel";
        using var connection = new SqlConnection(connectionString);
        var products = connection.Query<Product>("SELECT * FROM products").ToList();
        return View(products);
    }
    [HttpPost]
    public IActionResult Add(Product model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Message = "Ürün eklenirken bir hata oluştu";
            ViewBag.MessageCssClass = "alert-danger";
            return View("Message");
        }

        if (model.Image != null && model.Image.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            model.Image.CopyTo(fileStream);
            model.ImagePath = fileName;
        }

        model.CreatedDate = DateTime.Now;
        using var connection = new SqlConnection(connectionString);
        var sql = ("INSERT INTO products (Name, Price, Stock, ImagePath, CreatedDate) VALUES (@Name, @Price, @Stock, @ImagePath, @CreatedDate)");
        var data = new
        {
            model.Name,
            model.Price,
            model.Stock,
            model.ImagePath,
            CreatedDate = DateTime.Now,
        };
        var rowsAffected = connection.Execute(sql, data);
        ViewBag.Message = "Ürün eklendi.";
        ViewBag.MessageCssClass = "alert-success";
        return View("Message");
    }

    [HttpPost]
    public IActionResult Edit(Product model, string ExistingImagePath)
    {
        
        if (model.Image != null && model.Image.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            model.Image.CopyTo(fileStream);
            model.ImagePath = fileName;
        }
        else
        {
            // Mevcut resim ile devam ediyorum
            model.ImagePath = ExistingImagePath;
        }

        using var connection = new SqlConnection(connectionString);

        var sqlUpdate = "UPDATE products SET Name=@Name, Price=@Price, Stock=@Stock, ImagePath=@ImagePath WHERE Id = @Id";
        var param = new
        {
            model.Name,
            model.Price,
            model.Stock,
            model.ImagePath,
            model.Id
        };
        var affectedRows = connection.Execute(sqlUpdate, param);
        ViewBag.Message = "Ürün güncellendi.";
        ViewBag.MessageCssClass = "alert-success";
        return View("Message");

    }
    public IActionResult Delete(int id)
    {
        using var connection = new SqlConnection(connectionString);
        var sql = "DELETE from products WHERE Id = @Id";
        var rowAffected = connection.Execute(sql, new {Id = id});
        return RedirectToAction("index");

    }
}
