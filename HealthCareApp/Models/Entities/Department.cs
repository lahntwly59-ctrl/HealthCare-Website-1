using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models.Entities
{
    public class Department
    {
        [Key]
        public int Department_ID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Department Name")]
        public string Department_Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Location { get; set; } = string.Empty;

       
        public ICollection<Doctor> Doctors { get; set; } 
    }
}