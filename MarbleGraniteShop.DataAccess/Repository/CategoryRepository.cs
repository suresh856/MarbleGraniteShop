using MarbleGraniteShop.DataAccess.Data;
using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;
using System.Linq;

namespace MarbleGraniteShop.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            var objFromDb = _db.Categories.FirstOrDefault(s => s.Id == category.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = category.Name;

            }
        }
    }
}
