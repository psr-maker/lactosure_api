using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.PortableExecutable;

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

        public class Society
        {
            [Key]
            public int SID { get; set; }

            [Required]
            [MaxLength(20)]
            public string SocietyCode { get; set; }

            [Required]
            [MaxLength(100)]
            public string SName { get; set; }

            public bool Status { get; set; } = true;
            public ICollection<Machine> Machines { get; set; }
      = new List<Machine>();

        }
        public class MachineType
        {
            [Key]
            public int MTID { get; set; }

            [Required]
            [MaxLength(100)]
            public string MType { get; set; }

            public bool Status { get; set; } = true;

        }

        public class Machine
        {
            [Key]
            public int MID { get; set; }

            [Required]
            [MaxLength(20)]
            public string MachineCode { get; set; } 

            [Required]
            public int SID { get; set; }

            [Required]
            public int MTID { get; set; }

            public bool Status { get; set; } = true;

            [ForeignKey(nameof(SID))]
            public Society? Society { get; set; }

            [ForeignKey(nameof(MTID))]
            public MachineType? MachineType { get; set; }
        }
        public class BleDevice
        {
            public int Id { get; set; }

            public string BleName { get; set; } 

            public string MacAddress { get; set; } 

            public bool IsActive { get; set; } 

  
        }

        public class UserFace
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public int UserId { get; set; }

            [Required]
            public string FaceImage { get; set; } 

            [Required]
            public string FaceEmbedding { get; set; } 

            public bool Status { get; set; } = true;

            public DateTime EnrolledAt { get; set; } 
        }

        public class CorrMethodHistory
        {
            [Key]
            public int Id { get; set; }

            public int UId { get; set; }

            [Required]
            [MaxLength(50)]
            public string CorrMethod { get; set; } 

            [Required]
            [MaxLength(20)]
            public string Channel { get; set; } 

            public DateTime Date { get; set; }
        }


        //**********************************************
        public class AddMachineDto
        {
            public string MachineCode { get; set; } = string.Empty;
            public int SID { get; set; }
            public int MTID { get; set; }
            public bool Status { get; set; } = true;
        }
        public class UpdateSocietyDto
        {
            public string SName { get; set; } = string.Empty;
            public bool Status { get; set; }
        }
        public class UpdateMachineTypeDto
        {
            public string MType { get; set; } = string.Empty;
            public bool Status { get; set; }
        }
        public class UpdateMachineDto
        {
            public int SID { get; set; }
            public int MTID { get; set; }
            public bool Status { get; set; }
        }
        public class BleDeviceDto
        {
            public string BleName { get; set; } 

            public string MacAddress { get; set; } 

            public bool IsActive { get; set; }
        }
        public class FaceRegisterRequest
        {
            public int UserId { get; set; }

            public IFormFile Image { get; set; }
        }
        public class FaceStatusRequest
        {
            public bool Status { get; set; }
        }
    }
}
