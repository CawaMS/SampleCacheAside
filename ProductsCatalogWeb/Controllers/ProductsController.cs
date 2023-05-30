using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using ProductsCatalogWeb.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using ProductsCatalogWeb.Models;

namespace ProductsCatalogWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductsCatalogWebContext _context;
        private readonly IDistributedCache _cache;

        public ProductsController(ProductsCatalogWebContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            //Check if products exist in cache. 
            byte[]? productList = await _cache.GetAsync("AllProducts");

            //If there are cached entries, return the values directly
            if (!productList.IsNullOrEmpty())
            {
                List<Product>? products = await JsonSerializer.DeserializeAsync<List<Product>>(new MemoryStream(productList));
                return View(products);
            }
            //else, fetch the products from database, save in cache for next read operation.D
            else
            {
                List<Product>? products = await _context.Product.ToListAsync();
                using (MemoryStream? memoryStream = new MemoryStream())
                {
                    await JsonSerializer.SerializeAsync(memoryStream, products);
                    await _cache.SetAsync("AllProducts", memoryStream.ToArray());
                }
                return View(products);

            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            //try to get product from cache
            byte[]? productFromCache = await _cache.GetAsync(id.ToString());
            //if product is already in cache, return to view.
            if (!productFromCache.IsNullOrEmpty())
            {
                Product? product = await JsonSerializer.DeserializeAsync<Product>(new MemoryStream(productFromCache));
                return View(product);
            }
            //else, read product from database and save in cache for next read operation. Review product to view. 
            else
            {
                var product = await _context.Product.FirstOrDefaultAsync(m => m.Id == id);
                if (product == null)
                {
                    return NotFound();
                }
                else
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await JsonSerializer.SerializeAsync(memoryStream, product);
                        await _cache.SetAsync(id.ToString(), memoryStream.ToArray());
                    }
                    return View(product);

                }
            }

        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Brand,Image,category")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                _cache.Remove("AllProducts");
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Brand,Image,category")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    _cache.Remove("AllProducts");
                    _cache.Remove(id.ToString());
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Product == null)
            {
                return Problem("Entity set 'ProductsCatalogWebContext.Product'  is null.");
            }
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }
            _cache.Remove("AllProducts");
            _cache.Remove(id.ToString());
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
