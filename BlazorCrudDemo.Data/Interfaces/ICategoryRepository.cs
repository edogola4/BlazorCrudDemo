using BlazorCrudDemo.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorCrudDemo.Data.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> Exists(int id);
    }
}
