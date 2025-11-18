using DocsGenerator.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocsGenerator.Controllers
{
    [ApiController]
    [Route("docs")]
    public class DocsController : Controller
    {
        private readonly AppDbContext _db;

        public DocsController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var latest = await _db.ProjectVersions
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (latest == null)
                return NotFound("No documentation versions found");

            var url = $"http://localhost:8080{latest.DocsPath}/html/index.html";

            return Redirect(url);
        }

        // --------------------------------------------------------------
        // GET /docs/{id} → 302 Redirect
        // --------------------------------------------------------------
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var version = await _db.ProjectVersions.FindAsync(id);

            if (version == null)
                return NotFound("Version not found");

            var url = $"{version.DocsPath}/html/index.html";

            return Redirect(url);
        }
    }
}
