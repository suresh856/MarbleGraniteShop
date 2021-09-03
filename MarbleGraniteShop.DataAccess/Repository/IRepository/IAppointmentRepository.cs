using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository.IRepository
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        void Update(Appointment appointment);
    }
    public interface IProductsSelectedForAppointmentRepository : IRepository<ProductsSelectedForAppointment>
    {
        void Update(ProductsSelectedForAppointment productsSelectedForAppointment);
    }
}
