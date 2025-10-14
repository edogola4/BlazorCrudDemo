using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorCrudDemo.Data.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Product> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
