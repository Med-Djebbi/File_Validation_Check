using System.ComponentModel.DataAnnotations;

namespace FileValidationCheck.Models
{
    public class UploadedFilesModel
    {
        [Key]
        public int FileID { get; set; }
        public string Name { get; set; }
        public byte[] FileContent { get; set; }
        public DateTime UploadDate { get; set; }
        public string UserID { get; set; }

    }
}
