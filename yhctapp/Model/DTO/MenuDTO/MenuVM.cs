using System.ComponentModel.DataAnnotations;
using yhctapp.Model.DTO.BaseDTO;

namespace yhctapp.Model.DTO.UserDTO
{
    public class MenuVM : BaseDomainDTO
    {
        public int Order { get; set; }

        public int Id_menu { get; set; }

        public string url { get; set; }






    }
}
