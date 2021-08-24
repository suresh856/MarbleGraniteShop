using MarbleGraniteShop.DataAccess.Data;
using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
