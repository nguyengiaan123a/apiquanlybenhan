namespace yhctapp.Model.DTO.AuthorizationDTO
{
    public class AuthorizationVM
    {
        public string Title_ctmenu { get; set; }

        public string thumnail { get; set; }
        
        public int Order_ct { get; set; }

        public List<MenuAuthoVM> Menus { get; set; } = new List<MenuAuthoVM>();


    }
}
