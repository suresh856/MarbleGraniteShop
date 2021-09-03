using System;

namespace MarbleGraniteShop.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Category { get; }
        ISpecialTagRepository SpecialTag { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IApplicationUserRepository ApplicationUser { get; }
        ISP_Call SP_Call { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IImageRepository Images { get; }
        IFeedbackRepository FeedBack { get; }
        IAppointmentRepository Appointment { get; }
        IProductsSelectedForAppointmentRepository ProductsSelectedForAppointment { get; }

        void Save();
    }
}
