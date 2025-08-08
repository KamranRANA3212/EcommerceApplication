using Ecommerce_Application.Data;
using Ecommerce_Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ecommerce_Application.Controllers
{
    [RoutePrefix("products")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository = new ProductRepository();
        private readonly ICategoryRepository _categoryRepository = new CategoryRepository();

        // GET: /products
        [Route("")]
        public async Task<ActionResult> Index(string search, int? categoryId)
        {
            var products = await _productRepository.GetAllAsync(search, categoryId);
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.CategoryLookup = categories.ToDictionary(c => c.Id, c => c.Name);
            ViewBag.Search = search;
            return View(products);
        }

        // GET: /products/add
        [Route("add")]
        public async Task<ActionResult> Add()
        {
            await LoadCategoriesAsync();
            return View();
        }

        // POST: /products/add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add")]
        public async Task<ActionResult> Add(Product model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                await HandleFileUploadAsync(model, imageFile);
                await _productRepository.AddAsync(model);
                return RedirectToAction("Index");
            }
            await LoadCategoriesAsync();
            return View(model);
        }

        // GET: /products/edit/{id}
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            await LoadCategoriesAsync();
            return View(product);
        }

        // POST: /products/edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int id, Product model, HttpPostedFileBase imageFile)
        {
            if (id != model.Id)
            {
                return new HttpStatusCodeResult(400);
            }
            if (ModelState.IsValid)
            {
                await HandleFileUploadAsync(model, imageFile);
                await _productRepository.UpdateAsync(model);
                return RedirectToAction("Index");
            }
            await LoadCategoriesAsync();
            return View(model);
        }

        // POST: /products/delete/{id}
        [HttpPost]
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }

        private async Task HandleFileUploadAsync(Product model, HttpPostedFileBase imageFile)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                if (!allowed.Contains(extension))
                {
                    ModelState.AddModelError("Photo", "Only .jpg, .jpeg, .png formats are allowed.");
                    return;
                }
                if (imageFile.ContentLength > 25 * 1024)
                {
                    ModelState.AddModelError("Photo", "Image size must be less than 25 KB.");
                    return;
                }

                var uploadsPath = Server.MapPath("~/uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploadsPath, fileName);
                imageFile.SaveAs(filePath);
                model.Photo = "uploads/" + fileName;
            }
        }
    }
}