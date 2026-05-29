namespace yhctapp.Model.Enitity
{
    public class DocumentRecord
    {
        public int Id { get; set; }

        // Mã Hồ Sơ: người dùng tự nhập
        public string MaHoSo { get; set; } = string.Empty;

        // Tên Danh Mục Hồ Sơ/Tài liệu
        public string Title { get; set; } = string.Empty;

        // FK → Phòng Chức Năng
        public string Id_DepartmentRoom { get; set; } = string.Empty;

        // Năm hiệu lực (ví dụ: 2024)
        public int NamHieuLuc { get; set; }

        // Thời Hạn Lưu Trữ (số năm, 999 = vĩnh viễn)
        public int ThoiHanLuuTru { get; set; }

        // Năm hết hạn: computed property
        public int? NamHetHan => ThoiHanLuuTru >= 999 ? null : NamHieuLuc + ThoiHanLuuTru;

        // Vị Trí Lưu Trữ (Kho/Tủ/Kệ)
        public string? ViTriLuuTru { get; set; }

        // Người Quản Lý
        public string? NguoiQuanLy { get; set; }

        // Tình trạng (Tốt / Hư hỏng nhẹ / Hư hỏng nặng / Mất)
        public string? TinhTrang { get; set; }

        // Trạng Thái: computed property
        public string TrangThai
        {
            get
            {
                // 1. Nếu NamHieuLuc = 0 (ô E trống) → trả về chuỗi rỗng
                if (NamHieuLuc == 0) return "";

                // 2. Nếu ThoiHanLuuTru >= 999 (Vĩnh viễn) → AN TOÀN
                if (ThoiHanLuuTru >= 999) return "AN TOÀN";

                int currentYear = DateTime.Now.Year;

                // 3. Nếu năm hết hạn < năm hiện tại → ĐÃ HẾT HẠN
                if (NamHetHan.HasValue && NamHetHan.Value < currentYear)
                    return "ĐÃ HẾT HẠN - CẦN TIÊU HỦY";

                // 4. Nếu năm hết hạn = năm hiện tại → SẮP HẾT HẠN
                if (NamHetHan.HasValue && NamHetHan.Value == currentYear)
                    return "SẮP HẾT HẠN (Trong năm nay)";

                // 5. Các trường hợp còn lại → AN TOÀN
                return "AN TOÀN";
            }
        }

        // Mức độ bảo mật (Thường / Mật / Tối mật)
        public string? MucDoBaoMat { get; set; }

        // Ghi Chú
        public string? GhiChu { get; set; }

        // FK → Nhóm Hồ Sơ (DocumentGroup)
        public int? Id_DocumentGroup { get; set; }

        // Loại hồ sơ
        public string? LoaiHoSo { get; set; }

        // Số lần
        public int? SoLan { get; set; }

        // Ngày tạo record
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ===== Navigation Properties =====
        public DepartmentRoom DepartmentRoom { get; set; } = null!;
        public DocumentGroup? DocumentGroup { get; set; }
        public virtual ICollection<DocumentFile> DocumentFiles { get; set; } = new List<DocumentFile>();
    }
}
