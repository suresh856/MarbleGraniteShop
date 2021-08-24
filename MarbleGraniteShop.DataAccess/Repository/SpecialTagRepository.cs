using MarbleGraniteShop.DataAccess.Data;
using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;
using System.Linq;

namespace MarbleGraniteShop.DataAccess.Repository
{
    public class SpecialTagRepository : Repository<SpecialTag>, ISpecialTagRepository
    {
        private readonly ApplicationDbContext _db;

        public SpecialTagRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(SpecialTag SpecialTag)
        {
            var objFromDb = _db.SpecialTag.FirstOrDefault(s => s.Id == SpecialTag.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = SpecialTag.Name;

            }
        }
    }
}
