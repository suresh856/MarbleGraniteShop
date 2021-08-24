using MarbleGraniteShop.Models;

namespace MarbleGraniteShop.DataAccess.Repository.IRepository
{
    public interface ISpecialTagRepository : IRepository<SpecialTag>
    {
        void Update(SpecialTag SpecialTag);
    }
}
