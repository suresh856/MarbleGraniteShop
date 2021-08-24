using MarbleGraniteShop.Models;
using System.Text;

namespace MarbleGraniteShop.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company company);
    }
}
