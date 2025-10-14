using BlazorCrudDemo.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorCrudDemo.Data.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> Exists(int id);
    }
}
