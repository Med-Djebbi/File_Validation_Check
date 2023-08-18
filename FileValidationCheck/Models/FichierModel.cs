using System.ComponentModel.DataAnnotations;

namespace FileValidationCheck.Models
{
    public class FichierModel
    {
        [Key]
        public int fichierID { get; set; }
        public string? format_nomination { get; set;}
        public string? separateur { get; set;}
        public string? userID { get; set; }
        public List<ColonneModel> Colonnes { get; set; }

    }
}