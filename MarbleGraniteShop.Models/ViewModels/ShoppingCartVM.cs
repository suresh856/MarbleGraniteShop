﻿using System.Collections.Generic;

namespace MarbleGraniteShop.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public Appointment Appointment { get; set; }
        public List<Product> Products { get; set; }
    }
}
