using yhctapp.Model.Enitity.Base;

namespace yhctapp.Model.Enitity
{
    public class DepartmentRoom 
    {
        public DepartmentRoom()
        {
            ApplicationUsers = new List<ApplicationUser>();
            DocumentGroups = new List<DocumentGroup>();
            DocumentRecords = new List<DocumentRecord>();
        }
        public string Id { get; set;}
        public string Room { get; set; }
        public DateTime DateCreated { get; set;} = DateTime.Now;
        public int Status { get; set;} =1;
        public List<ApplicationUser> ApplicationUsers { get; set;}
        public List<DocumentGroup> DocumentGroups { get; set;}
        public List<DocumentRecord> DocumentRecords { get; set;}
    }

}
