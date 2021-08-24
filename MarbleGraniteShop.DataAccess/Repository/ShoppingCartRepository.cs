using MarbleGraniteShop.DataAccess.Data;
using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart obj)
        {
            _db.Update(obj);
        }
    }
}
