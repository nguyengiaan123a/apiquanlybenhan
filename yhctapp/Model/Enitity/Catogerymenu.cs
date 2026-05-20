using yhctapp.Model.Enitity.Base;

namespace yhctapp.Model.Enitity
{
    public class Catogerymenu : BaseDomainEnitity
    {
        public Catogerymenu()
        {
            Menus = new List<Menu>();
        }
        public int Order { get; set; }

        public List<Menu> Menus { get; set; }

    }
}
