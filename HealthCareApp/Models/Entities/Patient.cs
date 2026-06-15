using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareApp.Models.Entities
{
    public class Patient
    {
        [Key]
        public int Patient_ID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? Date_Of_Birth { get; set; }

        [Required]
        [StringLength(1)]
        public string Gender { get; set; } = "M";

        [Required]
        [StringLength(10)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(100)]
        public string? Address { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(30)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

  
        public ICollection<Appointment> Appointments { get; set; }

    
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}