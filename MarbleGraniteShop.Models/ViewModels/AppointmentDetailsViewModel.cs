using System.Collections.Generic;

namespace MarbleGraniteShop.Models.ViewModels
{
    public class AppointmentDetailsViewModel
    {

        public Appointment Appointment { get; set; }
        public List<Company> Companies { get; set; }
        public List<Product> Products { get; set; }

    }
}
