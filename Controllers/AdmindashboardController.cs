using Lactosure_api.DB_Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lactosure_api.Controllers
{
    [Route("api/AdmDashboard")]
    [ApiController]
    public class AdmindashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdmindashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            return Ok(new
            {
                totalUsers = await _context.Users.CountAsync(),

                totalSocieties = await _context.Society.CountAsync(),
                activeSocieties = await _context.Society.CountAsync(x => x.Status),
                inactiveSocieties = await _context.Society.CountAsync(x => !x.Status),

                totalMachines = await _context.Machine.CountAsync(),
                activeMachines = await _context.Machine.CountAsync(x => x.Status),
                inactiveMachines = await _context.Machine.CountAsync(x => !x.Status),

                totalMachineTypes = await _context.MachineType.CountAsync()
            });
        }
    }

}
