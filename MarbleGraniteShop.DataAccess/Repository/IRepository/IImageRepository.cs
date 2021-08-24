using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository.IRepository
{
    public interface IImageRepository : IRepository<Image>
    {
        void Update(Image image);
    }
}
