using System.ComponentModel.DataAnnotations;

namespace yhctapp.Model.DTO.BaseDTO
{
    public abstract class BaseDomainDTO
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Tiêu đề không được để trống ")]
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string? Thumnail { get; set; }
    }
}
