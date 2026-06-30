using Lactosure_api.DB_Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Lactosure_api.Models.lacto;

namespace Lactosure_api.Controllers
{
    [Route("api/Dashboard")]
    [ApiController]
    public class dashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public dashboardController(ApplicationDbContext context)
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

        [HttpPost("CorrMethodSave")]
        public async Task<IActionResult> Save([FromBody] CorrMethodHistory history)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            history.Date = DateTime.UtcNow;

            _context.CorrMethodHistory.Add(history);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Correction method saved successfully.",
                data = history
            });
        }
        [HttpGet("GetCorrMethodHistory/{uid}")]
        public async Task<IActionResult> GetCorrMethodHistory(int uid)
        {
            var history = await _context.CorrMethodHistory
                .Where(x => x.UId == uid)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return Ok(history);
        }
    }

}
