using System.ComponentModel.DataAnnotations;

namespace FileValidationCheck.Models
{
    public class ColonneModel
    {
        [Key]
        public int colonneID { get; set; }
        public string? nom_colonne { get; set; }
        public string? regle { get; set; }
        public int fichierID { get; set; }
        public int ordre { get; set; }
        public string userID { get; set; }
        public FichierModel Fichier { get; set; }
    }
}