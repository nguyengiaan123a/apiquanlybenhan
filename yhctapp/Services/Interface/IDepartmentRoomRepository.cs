using System.Linq.Expressions;
using yhctapp.Helpper;
using yhctapp.Model.Enitity;

namespace yhctapp.Services.Interface
{
    public interface IDepartmentRoomRepository
    {
        Task<Status> Add(DepartmentRoom entity);
        Task<Status> Update(DepartmentRoom entity);
        Task<Status> Delete(string id);
        Task<DepartmentRoom> GetById(string id);
        Task<(int totalpages, IReadOnlyList<DepartmentRoom>)> GetAll(
            int page,
            int pagesize,
            Expression<Func<DepartmentRoom, bool>> filter = null,
            Func<IQueryable<DepartmentRoom>, IOrderedQueryable<DepartmentRoom>> orderBy = null
        );
    }
}
