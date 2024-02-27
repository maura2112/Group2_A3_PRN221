using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group2_A3.Models;
using Microsoft.AspNetCore.SignalR;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;


namespace Group2_A3.Controllers
{
    public class ProductsController : Controller
    {
        private readonly eStore1Context _context;
        private readonly IHubContext<SignalRServer> _hubContext;
        public ProductsController(eStore1Context context, IHubContext<SignalRServer> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var eStore1Context = _context.Products.Include(p => p.Category);
            return View(await eStore1Context.ToListAsync());
        }
        public IActionResult GetProducts(float? unitPrice, string? name)
        {
            List<Product> products = new List<Product>();

            if (unitPrice != null || name != null)
            {
                products = _context.Products
    .Select(p => new Product
    {
        ProductId = p.ProductId,
        CategoryId = p.CategoryId,
        ProductName = p.ProductName,
        Weight = p.Weight,
        UnitPrice = p.UnitPrice,
        UnitInStock = p.UnitInStock,
        Category = new Category
        {
            CategoryId = p.Category.CategoryId,
            CategoryName = p.Category.CategoryName
        }
    })
    .Where(x =>
        (unitPrice == null || x.UnitPrice == unitPrice) &&
        (name == null || x.ProductName.Contains(name)))
    .ToList();
            }
            else
            {
                products = _context.Products.
                Select(p => new Product
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    ProductName = p.ProductName,
                    Weight = p.Weight,
                    UnitPrice = p.UnitPrice,
                    UnitInStock = p.UnitInStock,
                    Category = new Category
                    {
                        CategoryId = p.Category.CategoryId,
                        CategoryName = p.Category.CategoryName
                    }
                }).ToList();

            }


            return Ok(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,ProductName,Weight,UnitPrice,UnitInStock")] Product product)
        {

            _context.Add(product);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("GetProducts");
            return RedirectToAction(nameof(Index));

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CategoryId,ProductName,Weight,UnitPrice,UnitInStock")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("GetProducts");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
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
            if (_context.Products == null)
            {
                return Problem("Entity set 'eStore1Context.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("GetProducts");
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
