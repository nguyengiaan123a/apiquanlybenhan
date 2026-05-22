using yhctapp.Helpper;
using yhctapp.Model.DTO;
using yhctapp.Model.Enitity;

namespace yhctapp.Services.Interface
{
    public interface IDocumentRecordRepository
    {
        Task<(int totalPages, List<DocumentRecordVM> data)> GetAll(
            int page, int pageSize,
            string? departmentId, bool isAdmin,
            string? search = null,
            string? Id_DocumentGroup = null,
            string? Id_DepartmentRoom = null);

        Task<DocumentRecordVM?> GetById(int id, string? departmentId, bool isAdmin);

        Task<Status> Add(DocumentRecord entity);

        Task<Status> Update(int id, DocumentRecord updatedData, string? departmentId, bool isAdmin);

        Task<Status> Delete(int id, string? departmentId, bool isAdmin);
    }
}
