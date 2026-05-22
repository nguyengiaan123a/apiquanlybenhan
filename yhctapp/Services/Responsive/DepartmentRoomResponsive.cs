using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Services.Responsive
{
    public class DepartmentRoomResponsive : IDepartmentRoomRepository
    {
        private readonly MyDbcontext _dbcontext;
        private readonly DbSet<DepartmentRoom> _dbSet;

        public DepartmentRoomResponsive(MyDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
            _dbSet = dbcontext.Set<DepartmentRoom>();
        }

        public async Task<Status> Add(DepartmentRoom entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Thêm mới thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<Status> Update(DepartmentRoom entity)
        {
            try
            {
                _dbSet.Update(entity);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Cập nhật thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<Status> Delete(string id)
        {
            try
            {
                var item = await _dbSet.FindAsync(id);
                if (item == null)
                    return new Status { Code = 0, Message = "Không tìm thấy dữ liệu để xóa" };

                _dbSet.Remove(item);
                await _dbcontext.SaveChangesAsync();
                return new Status { Code = 1, Message = "Xóa thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 0, Message = "Lỗi: " + ex.Message };
            }
        }

        public async Task<DepartmentRoom> GetById(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<(int totalpages, IReadOnlyList<DepartmentRoom>)> GetAll(
            int page,
            int pagesize,
            Expression<Func<DepartmentRoom, bool>> filter = null,
            Func<IQueryable<DepartmentRoom>, IOrderedQueryable<DepartmentRoom>> orderBy = null)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pagesize);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var data = await query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            return (totalPages, data);
        }
    }
}
