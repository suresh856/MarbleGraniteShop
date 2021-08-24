using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
    }
}
