using yhctapp.Model.Enitity.Base;


namespace yhctapp.Model.Enitity
{
    public class DocumentGroup : BaseDomainEnitity
    {
        public string Id_DepartmentRoom { get; set; }
        public DepartmentRoom DepartmentRoom { get; set; }
        public List<DocumentRecord> DocumentRecords { get; set; } = new List<DocumentRecord>();
    }

}
