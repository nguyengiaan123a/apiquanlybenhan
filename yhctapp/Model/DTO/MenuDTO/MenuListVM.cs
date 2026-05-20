using System.ComponentModel.DataAnnotations;
using yhctapp.Model.DTO.BaseDTO;

namespace yhctapp.Model.DTO.UserDTO
{
    public class MenuListVM
    {

        public int Id_Ctmenu { get; set; }

        public string Title_Ctmenu { get; set; }



        public List<MenuVM> Menus { get; set; }








    }
}
