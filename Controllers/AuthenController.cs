using Lactosure_api.DB_Context;
using Lactosure_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Lactosure_api.Models.lacto;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;

namespace Lactosure_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public AuthenController(ApplicationDbContext context, IConfiguration configuration, JwtService jwtService)
        {
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Users user)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(x => x.Email == user.Email);

                if (existingUser != null)
                    return BadRequest(new { message = "Email already exists" });

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Status = false;

                _context.Users.Add(user);

                string otp = new Random().Next(100000, 999999).ToString();

                var otpData = new Otp
                {
                    Email = user.Email,
                    OTP = HashOtp(otp),
                    Expiry = DateTime.UtcNow.AddMinutes(5),
                    CreatedAt = DateTime.UtcNow,
                    ResendCount = 0
                };

                _context.Otp.Add(otpData);

                await _context.SaveChangesAsync();

                SendEmail(user.Email, "OTP Verification", $"Your OTP is {otp}. It expires in 5 minutes.");

                return Ok(new { message = "Registered successfully. OTP sent to email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

       
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            var existingOtp = _context.Otp.FirstOrDefault(x => x.Email == email);
            var existingUser = _context.Users.FirstOrDefault(x => x.Email == email);

            if (existingOtp == null || existingUser == null)
                return BadRequest(new { message = "User or OTP record not found" });

            // 🚨 4th attempt trigger (after 3 max reached)
            if (existingOtp.ResendCount >= 3)
            {
                _context.Otp.Remove(existingOtp);
                _context.Users.Remove(existingUser);

                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "Maximum OTP attempts exceeded. Account has been removed."
                });
            }

            // Generate new OTP
            string otp = new Random().Next(100000, 999999).ToString();

            existingOtp.OTP = HashOtp(otp);
            existingOtp.Expiry = DateTime.UtcNow.AddMinutes(5);
            existingOtp.ResendCount++;

            await _context.SaveChangesAsync();

            SendEmail(email, "Resend OTP", $"Your OTP is {otp}");

            return Ok(new
            {
                message = "OTP sent successfully",
                resendCount = existingOtp.ResendCount,
                remainingAttempts = 3 - existingOtp.ResendCount
            });
        }

      
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] Otp request)
        {
            var otpData = _context.Otp.FirstOrDefault(x => x.Email == request.Email);
            var user = _context.Users.FirstOrDefault(x => x.Email == request.Email);

            if (otpData == null || user == null)
                return BadRequest("Invalid request");

            if (otpData.Expiry < DateTime.UtcNow)
                return BadRequest("OTP expired");

            if (otpData.OTP != HashOtp(request.OTP))
                return BadRequest("Invalid OTP");

            // ✅ delete OTP after success
            _context.Otp.Remove(otpData);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "OTP verified. Waiting for admin approval."
            });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement data)
        {
            try
            {
                if (!data.TryGetProperty("email", out var emailProp) ||
                    !data.TryGetProperty("password", out var passProp))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid request body"
                    });
                }

                var email = emailProp.GetString()?.Trim();
                var password = passProp.GetString();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email or password is empty"
                    });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == email);

                if (user == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Email not found"
                    });
                }

                if (!user.Status)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "User not approved"
                    });
                }

                // SAFE BCrypt check (prevents "Invalid salt version" crash)
                bool isPasswordValid = false;

                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                }
                catch
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Password format error in database (invalid hash)"
                    });
                }

                if (!isPasswordValid)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Incorrect password"
                    });
                }

                var token = _jwtService.GenerateToken(user.UId,user.Email);

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    token,
                    user = new
                    {
                        user.UId,
                        user.Name,
                        user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UId,
                    u.Name,
                    u.Email,
                    u.Status
                })
                .ToListAsync();

            return Ok(users);
        }


        [HttpPut("approve-user/{id}")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            user.Status = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "User approved successfully"
            });
        }

      
        [HttpDelete("reject-user/{id}")]
        public async Task<IActionResult> RejectUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            // Remove OTP also
            var otp = await _context.Otp
                .FirstOrDefaultAsync(x => x.Email == user.Email);

            if (otp != null)
            {
                _context.Otp.Remove(otp);
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "User rejected and removed"
            });
        }

      
        [HttpPost("forgot-password-send-otp")]
        public async Task<IActionResult> ForgotPasswordSendOtp([FromBody] string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email not found"
                });
            }

            var existingOtp = await _context.Otp
                .FirstOrDefaultAsync(x => x.Email == email);

            string otp = new Random().Next(100000, 999999).ToString();

            if (existingOtp == null)
            {
                existingOtp = new Otp
                {
                    Email = email,
                    OTP = HashOtp(otp),
                    Expiry = DateTime.UtcNow.AddMinutes(5),
                    CreatedAt = DateTime.UtcNow,
                    ResendCount = 0
                };

                _context.Otp.Add(existingOtp);
            }
            else
            {
                existingOtp.OTP = HashOtp(otp);
                existingOtp.Expiry = DateTime.UtcNow.AddMinutes(5);
            }

            await _context.SaveChangesAsync();

            SendEmail(email, "Forgot Password OTP", $"Your OTP is {otp}");

            return Ok(new
            {
                success = true,
                message = "OTP sent successfully"
            });
        }

   
        [HttpPost("forgot-password-verify-otp")]
        public async Task<IActionResult> ForgotPasswordVerifyOtp([FromBody] Otp request)
        {
            var otpData = await _context.Otp
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (otpData == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP not found"
                });
            }

            if (otpData.Expiry < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP expired"
                });
            }

            if (otpData.OTP != HashOtp(request.OTP))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid OTP"
                });
            }

            return Ok(new
            {
                success = true,
                message = "OTP verified"
            });
        }

       
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] JsonElement data)
        {
            string email = data.GetProperty("email").GetString();
            string password = data.GetProperty("password").GetString();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            // remove otp after success
            var otp = await _context.Otp
                .FirstOrDefaultAsync(x => x.Email == email);

            if (otp != null)
            {
                _context.Otp.Remove(otp);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Password updated successfully"
            });
        }

        // ========================= HASH OTP =========================
        private string HashOtp(string otp)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(bytes);
        }

        // ========================= EMAIL METHOD =========================
        private void SendEmail(string to, string subject, string body)
        {
            var senderEmail = _configuration["EmailSettings:Email"];
            var senderPassword = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            using MailMessage mail = new MailMessage();
            mail.From = new MailAddress(senderEmail!);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;

            using SmtpClient smtp = new SmtpClient(host, port);
            smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtp.EnableSsl = true;

            smtp.Send(mail);
        }


    }
}