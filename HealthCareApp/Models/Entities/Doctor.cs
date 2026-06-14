using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HealthCareApp.Models.Entities
{
    public class Doctor
    {
        [Key]
        public int Doctor_ID { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int Department_ID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        public string Field { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(30)]
        public string Email { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("Department_ID")]
        [ValidateNever]
        public Department Department { get; set; } = null!;

        [ValidateNever]
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Computed full name
        [NotMapped]
        public string FullName => $"Dr. {FirstName} {LastName}";
    }
}