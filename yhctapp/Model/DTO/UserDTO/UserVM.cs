using System.ComponentModel.DataAnnotations;

namespace yhctapp.Model.DTO.UserDTO
{
    public class UserVM
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "Tài khoản không được để trống")]
        public string Username { get; set; } = null!;
        [Required(ErrorMessage = "Password không được để trống")]
        [RegularExpression(@"^$|^(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
         ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm ít nhất 1 chữ in hoa, 1 số và 1 ký tự đặc biệt")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string Fullname { get; set; } = null!;

        [Required(ErrorMessage = "Khoa không được để trống")]
        public string IdDepartmentRoom { get; set; } = null!;

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        public string RoleName { get; set; } = null!;

    }
}
