using System;

namespace yhctapp.Model.Enitity
{
    public class DocumentFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // FK -> DocumentRecord
        public int Id_DocumentRecord { get; set; }
        public DocumentRecord DocumentRecord { get; set; } = null!;
    }
}
