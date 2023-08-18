using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileValidationCheck.Data;
using FileValidationCheck.Models;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FileValidationCheck.Controllers
{
    [Authorize]
    public class UploadedFilesModelsController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration configuration;

        public UploadedFilesModelsController(AuthDbContext context, IConfiguration config)
        {
            _context = context;
            configuration=config;

        }

        // GET: UploadedFilesModels
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string connectionString = configuration.GetConnectionString("AuthDbContextConnection");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            List<UploadedFilesModel> files = new List<UploadedFilesModel>();
            string queryString1 = "Select * from UploadedFiles where UserID=@UID";
            SqlCommand command1 = new SqlCommand(queryString1, connection);
            SqlParameter parameter1 = new SqlParameter("@UID", System.Data.SqlDbType.NVarChar);
            command1.Parameters.Add(parameter1);
            command1.Parameters["@UID"].Value = userId;
            SqlDataReader reader1 = command1.ExecuteReader();
            while (reader1.Read())
            {
                UploadedFilesModel file = new UploadedFilesModel
                {
                    FileID = Convert.ToInt32(reader1["FileID"]),
                    Name = reader1["Name"].ToString(),
                    UploadDate= Convert.ToDateTime(reader1["UploadDate"]),
                    UserID = reader1["UserID"].ToString()
                };
                files.Add(file);
            }
            reader1.Close();
            return _context.UploadedFiles != null ? 
                          View(files.ToList()) :
                          Problem("Entity set 'AuthDbContext.UploadedFiles'  is null.");
        }


        // GET: UploadedFilesModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UploadedFiles == null)
            {
                return NotFound();
            }

            var uploadedFilesModel = await _context.UploadedFiles
                .FirstOrDefaultAsync(m => m.FileID == id);
            if (uploadedFilesModel == null)
            {
                return NotFound();
            }

            return View(uploadedFilesModel);
        }

        // POST: UploadedFilesModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (_context.UploadedFiles == null)
            {
                return Problem("Entity set 'AuthDbContext.UploadedFilesModel'  is null.");
            }
            var uploadedFilesModel = await _context.UploadedFiles.FindAsync(id);
            if (uploadedFilesModel != null)
            {
                _context.UploadedFiles.Remove(uploadedFilesModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UploadedFilesModelExists(int? id)
        {
          return (_context.UploadedFiles?.Any(e => e.FileID == id)).GetValueOrDefault();
        }
    }
}
