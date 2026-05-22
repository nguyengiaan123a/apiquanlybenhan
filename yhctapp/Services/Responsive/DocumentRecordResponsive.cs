using Microsoft.EntityFrameworkCore;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Model.DTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Services.Responsive
{
    public class DocumentRecordResponsive : IDocumentRecordRepository
    {
        private readonly MyDbcontext _dbcontext;

        public DocumentRecordResponsive(MyDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<(int totalPages, List<DocumentRecordVM> data)> GetAll(
            int page, int pageSize,
            string? departmentId, bool isAdmin,
            string? search = null,
            string? Id_DocumentGroup = null,
            string? Id_DepartmentRoom = null)
        {
            try
            {
                var query = _dbcontext.DocumentRecords
                    .AsNoTracking()
                    .Include(x => x.DepartmentRoom)
                    .Include(x => x.DocumentGroup)
                    .AsQueryable();

                // Phân quyền: Admin xem tất cả (hoặc lọc theo phòng ban được chọn), user thường chỉ xem phòng mình
                if (!isAdmin && !string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(x => x.Id_DepartmentRoom == departmentId);
                }
                else if (isAdmin && !string.IsNullOrEmpty(Id_DepartmentRoom))
                {
                    query = query.Where(x => x.Id_DepartmentRoom == Id_DepartmentRoom);
                }

                if (!string.IsNullOrEmpty(Id_DocumentGroup) && int.TryParse(Id_DocumentGroup, out int groupId))
                {
                    query = query.Where(x => x.Id_DocumentGroup == groupId);
                }

                // Tìm kiếm theo tên hồ sơ
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(x => x.Title.Contains(search));
                }

                int totalCount = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var data = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map sang DTO với computed fields
                var result = data.Select(MapToVM).ToList();

                return (totalPages, result);
            }
            catch (Exception)
            {
                return (0, new List<DocumentRecordVM>());
            }
        }

        public async Task<DocumentRecordVM?> GetById(int id, string? departmentId, bool isAdmin)
        {
            var query = _dbcontext.DocumentRecords
                .AsNoTracking()
                .Include(x => x.DepartmentRoom)
                .Include(x => x.DocumentGroup)
                .AsQueryable();

            // Phân quyền
            if (!isAdmin && !string.IsNullOrEmpty(departmentId))
            {
                query = query.Where(x => x.Id_DepartmentRoom == departmentId);
            }

            var entity = await query.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return null;

            return MapToVM(entity);
        }

        public async Task<Status> Add(DocumentRecord entity)
        {
            try
            {
                await _dbcontext.DocumentRecords.AddAsync(entity);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Thêm hồ sơ thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<Status> Update(int id, DocumentRecord updatedData, string? departmentId, bool isAdmin)
        {
            try
            {
                var query = _dbcontext.DocumentRecords.AsQueryable();

                // Phân quyền: chỉ cho phép sửa hồ sơ trong phòng ban (trừ Admin)
                if (!isAdmin && !string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(x => x.Id_DepartmentRoom == departmentId);
                }

                var existing = await query.FirstOrDefaultAsync(x => x.Id == id);
                if (existing == null)
                    return new Status { Code = 0, Message = "Không tìm thấy hồ sơ hoặc không có quyền chỉnh sửa" };

                existing.MaHoSo = updatedData.MaHoSo;
                existing.Title = updatedData.Title;
                existing.NamHieuLuc = updatedData.NamHieuLuc;
                existing.ThoiHanLuuTru = updatedData.ThoiHanLuuTru;
                existing.ViTriLuuTru = updatedData.ViTriLuuTru;
                existing.NguoiQuanLy = updatedData.NguoiQuanLy;
                existing.TinhTrang = updatedData.TinhTrang;
                existing.MucDoBaoMat = updatedData.MucDoBaoMat;
                existing.GhiChu = updatedData.GhiChu;
                existing.Id_DocumentGroup = updatedData.Id_DocumentGroup;

                _dbcontext.DocumentRecords.Update(existing);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Cập nhật hồ sơ thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<Status> Delete(int id, string? departmentId, bool isAdmin)
        {
            try
            {
                var query = _dbcontext.DocumentRecords.AsQueryable();

                // Phân quyền
                if (!isAdmin && !string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(x => x.Id_DepartmentRoom == departmentId);
                }

                var entity = await query.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    return new Status { Code = 0, Message = "Không tìm thấy hồ sơ hoặc không có quyền xóa" };

                _dbcontext.DocumentRecords.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Xóa hồ sơ thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi: " + ex.Message };
            }
        }

        // ===== Helper: Map Entity → DTO với computed fields =====
        private static DocumentRecordVM MapToVM(DocumentRecord entity)
        {
            int? namHetHan = entity.ThoiHanLuuTru >= 999 ? null : entity.NamHieuLuc + entity.ThoiHanLuuTru;

            // Logic trạng thái mới
            string trangThai;
            if (entity.NamHieuLuc == 0)
                trangThai = "";
            else if (entity.ThoiHanLuuTru >= 999)
                trangThai = "AN TOÀN";
            else if (namHetHan.HasValue && namHetHan.Value < DateTime.Now.Year)
                trangThai = "ĐÃ HẾT HẠN - CẦN TIÊU HỦY";
            else if (namHetHan.HasValue && namHetHan.Value == DateTime.Now.Year)
                trangThai = "SẮP HẾT HẠN (Trong năm nay)";
            else
                trangThai = "AN TOÀN";

            return new DocumentRecordVM
            {
                Id = entity.Id,
                MaHoSo = entity.MaHoSo,
                Title = entity.Title,
                Id_DepartmentRoom = entity.Id_DepartmentRoom,
                NamHieuLuc = entity.NamHieuLuc,
                ThoiHanLuuTru = entity.ThoiHanLuuTru,
                NamHetHan = namHetHan,
                ViTriLuuTru = entity.ViTriLuuTru,
                NguoiQuanLy = entity.NguoiQuanLy,
                TinhTrang = entity.TinhTrang,
                TrangThai = trangThai,
                MucDoBaoMat = entity.MucDoBaoMat,
                GhiChu = entity.GhiChu,
                Id_DocumentGroup = entity.Id_DocumentGroup,
                TenNhomHoSo = entity.DocumentGroup?.Title,
                TenPhongBan = entity.DepartmentRoom?.Room,
                CreatedDate = entity.CreatedDate
            };
        }

        public async Task<List<DocumentFileVM>> GetFilesByRecordId(int recordId)
        {
            var files = await _dbcontext.DocumentFiles
                .AsNoTracking()
                .Where(x => x.Id_DocumentRecord == recordId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return files.Select(x => new DocumentFileVM
            {
                Id = x.Id,
                FileName = x.FileName,
                FilePath = x.FilePath,
                FileType = x.FileType,
                FileSize = x.FileSize,
                CreatedDate = x.CreatedDate,
                Id_DocumentRecord = x.Id_DocumentRecord
            }).ToList();
        }

        public async Task<Status> AddFile(DocumentFile file)
        {
            try
            {
                await _dbcontext.DocumentFiles.AddAsync(file);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Thêm file thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi thêm file: " + ex.Message };
            }
        }

        public async Task<DocumentFile?> GetFileById(int fileId)
        {
            return await _dbcontext.DocumentFiles.FindAsync(fileId);
        }

        public async Task<Status> DeleteFile(int fileId)
        {
            try
            {
                var file = await _dbcontext.DocumentFiles.FindAsync(fileId);
                if (file == null) return new Status { Code = 0, Message = "Không tìm thấy file" };

                _dbcontext.DocumentFiles.Remove(file);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Xóa file thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi xóa file: " + ex.Message };
            }
        }
    }
}
