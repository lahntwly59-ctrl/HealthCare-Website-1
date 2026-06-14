using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareApp.Models.Entities
{
    public class Appointment
    {
        [Key]
        public int Appointment_ID { get; set; }

        [Required]
        public int Patient_ID { get; set; }

        [Required]
        public int Doctor_ID { get; set; }

        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        public DateTime Appointment_Date { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Scheduled";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- CHANGE THESE TWO LINES (Add the '?' ) ---
        [ForeignKey("Patient_ID")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("Doctor_ID")]
        public virtual Doctor? Doctor { get; set; }
    }
}
