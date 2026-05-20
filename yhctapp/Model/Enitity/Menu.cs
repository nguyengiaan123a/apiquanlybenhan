using yhctapp.Model.Enitity.Base;

namespace yhctapp.Model.Enitity
{
    public class Menu : BaseDomainEnitity
    {
        public int Order { get; set; }

        public int Id_menu { get; set; }

        public string url { get; set; }

        public Catogerymenu Catogerymenu { get; set; }
    }
}
