using System.ComponentModel.DataAnnotations;

namespace yhctapp.Model.DTO
{
    public class DepartmentRoomVM
    {
        [Required(ErrorMessage = "Mã phòng ban không được để trống")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tên phòng ban không được để trống")]
        [StringLength(200, ErrorMessage = "Tên phòng ban không được vượt quá 200 ký tự")]
        public string Room { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public int Status { get; set; } = 1;
    }
}
