using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Model.Enitity.Base;
using yhctapp.Services.Interface;

namespace yhctapp.Services.Responsive // Sửa lại tên folder thành Repository cho chuẩn nha ông
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseDomainEnitity
    {
     
        protected readonly MyDbcontext _dbcontext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(MyDbcontext dbcontext)
        {
           
            _dbcontext = dbcontext;
            _dbSet = dbcontext.Set<T>(); // Xác định DbContext đang làm việc với bảng T nào
        }

        public async Task<Status> Add(T entity)
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

        public async Task<Status> Update(T entity)
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

        public async Task<Status> Delete(int id)
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

        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<(int totalpages, IReadOnlyList<T>)> GetAll(
            int page,
            int pagesize,
 
 
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null
            )
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            // Nếu có truyền điều kiện search thì nhét vào đây

      

            if (filter != null)
            {
                query = query.Where(filter);
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pagesize);

            // Nếu có truyền điều kiện sắp xếp (ví dụ: order by CreatedDate desc)
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