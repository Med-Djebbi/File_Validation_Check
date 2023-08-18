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
using System.Configuration;

namespace FileValidationCheck.Controllers
{
    [Authorize]
    public class ColonneModelsController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration configuration;

        public ColonneModelsController(AuthDbContext context, IConfiguration config)
        {
            _context = context;
            configuration = config;
        }

        // GET: ColonneModels
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string connectionString = configuration.GetConnectionString("AuthDbContextConnection");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            List<ColonneModel> columns = new List<ColonneModel>();
            string queryString1 = "Select * from Colonne where userID=@UID";
            SqlCommand command1 = new SqlCommand(queryString1, connection);
            SqlParameter parameter1 = new SqlParameter("@UID", System.Data.SqlDbType.NVarChar);
            command1.Parameters.Add(parameter1);
            command1.Parameters["@UID"].Value = userId;
            SqlDataReader reader1 = command1.ExecuteReader();
            while (reader1.Read())
            {
                ColonneModel column = new ColonneModel
                {
                    colonneID = Convert.ToInt32(reader1["ColonneID"]),
                    nom_colonne = reader1["nom_colonne"].ToString(),
                    regle = reader1["regle"].ToString(),
                    fichierID = Convert.ToInt32(reader1["fichierID"]),
                    ordre = Convert.ToInt32(reader1["ordre"]),
                    userID = reader1["userID"].ToString()
                };
                columns.Add(column);
            }
            reader1.Close();
            return _context.Colonne != null ? 
                          View( columns.ToList()) :
                          Problem("Entity set 'AuthDbContext.Colonne'  is null.");
        }

        // GET: ColonneModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Colonne == null)
            {
                return NotFound();
            }

            var colonneModel = await _context.Colonne
                .FirstOrDefaultAsync(m => m.colonneID == id);
            if (colonneModel == null)
            {
                return NotFound();
            }

            return View(colonneModel);
        }

        // GET: ColonneModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ColonneModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("colonneID,nom_colonne,regle,fichierID,ordre,userID")] ColonneModel colonneModel)
        {
                _context.Add(colonneModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        // GET: ColonneModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Colonne == null)
            {
                return NotFound();
            }

            var colonneModel = await _context.Colonne.FindAsync(id);
            if (colonneModel == null)
            {
                return NotFound();
            }
            return View(colonneModel);
        }

        // POST: ColonneModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("colonneID,nom_colonne,regle,fichierID,ordre,userID")] ColonneModel colonneModel)
        {
            if (id != colonneModel.colonneID)
            {
                return NotFound();
            }

                try
                {
                    _context.Update(colonneModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ColonneModelExists(colonneModel.colonneID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
        }

        // GET: ColonneModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Colonne == null)
            {
                return NotFound();
            }

            var colonneModel = await _context.Colonne
                .FirstOrDefaultAsync(m => m.colonneID == id);
            if (colonneModel == null)
            {
                return NotFound();
            }

            return View(colonneModel);
        }

        // POST: ColonneModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Colonne == null)
            {
                return Problem("Entity set 'AuthDbContext.Colonne'  is null.");
            }
            var colonneModel = await _context.Colonne.FindAsync(id);
            if (colonneModel != null)
            {
                _context.Colonne.Remove(colonneModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ColonneModelExists(int id)
        {
          return (_context.Colonne?.Any(e => e.colonneID == id)).GetValueOrDefault();
        }
    }
}
