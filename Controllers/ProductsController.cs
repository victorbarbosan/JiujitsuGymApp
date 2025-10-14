using Microsoft.AspNetCore.Mvc;
using JiujitsuGymApp.Models;

namespace JiujitsuGymApp.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            // Sample list of products
            var products = new List<Product>
            {
                new Product {Id =1, Name = "Laptop", Price = 1200.00m},
                new Product {Id = 2, Name = "Smartphone", Price = 800.00m},
                new Product {Id =3, Name = "Headphones", Price = 150.00m}
            };
            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = 
            return View(product);
        }
    }
}
