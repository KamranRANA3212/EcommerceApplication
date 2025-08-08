using EcommerceApplication.Models.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using EcommerceApplication.Services;
using Ecommerce_Application.Data;

namespace EcommerceApplication.Controllers
{
    public class ProductsController : Controller
    {
        // GET: /products
        public async Task<ActionResult> Index(string searchTerm, int? categoryFilter, int page = 1)
        {
            try
            {
                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    var viewModel = await service.GetProductsAsync(searchTerm, categoryFilter, page);
                    return View(viewModel);
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading products. Please try again.";
                return View(new ProductListViewModel());
            }
        }

        // GET: /products/add (Popup modal)
        public async Task<ActionResult> Add()
        {
            try
            {
                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    var categories = await service.GetAllCategoriesAsync();
                    var viewModel = new ProductViewModel
                    {
                        Categories = categories
                    };
                    return PartialView("_AddProductModal", viewModel);
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while loading the form." }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /products/add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Add(ProductViewModel productViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    using (var connection = new DapperContext().CreateConnection())
                    {
                        var service = new ProductService(connection);
                        var categories = await service.GetAllCategoriesAsync();
                        productViewModel.Categories = categories;
                        return Json(new { success = false, html = RenderPartialViewToString("_AddProductModal", productViewModel) });
                    }
                }

                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    if (!await service.IsSkuUniqueAsync(productViewModel.SKU))
                    {
                        ModelState.AddModelError("SKU", "SKU must be unique.");
                        var categories = await service.GetAllCategoriesAsync();
                        productViewModel.Categories = categories;
                        return Json(new { success = false, html = RenderPartialViewToString("_AddProductModal", productViewModel) });
                    }

                    var success = await service.CreateProductAsync(productViewModel);
                    if (success)
                    {
                        return Json(new { success = true, message = "Product created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create product. Please try again." });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while creating the product." });
            }
        }

        // GET: /products/edit/:id
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    var product = await service.GetProductByIdAsync(id);
                    if (product == null)
                    {
                        TempData["ErrorMessage"] = "Product not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    return View(product);
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /products/edit/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ProductViewModel productViewModel)
        {
            try
            {
                if (id != productViewModel.Id)
                {
                    TempData["ErrorMessage"] = "Invalid product ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    using (var connection = new DapperContext().CreateConnection())
                    {
                        var service = new ProductService(connection);
                        var categories = await service.GetAllCategoriesAsync();
                        productViewModel.Categories = categories;
                        return View(productViewModel);
                    }
                }

                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    if (!await service.IsSkuUniqueAsync(productViewModel.SKU, productViewModel.Id))
                    {
                        ModelState.AddModelError("SKU", "SKU must be unique.");
                        var categories = await service.GetAllCategoriesAsync();
                        productViewModel.Categories = categories;
                        return View(productViewModel);
                    }

                    var success = await service.UpdateProductAsync(productViewModel);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Product updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update product. Please try again.";
                        var categories = await service.GetAllCategoriesAsync();
                        productViewModel.Categories = categories;
                        return View(productViewModel);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    var categories = await service.GetAllCategoriesAsync();
                    productViewModel.Categories = categories;
                    return View(productViewModel);
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the product.";
                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    var categories = await service.GetAllCategoriesAsync();
                    productViewModel.Categories = categories;
                    return View(productViewModel);
                }
            }
        }

        // POST: /products/delete/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    var success = await service.DeleteProductAsync(id);
                    if (success)
                    {
                        return Json(new { success = true, message = "Product deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to delete product. Please try again." });
                    }
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while deleting the product." });
            }
        }

        // GET: /products/check-sku
        [HttpGet]
        public async Task<JsonResult> CheckSku(string sku, int excludeId = 0)
        {
            try
            {
                using (var connection = new DapperContext().CreateConnection())
                {
                    var service = new ProductService(connection);
                    var isUnique = await service.IsSkuUniqueAsync(sku, excludeId);
                    return Json(new { isUnique }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { isUnique = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // Utility method to render partial view to string for AJAX
        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
