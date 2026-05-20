namespace yhctapp.Model.Enitity
{
    public class RolePermission 
    {

        public int Id_RolePermission { get; set; }
        public string RoleId { get; set; }

        public int Id_menu { get; set; }
        public Approle Approle { get; set; }

    }
}
