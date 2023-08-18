using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileValidationCheck.Data;
using FileValidationCheck.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace FileValidationCheck.Controllers
{
    [Authorize]
    public class FichierModelsController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration configuration;

        public FichierModelsController(AuthDbContext context, IConfiguration config)
        {
            _context = context;
            configuration = config;
        }

        // GET: FichierModels
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string connectionString = configuration.GetConnectionString("AuthDbContextConnection");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            List<FichierModel> files = new List<FichierModel>();
            string queryString1 = "Select * from Fichier where userID=@UID";
            SqlCommand command1 = new SqlCommand(queryString1, connection);
            SqlParameter parameter1 = new SqlParameter("@UID", System.Data.SqlDbType.NVarChar);
            command1.Parameters.Add(parameter1);
            command1.Parameters["@UID"].Value = userId;
            SqlDataReader reader1 = command1.ExecuteReader();
            while (reader1.Read())
            {
                FichierModel file = new FichierModel
                {
                    fichierID = Convert.ToInt32(reader1["fichierID"]),
                    format_nomination = reader1["format_nomination"].ToString(),
                    separateur = reader1["separateur"].ToString(),
                    userID = reader1["userID"].ToString()
                };
                files.Add(file);
            }
            reader1.Close();
            return _context.Fichier != null ? 
                          View(files.ToList()) :
                          Problem("Entity set 'AuthDbContext.Fichier'  is null.");
        }

        // GET: FichierModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Fichier == null)
            {
                return NotFound();
            }

            var fichierModel = await _context.Fichier
                .FirstOrDefaultAsync(m => m.fichierID == id);
            if (fichierModel == null)
            {
                return NotFound();
            }

            return View(fichierModel);
        }

        // GET: FichierModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FichierModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("fichierID,format_nomination,separateur,userID")] FichierModel fichierModel)
        {
                _context.Add(fichierModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        // GET: FichierModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Fichier == null)
            {
                return NotFound();
            }

            var fichierModel = await _context.Fichier.FindAsync(id);
            if (fichierModel == null)
            {
                return NotFound();
            }
            return View(fichierModel);
        }

        // POST: FichierModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("fichierID,format_nomination,separateur,userID")] FichierModel fichierModel)
        {
            if (id != fichierModel.fichierID)
            {
                return NotFound();
            }
                try
                {
                    _context.Update(fichierModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FichierModelExists(fichierModel.fichierID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
        }

        // GET: FichierModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Fichier == null)
            {
                return NotFound();
            }

            var fichierModel = await _context.Fichier
                .FirstOrDefaultAsync(m => m.fichierID == id);
            if (fichierModel == null)
            {
                return NotFound();
            }

            return View(fichierModel);
        }

        // POST: FichierModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Fichier == null)
            {
                return Problem("Entity set 'AuthDbContext.Fichier'  is null.");
            }
            var fichierModel = await _context.Fichier.FindAsync(id);
            if (fichierModel != null)
            {
                _context.Fichier.Remove(fichierModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FichierModelExists(int id)
        {
          return (_context.Fichier?.Any(e => e.fichierID == id)).GetValueOrDefault();
        }
    }
}
