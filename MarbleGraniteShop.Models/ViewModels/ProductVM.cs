using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MarbleGraniteShop.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> SpecialTagList { get; set; }
        public IEnumerable<SelectListItem> CompanyList { get; set; }
    }
}
