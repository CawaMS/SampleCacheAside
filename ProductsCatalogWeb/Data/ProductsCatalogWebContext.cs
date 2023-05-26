using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductsCatalogWeb.Models;

namespace ProductsCatalogWeb.Data
{
    public class ProductsCatalogWebContext : DbContext
    {
        public ProductsCatalogWebContext (DbContextOptions<ProductsCatalogWebContext> options)
            : base(options)
        {
        }

        public DbSet<ProductsCatalogWeb.Models.Product> Product { get; set; } = default!;
    }
}
