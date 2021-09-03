using MarbleGraniteShop.DataAccess.Data;
using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository
{
    public class ProductsSelectedForAppointmentRepository : Repository<ProductsSelectedForAppointment>, IProductsSelectedForAppointmentRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductsSelectedForAppointmentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductsSelectedForAppointment appointment)
        {
            _db.Update(appointment);
        }
    }
}
