using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocsGenerator.Data;
using DocsGenerator.Models;

namespace DocsGenerator.Controllers
{
    public class ProjectVersionsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectVersionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ProjectVersions
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProjectVersions.ToListAsync());
        }

        // GET: ProjectVersions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectVersion = await _context.ProjectVersions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectVersion == null)
            {
                return NotFound();
            }

            return View(projectVersion);
        }

        // GET: ProjectVersions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProjectVersions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CommitHash,Branch,CreatedAt,DocsGenerated,DocsPath")] ProjectVersion projectVersion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectVersion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(projectVersion);
        }

        // GET: ProjectVersions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectVersion = await _context.ProjectVersions.FindAsync(id);
            if (projectVersion == null)
            {
                return NotFound();
            }
            return View(projectVersion);
        }

        // POST: ProjectVersions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CommitHash,Branch,CreatedAt,DocsGenerated,DocsPath")] ProjectVersion projectVersion)
        {
            if (id != projectVersion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectVersion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectVersionExists(projectVersion.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(projectVersion);
        }

        // GET: ProjectVersions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectVersion = await _context.ProjectVersions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectVersion == null)
            {
                return NotFound();
            }

            return View(projectVersion);
        }

        // POST: ProjectVersions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projectVersion = await _context.ProjectVersions.FindAsync(id);
            if (projectVersion != null)
            {
                _context.ProjectVersions.Remove(projectVersion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectVersionExists(int id)
        {
            return _context.ProjectVersions.Any(e => e.Id == id);
        }
    }
}
