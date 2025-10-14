using Microsoft.AspNetCore.Mvc;
using JiujitsuGymApp.Models;

namespace JiujitsuGymApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly List<Product> _products = new List<Product>
        {
               new Product {Id =1, Name = "Laptop", Price = 1200.00m},
                new Product {Id = 2, Name = "Smartphone", Price = 800.00m},
                new Product {Id =3, Name = "Headphones", Price = 150.00m},
                new Product { Id = 4, Name = "Keyboard", Price = 100.00m}
        };
            
        public IActionResult Index()
        {
            return View(_products);
        }
            

        public IActionResult Details(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

                return View(product);
        }
    }
}
