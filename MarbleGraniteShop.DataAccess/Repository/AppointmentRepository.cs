using MarbleGraniteShop.DataAccess.Data;
using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly ApplicationDbContext _db;

        public AppointmentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Appointment appointment)
        {
            _db.Update(appointment);
        }
    }
}
