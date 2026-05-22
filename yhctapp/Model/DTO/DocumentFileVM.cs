using System;

namespace yhctapp.Model.DTO
{
    public class DocumentFileVM
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Id_DocumentRecord { get; set; }
    }
}
