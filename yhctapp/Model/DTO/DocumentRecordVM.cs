using System.ComponentModel.DataAnnotations;

namespace yhctapp.Model.DTO
{
    public class DocumentRecordVM
    {
        public int? Id { get; set; }

        // Mã Hồ Sơ (computed, read-only)
        public string? MaHoSo { get; set; }

        [Required(ErrorMessage = "Tên hồ sơ không được để trống")]
        public string Title { get; set; } = string.Empty;

        // Phòng Chức Năng (auto-gán từ JWT claim)
        public string? Id_DepartmentRoom { get; set; }

        [Required(ErrorMessage = "Năm hiệu lực không được để trống")]
        public int NamHieuLuc { get; set; }

        [Required(ErrorMessage = "Thời hạn lưu trữ không được để trống")]
        public int ThoiHanLuuTru { get; set; }

        // Năm hết hạn (computed, read-only)
        public int? NamHetHan { get; set; }

        public string? ViTriLuuTru { get; set; }

        public string? NguoiQuanLy { get; set; }

        // Tình trạng: Tốt / Hư hỏng nhẹ / Hư hỏng nặng / Mất
        public string? TinhTrang { get; set; }

        // Trạng Thái (computed, read-only)
        public string? TrangThai { get; set; }

        // Mức độ bảo mật: Thường / Mật / Tối mật
        public string? MucDoBaoMat { get; set; }

        public string? GhiChu { get; set; }

        // FK → Nhóm Hồ Sơ
        public int? Id_DocumentGroup { get; set; }

        public string? LoaiHoSo { get; set; }

        public int? SoLan { get; set; }

        // Tên hiển thị (include từ navigation)
        public string? TenNhomHoSo { get; set; }
        public string? TenPhongBan { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
