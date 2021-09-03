using System.ComponentModel.DataAnnotations.Schema;

namespace MarbleGraniteShop.Models
{
    public class ProductsSelectedForAppointment
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        public int ProductId { get; set; }

    }
}
