using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorCrudDemo.Data.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Category> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task DeleteAsync(int id)
        {
            var category = await _dbSet
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                _context.Products.RemoveRange(category.Products);
                _dbSet.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
