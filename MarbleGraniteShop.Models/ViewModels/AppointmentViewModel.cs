using System.Collections.Generic;

namespace MarbleGraniteShop.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public List<Appointment> Appointments { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
