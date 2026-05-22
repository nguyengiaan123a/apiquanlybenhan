using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Model.DTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface.Role;

namespace yhctapp.Services
{
    public class PermissonResponsive : IRolePermisson
    {
        private readonly MyDbcontext _context;
        private readonly IMapper _mapper;

        public PermissonResponsive(MyDbcontext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RolePermissonVM>> RolePermisson()
        {
            var data = await _context.Set<RolePermission>()
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<List<RolePermissonVM>>(data);
        }

        public async Task<Status> AddRolePermisson(List<RolePermissonVM> rolePermissonVMs)
        {
            if (rolePermissonVMs == null || !rolePermissonVMs.Any())
                return new Status { Code = 0, Message = "Danh sách quyền trống!" };

            try
            {
                var entities = _mapper.Map<List<RolePermission>>(rolePermissonVMs);
                await _context.Set<RolePermission>().AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return new Status { Code = 1, Message = "Thêm quyền thành công!" };
            }
            catch (Exception ex)
            {
                return new Status { Code = -1, Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<Status> DeleteRolePermisson(List<int> ids)
        {
            try
            {
                var targets = await _context.Set<RolePermission>()
                    .Where(x => ids.Contains(x.Id_RolePermission))
                    .ToListAsync();

                if (!targets.Any()) return new Status { Code = 1, Message = "Không có gì để xóa." };

                _context.Set<RolePermission>().RemoveRange(targets);
                await _context.SaveChangesAsync();
                return new Status { Code = 1, Message = "Xóa thành công!" };
            }
            catch (Exception ex)
            {
                return new Status { Code = -1, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // FIX: Sửa lỗi Transaction Execution Strategy
        public async Task<Status> UpdateRolePermisson(List<int> oldIds, List<RolePermissonVM> newPermissions)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Xóa quyền cũ
                    if (oldIds != null && oldIds.Any())
                    {
                        var oldEntities = await _context.Set<RolePermission>()
                            .Where(x => oldIds.Contains(x.Id_RolePermission))
                            .ToListAsync();
                        if (oldEntities.Any()) _context.Set<RolePermission>().RemoveRange(oldEntities);
                    }

                    // 2. Thêm quyền mới
                    if (newPermissions != null && newPermissions.Any())
                    {
                        var newEntities = _mapper.Map<List<RolePermission>>(newPermissions);
                        await _context.Set<RolePermission>().AddRangeAsync(newEntities);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new Status { Code = 1, Message = "Cập nhật thành công!" };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new Status { Code = -1, Message = $"Lỗi hệ thống: {ex.Message}" };
                }
            });
        }
    }
}