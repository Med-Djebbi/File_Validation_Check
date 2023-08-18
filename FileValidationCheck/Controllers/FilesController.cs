using System.Security.Claims;
using System.Text.RegularExpressions;
using FileValidationCheck.Areas.Identity.Data;
using FileValidationCheck.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FileValidationCheck.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private readonly IConfiguration configuration;
        public FilesController(IConfiguration config)
        {
            this.configuration = config;
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Verification()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string connectionString = configuration.GetConnectionString("AuthDbContextConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    List<FichierModel> files = new List<FichierModel>();
                    string queryString1 = "Select * from Fichier";
                    SqlCommand command1 = new SqlCommand(queryString1, connection);
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
                    string queryString2 = "Select * from Colonne where fichierID=@ID and userID=@UID";
                    SqlCommand command2 = new SqlCommand(queryString2, connection);
                    SqlParameter parameter = new SqlParameter("@ID", System.Data.SqlDbType.Int);
                    SqlParameter parameter1 = new SqlParameter("@UID", System.Data.SqlDbType.NVarChar);
                    command2.Parameters.Add(parameter);
                    command2.Parameters.Add(parameter1);
                    IFormFileCollection filesCollection = Request.Form.Files;
                    for (int i = 0; i < filesCollection.Count; i++)
                    {
                        IFormFile uploaded_file = filesCollection[i];
                        List<ColonneModel> columns = new List<ColonneModel>();
                        bool test = false;
                        foreach (var db_file in files)
                        {
                            if (Regex.IsMatch(uploaded_file.FileName, db_file.format_nomination))
                            {
                                
                                using (StreamReader sr = new StreamReader(uploaded_file.OpenReadStream()))
                                {
                                    command2.Parameters["@ID"].Value = db_file.fichierID;
                                    command2.Parameters["@UID"].Value = userId;
                                    SqlDataReader reader2 = command2.ExecuteReader();
                                    while (reader2.Read())
                                    {
                                        ColonneModel column = new ColonneModel
                                        {
                                            colonneID= Convert.ToInt32(reader2["ColonneID"]),
                                            nom_colonne = reader2["nom_colonne"].ToString(),
                                            regle = reader2["regle"].ToString(),
                                            fichierID = Convert.ToInt32(reader2["fichierID"]),
                                            ordre = Convert.ToInt32(reader2["ordre"]),
                                            userID= reader2["userID"].ToString()
                                        };
                                        columns.Add(column);
                                    }
                                    reader2.Close();
                                    columns = columns.OrderBy(c => c.ordre).ToList();
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        // Split the line into columns based on the separator
                                        string[] rowData = line.Split(new string[] { db_file.separateur }, StringSplitOptions.None);
                                        if (rowData.Length < 1) { throw new Exception("Files Verification Failed."); }
                                        for (int j = 0; j < rowData.Length; j++)
                                        {
                                            if (!Regex.IsMatch(rowData[j], columns[j].regle)) { throw new Exception("Files Verification Failed."); }
                                        }
                                    }
                                    // Read the file data as bytes
                                    byte[] fileData;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        sr.BaseStream.CopyTo(ms);
                                        fileData = ms.ToArray();
                                    }
                                    // Save the file data to the database
                                    string queryString = "INSERT INTO UploadedFiles (Name, FileContent, UploadDate,UserID) VALUES (@FileName, @FileData, @UploadDate,@UID)";
                                    SqlCommand command = new SqlCommand(queryString, connection);
                                    SqlParameter parameter2 = new SqlParameter("@FileName", System.Data.SqlDbType.VarChar);
                                    SqlParameter parameter3 = new SqlParameter("@FileData", System.Data.SqlDbType.VarBinary);
                                    SqlParameter parameter4 = new SqlParameter("@UploadDate", System.Data.SqlDbType.DateTime);
                                    SqlParameter parameter5 = new SqlParameter("@UID", System.Data.SqlDbType.NVarChar);
                                    command.Parameters.Add(parameter2);
                                    command.Parameters.Add(parameter3);
                                    command.Parameters.Add(parameter4);
                                    command.Parameters.Add(parameter5);
                                    command.Parameters["@FileName"].Value = uploaded_file.FileName;
                                    command.Parameters["@FileData"].Value = fileData;
                                    command.Parameters["@UploadDate"].Value = DateTime.Now;
                                    command.Parameters["@UID"].Value = userId;
                                    command.ExecuteNonQuery();
                                }
                                test = true;
                                break;
                            }
                            
                        }
                        if (!test) { throw new Exception("Files Verification Failed."); }
                    }
                    connection.Close();
                    return Json(new {success=true, responseText="Files Successfully Submitted !"});
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions during the verification process
                return Json(new { success = false, responseText = ex.Message });
            }
        }
    }
}