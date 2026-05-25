using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Model.DTO.UserDTO;
using yhctapp.Model.Enitity;
using yhctapp.Services.Interface;

namespace yhctapp.Services.Responsive
{
    public class UserResponsive : IUser
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly MyDbcontext _dbcontext;
        private readonly IMapper _mapper;
        private readonly MyDbcontext _myDbcontext;

        public UserResponsive(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,MyDbcontext myDbcontext,IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dbcontext = myDbcontext;
            _mapper= mapper;
        }

        public async Task<Status> DeleteUser(string id)
        {
            var status = new Status();
            try
            {
                // 1. Tìm user theo Id
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    status.Code = 0; // 0 = Thất bại
                    status.Message = "Không tìm thấy người dùng này.";
                    return status;
                }

                // 2. Thực hiện xóa
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    status.Code = 1; // 1 = Thành công
                    status.Message = "Xóa người dùng thành công.";
                }
                else
                {
                    status.Code = 0;
                    // Gộp các lỗi từ Identity (nếu có) để trả về
                    status.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                // TODO: Ghi log lỗi tại đây
                status.Code = -1; // -1 = Lỗi hệ thống
                status.Message = "Đã xảy ra lỗi trong quá trình xóa.";
            }

            return status;
        }

        public async Task<(int totalpage, List<ListuserVM> listuser)> ListuserVM(int page, int pagesize, string search)
        {
            try
            {
                // 1. Tạo IQueryable cơ bản
                var query = _dbcontext.ApplicationUsers.AsNoTracking();

                // 2. Gộp truy vấn loại bỏ Role CUSTOMER vào câu lệnh chính 
                var customerUserIds = _dbcontext.UserRoles
                    .Where(ur => _dbcontext.Roles.Any(r => r.Id == ur.RoleId && r.NormalizedName == "CUSTOMER"))
                    .Select(ur => ur.UserId);

                query = query.Where(u => !customerUserIds.Contains(u.Id));

                // 3. Lọc theo chuỗi tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.UserName.Contains(search) || u.Fullname.Contains(search));
                }

                // 4. Đếm tổng số lượng và THOÁT SỚM nếu không có dữ liệu
                int totalItems = await query.CountAsync();
                if (totalItems == 0)
                {
                    return (0, new List<ListuserVM>());
                }

                int totalPages = (int)Math.Ceiling(totalItems / (double)pagesize);

                // 5. Lấy dữ liệu và map TRỰC TIẾP sang ViewModel kèm theo ROLE
                var users = await query
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize)
                    .Select(u => new ListuserVM
                    {
                        Id = u.Id,
                        Username = u.UserName,
                        FullName = u.Fullname,
                        // Lấy Role đầu tiên của User
                        Role = _dbcontext.Roles
                            .Where(r => _dbcontext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == r.Id))
                            .Select(r => r.Name)
                            .FirstOrDefault(),
                        Password=u.PasswordHash,
                        IdDepartmentRoom=u.IdDepartmentRoom,
                        ChucVu=u.ChucVu,
                    })
                    .ToListAsync();

                return (totalPages, users);
            }
            catch (Exception ex)
            {
                // TODO: Ghi log lỗi (Log.Error(ex, "Lỗi lấy danh sách user"))
                return (0, new List<ListuserVM>());
            }
        }

        public async Task<Status> Login(LoginUser user)
        {
            try
            {
                var existingUser = await _userManager.FindByNameAsync(user.Username);
                if (existingUser == null || !await _userManager.CheckPasswordAsync(existingUser, user.Password))
                {
                    return new Status { Code = 400, Message = "Sai tài khoản hoặc mật khẩu." };
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(existingUser);

                // Build JWT claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, existingUser.Id),
                    new Claim("Fullname", existingUser.Fullname ?? ""),
                    new Claim("IdDepartmentRoom", existingUser.IdDepartmentRoom.ToString())
                };
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // JWT settings from configuration
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: creds
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                return new Status { Code = 200, Message = jwt };
            }
            catch (Exception ex)
            {
                return new Status { Code = 500, Message = $"Đăng nhập thất bại: {ex.Message}" };
            }
        }

        public async Task<Status> RegisterAdmin(UserVM user)
        {
            try
            {
                // 1. Kiểm tra xem Client có truyền RoleName lên không
                if (string.IsNullOrWhiteSpace(user.RoleName))
                {
                    return new Status { Code = 400, Message = "Vui lòng cung cấp quyền (RoleName) cho tài khoản này." };
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(user.Username);
                if (existingUser != null)
                {
                    return new Status { Code = 400, Message = "Tài khoản đã tồn tại" };
                }

                // Create ApplicationUser
                var applicationUser = _mapper.Map<ApplicationUser>(user);

                var result = await _userManager.CreateAsync(applicationUser, user.Password);
                if (!result.Succeeded)
                {
                    return new Status
                    {
                        Code = 400,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description))
                    };
                }

                // ==========================================
                // GÁN ROLE ĐỘNG TỪ DỮ LIỆU TRUYỀN VÀO
                // ==========================================

                // Chuẩn hóa tên Role thành in hoa (ví dụ: "admin" -> "ADMIN") để dễ quản lý
                var dynamicRoleName = user.RoleName.Trim().ToUpper();

                // Kiểm tra xem Role đã tồn tại trong DB chưa, nếu chưa thì tạo mới
                if (!await _roleManager.RoleExistsAsync(dynamicRoleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(dynamicRoleName));
                }

                // Gán Role động cho User
                var roleResult = await _userManager.AddToRoleAsync(applicationUser, dynamicRoleName);
                if (!roleResult.Succeeded)
                {
                    return new Status
                    {
                        Code = 400,
                        Message = "Tạo tài khoản thành công nhưng gán quyền thất bại: " + string.Join("; ", roleResult.Errors.Select(e => e.Description))
                    };
                }
                // ==========================================

                return new Status { Code = 200, Message = $"Đăng ký thành công với quyền {dynamicRoleName}" };
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                return new Status { Code = 500, Message = $"Registration failed: {ex.Message} | Inner: {innerMsg}" };
            }
        }

        public async Task<Status> RegisterCustomer(UserVM user)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(user.Username);
                if (existingUser != null)
                {
                    return new Status { Code = 400, Message = "tài khoản đã tồn tại" };
                }

                // Create ApplicationUser
                var applicationUser = new ApplicationUser
                {
                    UserName = user.Username,
                    Fullname = user.Fullname,
                    IdDepartmentRoom = user.IdDepartmentRoom,
                    ChucVu = user.ChucVu,
                };

                var result = await _userManager.CreateAsync(applicationUser, user.Password);
                if (!result.Succeeded)
                {
                    return new Status
                    {
                        Code = 400,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description))
                    };
                }

                // Ensure CUSTOMER role exists
                if (!await _roleManager.RoleExistsAsync(Helpper.Approle.CUSTOMER))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Helpper.Approle.CUSTOMER));
                }

                // Assign CUSTOMER role
                await _userManager.AddToRoleAsync(applicationUser, Helpper.Approle.CUSTOMER);

                return new Status { Code = 200, Message = "Đăng ký thành công." };
            }
            catch (Exception ex)
            {
                return new Status { Code = 500, Message = $"đăng ký thất bại: {ex.Message}" };
            }
        }

        public async Task<Status> UpdateUser(string id, UserVM user)
        {
            try
            {
                // 1. Tìm user
                var existingUser = await _userManager.FindByIdAsync(id);
                if (existingUser == null)
                {
                    return new Status { Code = 404, Message = "Không tìm thấy người dùng" };
                }

                // 2. Check username trùng
                var checkUsername = await _userManager.FindByNameAsync(user.Username);
                if (checkUsername != null && checkUsername.Id != id)
                {
                    return new Status { Code = 400, Message = "Tên tài khoản đã tồn tại" };
                }

                // 3. Map dữ liệu
                _mapper.Map(user, existingUser);

                // 4. Update thông tin cơ bản
                var result = await _userManager.UpdateAsync(existingUser);
                if (!result.Succeeded)
                {
                    return new Status { Code = 400, Message = string.Join("; ", result.Errors.Select(x => x.Description)) };
                }

                // 5. Update password nếu có nhập password mới
                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    var isSamePassword = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                    if (!isSamePassword)
                    {
                        // xóa password cũ
                        var removePasswordResult = await _userManager.RemovePasswordAsync(existingUser);
                        if (!removePasswordResult.Succeeded)
                        {
                            return new Status { Code = 400, Message = string.Join("; ", removePasswordResult.Errors.Select(x => x.Description)) };
                        }

                        // thêm password mới
                        var addPasswordResult = await _userManager.AddPasswordAsync(existingUser, user.Password);
                        if (!addPasswordResult.Succeeded)
                        {
                            return new Status { Code = 400, Message = string.Join("; ", addPasswordResult.Errors.Select(x => x.Description)) };
                        }
                    }
                }

                // ==========================================
                // 6. CẬP NHẬT ROLE ĐỘNG (BỔ SUNG)
                // ==========================================
                if (!string.IsNullOrWhiteSpace(user.RoleName))
                {
                    var newRoleName = user.RoleName.Trim().ToUpper();

                    // Lấy danh sách các quyền hiện tại của user này
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);

                    // Nếu user chưa sở hữu quyền này thì mới tiến hành cập nhật
                    if (!currentRoles.Contains(newRoleName))
                    {
                        // Xóa các quyền cũ (Giả định mỗi user chỉ có 1 quyền duy nhất tại 1 thời điểm)
                        if (currentRoles.Any())
                        {
                            var removeRolesResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                            if (!removeRolesResult.Succeeded)
                            {
                                return new Status { Code = 400, Message = "Lỗi khi xóa quyền cũ: " + string.Join("; ", removeRolesResult.Errors.Select(x => x.Description)) };
                            }
                        }

                        // Kiểm tra Role mới đã tồn tại trong DB chưa, chưa có thì tạo
                        if (!await _roleManager.RoleExistsAsync(newRoleName))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(newRoleName));
                        }

                        // Gán quyền mới cho user
                        var addRoleResult = await _userManager.AddToRoleAsync(existingUser, newRoleName);
                        if (!addRoleResult.Succeeded)
                        {
                            return new Status { Code = 400, Message = "Lỗi khi gán quyền mới: " + string.Join("; ", addRoleResult.Errors.Select(x => x.Description)) };
                        }
                    }
                }
                // ==========================================

                return new Status { Code = 200, Message = "Cập nhật thành công" };
            }
            catch (Exception ex)
            {
                return new Status { Code = 500, Message = ex.Message };
            }
        }
    }
}
