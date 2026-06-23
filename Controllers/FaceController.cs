using Lactosure_api.DB_Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Lactosure_api.Models.lacto;

namespace Lactosure_api.Controllers
{
   
    [Route("api/face")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private readonly FaceService _faceService;
        private readonly ApplicationDbContext _db;

        public FaceController(FaceService faceService, ApplicationDbContext db)
        {
            _faceService = faceService;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] IFormFile file, [FromForm] int userId)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var embedding = await _faceService.GetEmbedding(ms.ToArray());

            var userFace = new UserFace
            {
                UserId = userId,
                FaceImage = Convert.ToBase64String(ms.ToArray()),
                FaceEmbedding = JsonSerializer.Serialize(embedding),
                EnrolledAt = DateTime.UtcNow,
                Status = true
            };

            _db.UserFace.Add(userFace);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Face registered" });
        }

        [Authorize]
        [HttpPost("face-verify")]
        public async Task<IActionResult> Verify([FromForm] IFormFile file)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var newEmbedding =
                    await _faceService.GetEmbedding(ms.ToArray());

                if (newEmbedding == null || newEmbedding.Count == 0)
                {
                    return BadRequest(new
                    {
                        match = false,
                        message = "No face detected"
                    });
                }

                var faces = await _db.UserFace
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                if (!faces.Any())
                {
                    return NotFound(new
                    {
                        match = false,
                        message = "No registered face found"
                    });
                }

                foreach (var face in faces)
                {
                    var storedEmbedding =
                        JsonSerializer.Deserialize<List<float>>(
                            face.FaceEmbedding
                        );

                    if (storedEmbedding == null)
                        continue;

                    if (storedEmbedding.Count != newEmbedding.Count)
                        continue;

                    var similarity =
                        CosineSimilarity(newEmbedding, storedEmbedding);

                    Console.WriteLine("================================");
                    Console.WriteLine($"UserId: {userId}");
                    Console.WriteLine($"Stored Count: {storedEmbedding.Count}");
                    Console.WriteLine($"New Count: {newEmbedding.Count}");
                    Console.WriteLine($"Similarity: {similarity}");
                    Console.WriteLine("================================");

                    if (similarity > 0.45)
                    {
                        return Ok(new
                        {
                            match = true,
                            score = similarity
                        });
                    }
                }

                return Ok(new
                {
                    match = false,
                    score = 0
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    match = false,
                    error = ex.Message
                });
            }
        }

        private double CosineSimilarity(List<float> a, List<float> b)
        {
            double dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

    }
}
