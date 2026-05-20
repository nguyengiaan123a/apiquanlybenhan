using System.Linq.Expressions;
using yhctapp.Helpper;

namespace yhctapp.Services.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<Status> Add(T entity);
        Task<Status> Update(T entity); // EF Core tự hiểu Id khi truyền cả cục entity vào, không cần tham số id rời
        Task<Status> Delete(int id);
        Task<T> GetById(int id);

        // Đổi string search thành Expression để bảng nào search cột nào cũng được
        Task<(int totalpages, IReadOnlyList<T>)> GetAll(
            int page,
            int pagesize,
       
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null
            
            );
    }
}