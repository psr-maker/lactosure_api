using System.ComponentModel.DataAnnotations;

namespace Lactosure_api.Models
{
    public class lacto
    {
        public class Users
        {
            [Key]
            public int UId { get; set; }

            [Required]
            [MaxLength(100)]
            public string Name { get; set; } 

            [Required]
            [MaxLength(150)]
            public string Email { get; set; } 

            [Required]
            public string Password { get; set; }

            public bool Status { get; set; } 
        }

        public class Otp
        {
            [Key]
            public int Id { get; set; }

            [Required]
            [MaxLength(150)]
            public string Email { get; set; } 

            [Required]
            [MaxLength(255)]
            public string OTP { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime Expiry { get; set; }
            public int ResendCount { get; set; } 
        }
    }
}
