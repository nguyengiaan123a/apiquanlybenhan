namespace yhctapp.Model.Enitity.Base
{
    public abstract class BaseDomainEnitity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string ?Thumnail { get; set; }



    }
}
