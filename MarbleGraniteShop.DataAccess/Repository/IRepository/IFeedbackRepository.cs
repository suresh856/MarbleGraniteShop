using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository.IRepository
{
    public interface IFeedbackRepository : IRepository<FeedBack>
    {
        void Update(FeedBack feedBack);
    }
}
