using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
	public class ProductsController : Controller
	{
		private readonly ProductService _productService;

		public ProductsController(ProductService productService)
		{
			_productService = productService;
		}

		// GET : Products
		public async Task<IActionResult> Index()
		{
			var products = await _productService.GetProductsAsync();
			return View(products);
		}

		// GET : Products/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(id.Value);

			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		// GET : Products/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST : Products
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateProductDto dto)
		{
			if (ModelState.IsValid)
			{
				await _productService.CreateProductAsync(dto);
				return RedirectToAction(nameof(Index));
			}
			return View(dto);
		}

		// GET : Products/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(id.Value);
			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		// POST : Products/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, UpdateProductDto dto)
		{
			if (ModelState.IsValid)
			{
				var updated = await _productService.UpdateProductAsync(id, dto);
				if (!updated)
				{
					return NotFound();
				}
				return RedirectToAction(nameof(Index));
			}
			return View(dto);
		}
	}
}
