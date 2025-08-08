using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };
    private const long MaxFileSizeBytes = 25 * 1024; // 25 KB

    public ProductsController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment env)
    {
        _productService = productService;
        _categoryService = categoryService;
        _env = env;
    }

    [HttpGet("/products")]
    public async Task<IActionResult> Index([FromQuery] ProductIndexFilter filter)
    {
        var data = await _productService.SearchAsync(filter.Search, filter.CategoryId);
        var items = data.Select(d => new ProductListItemViewModel
        {
            Id = d.product.Id,
            ProductName = d.product.ProductName,
            Sku = d.product.Sku,
            Price = d.product.Price,
            CategoryName = d.categoryName,
            Status = d.product.Status.ToString(),
            Photo = d.product.Photo
        }).ToList();

        ViewBag.Categories = await _categoryService.GetAllAsync();
        ViewBag.Filter = filter;
        return View(items);
    }

    [HttpPost("/products/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ProductFormViewModel model)
    {
        await ValidateAndProcessPhotoAsync(model);
        if (!ModelState.IsValid)
        {
            return BadRequest(new { ok = false, errors = ModelState.ToDictionary(k => k.Key, v => v.Value!.Errors.Select(e => e.ErrorMessage)) });
        }

        var product = new Product
        {
            ProductName = model.ProductName.Trim(),
            Sku = model.Sku.Trim(),
            Price = model.Price,
            CategoryId = model.CategoryId,
            Status = model.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? ProductStatus.Active : ProductStatus.Inactive,
            Photo = model.ExistingPhotoPath
        };

        var result = await _productService.CreateAsync(product);
        if (!result.ok)
        {
            ModelState.AddModelError(string.Empty, result.error ?? "Validation failed");
            return BadRequest(new { ok = false, errors = ModelState.ToDictionary(k => k.Key, v => v.Value!.Errors.Select(e => e.ErrorMessage)) });
        }

        return Ok(new { ok = true });
    }

    [HttpGet("/products/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetAsync(id);
        if (product == null) return NotFound();
        var vm = new ProductFormViewModel
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Sku = product.Sku,
            Price = product.Price,
            CategoryId = product.CategoryId,
            Status = product.Status.ToString(),
            ExistingPhotoPath = product.Photo
        };
        ViewBag.Categories = await _categoryService.GetAllAsync();
        return View(vm);
    }

    [HttpPost("/products/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        await ValidateAndProcessPhotoAsync(model);
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(model);
        }

        var product = new Product
        {
            Id = id,
            ProductName = model.ProductName.Trim(),
            Sku = model.Sku.Trim(),
            Price = model.Price,
            CategoryId = model.CategoryId,
            Status = model.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? ProductStatus.Active : ProductStatus.Inactive,
            Photo = model.ExistingPhotoPath
        };

        var result = await _productService.UpdateAsync(product);
        if (!result.ok)
        {
            ModelState.AddModelError(string.Empty, result.error ?? "Validation failed");
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(model);
        }

        TempData["Success"] = "Product updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/products/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        TempData["Success"] = "Product deleted";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateAndProcessPhotoAsync(ProductFormViewModel model)
    {
        if (model.PhotoFile == null || model.PhotoFile.Length == 0) return;
        var ext = Path.GetExtension(model.PhotoFile.FileName);
        if (!AllowedExtensions.Contains(ext))
        {
            ModelState.AddModelError(nameof(model.PhotoFile), "Only .jpg, .jpeg, .png are allowed.");
            return;
        }
        if (model.PhotoFile.Length > MaxFileSizeBytes)
        {
            ModelState.AddModelError(nameof(model.PhotoFile), "Image must be less than 25 KB.");
            return;
        }
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);
        using (var stream = System.IO.File.Create(filePath))
        {
            await model.PhotoFile.CopyToAsync(stream);
        }
        model.ExistingPhotoPath = $"/uploads/{fileName}";
    }
}