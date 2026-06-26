using Lactosure_api.DB_Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using static Lactosure_api.Models.lacto;

namespace Lactosure_api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("add-society")]
        public async Task<IActionResult> AddSociety([FromBody] Society society)
        {
            var exists = await _context.Society
                .AnyAsync(x => x.SocietyCode == society.SocietyCode);

            if (exists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Society Code already exists"
                });
            }

            _context.Society.Add(society);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Society added successfully"
            });
        }
        [HttpPost("add-machine-type")]
        public async Task<IActionResult> AddMachineType([FromBody] MachineType machineType)
        {
            var exists = await _context.MachineType
                .AnyAsync(x => x.MType == machineType.MType);

            if (exists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Machine Type already exists"
                });
            }

            _context.MachineType.Add(machineType);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Machine type added successfully"
            });
        }
        [HttpPost("add-machine")]
        public async Task<IActionResult> AddMachine([FromBody] AddMachineDto dto)
        {
            var machineExists = await _context.Machine
                .AnyAsync(x => x.MachineCode == dto.MachineCode);

            if (machineExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Machine Code already exists"
                });
            }

            var machine = new Machine
            {
                MachineCode = dto.MachineCode,
                SID = dto.SID,
                MTID = dto.MTID,
                Status = dto.Status
            };

            _context.Machine.Add(machine);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Machine added successfully"
            });
        }
        [HttpGet("societies")]
        public async Task<IActionResult> GetSocieties()
        {
            var societies = await _context.Society
                .OrderBy(x => x.SName)
                .ToListAsync();

            return Ok(societies);
        }
        [HttpGet("machine-types")]
        public async Task<IActionResult> GetMachineTypes()
        {
            var machineTypes = await _context.MachineType
                .OrderBy(x => x.MType)
                .ToListAsync();

            return Ok(machineTypes);
        }
        [HttpGet("machines")]
        public async Task<IActionResult> GetMachines()
        {
            var machines = await _context.Machine
                .Include(x => x.Society)
                .Include(x => x.MachineType)
                .Select(x => new
                {
                    x.MID,
                    x.MachineCode,
                    x.SID,
                    SocietyName = x.Society!.SName,
                    x.MTID,
                    MachineType = x.MachineType!.MType,
                    x.Status
                })
                .ToListAsync();

            return Ok(machines);
        }

        [HttpPut("society/{id}")]
        public async Task<IActionResult> UpdateSociety(int id, [FromBody] UpdateSocietyDto dto)
        {
            var society = await _context.Society.FindAsync(id);

            if (society == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Society not found"
                });
            }

            society.SName = dto.SName;
            society.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Society updated successfully"
            });
        }

        [HttpDelete("society/{id}")]
        public async Task<IActionResult> DeleteSociety(int id)
        {
            var society = await _context.Society.FindAsync(id);

            if (society == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Society not found"
                });
            }

            _context.Society.Remove(society);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Society deleted successfully"
            });
        }
        [HttpPut("machine-type/{id}")]
        public async Task<IActionResult> UpdateMachineType(int id, [FromBody] UpdateMachineTypeDto dto)
        {
            var machineType = await _context.MachineType.FindAsync(id);

            if (machineType == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Machine Type not found"
                });
            }

            machineType.MType = dto.MType;
            machineType.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Machine Type updated successfully"
            });
        }

        [HttpDelete("machine-type/{id}")]
        public async Task<IActionResult> DeleteMachineType(int id)
        {
            var machineType = await _context.MachineType.FindAsync(id);

            if (machineType == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Machine Type not found"
                });
            }

            _context.MachineType.Remove(machineType);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Machine Type deleted successfully"
            });
        }

        [HttpPut("machine/{id}")]
        public async Task<IActionResult> UpdateMachine(int id, [FromBody] UpdateMachineDto dto)
        {
            var machine = await _context.Machine.FindAsync(id);

            if (machine == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Machine not found"
                });
            }

            machine.SID = dto.SID;
            machine.MTID = dto.MTID;
            machine.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Machine updated successfully"
            });
        }

        [HttpDelete("machine/{id}")]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            var machine = await _context.Machine.FindAsync(id);

            if (machine == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Machine not found"
                });
            }

            _context.Machine.Remove(machine);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Machine deleted successfully"
            });
        }

        [HttpPost("add-devices")]
        public async Task<IActionResult> Add([FromBody] BleDeviceDto model)
        {
            var device = new BleDevice
            {
                BleName = model.BleName,
                MacAddress = model.MacAddress.ToUpper(),
                IsActive = model.IsActive
            };

            _context.BleDevice.Add(device);
            await _context.SaveChangesAsync();

            return Ok(device);
        }
        [HttpGet("get-alldevices")]
        public async Task<IActionResult> GetAll()
        {
            var devices = await _context.BleDevice.ToListAsync();
            return Ok(devices);
        }
        [HttpPut("update-device/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BleDeviceDto model)
        {
            var device = await _context.BleDevice.FindAsync(id);

            if (device == null)
                return NotFound("Device not found.");

            device.BleName = model.BleName;
            device.MacAddress = model.MacAddress.ToUpper();
            device.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return Ok(device);
        }
        [HttpDelete("delete-devices/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var device = await _context.BleDevice.FindAsync(id);

            if (device == null)
                return NotFound("Device not found.");

            _context.BleDevice.Remove(device);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Deleted Successfully"
            });
        }

       ////
    }
}
