using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarbleGraniteShop.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Display(Name = "Company Name")]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public DateTime AppointmentDate { get; set; }

        [NotMapped]
        public DateTime AppointmentTime { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerEmail { get; set; }
        public bool isConfirmed { get; set; }

    }
}
