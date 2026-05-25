using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using yhctapp.Data;
using yhctapp.Helpper;
using yhctapp.Interceptors;
using yhctapp.Model.Enitity;
using yhctapp.Services;
using yhctapp.Services.Interface;
using yhctapp.Services.Interface.Role;
using yhctapp.Services.Responsive;

var builder = WebApplication.CreateBuilder(args);

// =======================================================
// 1. CORE SERVICES (Bộ nhớ, Controller, Validation)
// =======================================================
builder.Services.AddMemoryCache(); // Phải khai báo Cache đầu tiên
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errorMessages = actionContext.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var errorResponse = new
        {
            Code = -1,
            Message = errorMessages,
        };
        return new BadRequestObjectResult(errorResponse);
    };
});

// =======================================================
// 2. DATABASE & INTERCEPTORS (LƯU Ý THỨ TỰ Ở ĐÂY)
// =======================================================
// Bắt buộc đăng ký Interceptor TRƯỚC AddDbContext
builder.Services.AddSingleton<CacheInvalidationInterceptor>();

builder.Services.AddDbContext<MyDbcontext>((serviceProvider, options) =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 30,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

    // Lấy Interceptor từ DI Container và gắn vào DbContext
    var cacheInterceptor = serviceProvider.GetRequiredService<CacheInvalidationInterceptor>();
    options.AddInterceptors(cacheInterceptor);
});

// =======================================================
// 3. IDENTITY & AUTHENTICATION (Bảo mật)
// =======================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<MyDbcontext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// =======================================================
// 4. DEPENDENCY INJECTION (Khai báo các Services của bạn)
// =======================================================
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUser, UserResponsive>();
builder.Services.AddScoped<IMenu, MenuResponsive>();
builder.Services.AddScoped<IRole, RoleResponsive>();
builder.Services.AddScoped<IRolePermisson, PermissonResponsive>();
builder.Services.AddScoped<IAuthorization, AuthorizationResponsive>();
builder.Services.AddScoped<IImageService, ImageServiceResponsive>();
builder.Services.AddScoped<IDepartmentRoomRepository, DepartmentRoomResponsive>();
builder.Services.AddScoped<IDocumentRecordRepository, DocumentRecordResponsive>();
// =======================================================
// 5. CORS (Cho phép React/Mobile gọi API)
// =======================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
          policy => policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true) // <--- Thay thế dòng .WithOrigins(...) bằng dòng này
            );
});
// =======================================================
// 6. SWAGGER (Tài liệu API)
// =======================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhap JWT Token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// =======================================================
// 7. HTTP REQUEST PIPELINE (THỨ TỰ MIDDLEWARE BẮT BUỘC)
// =======================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();

// Đặt trước Routing để load hình ảnh không bị chặn (Đã chuyển xuống dưới để phân quyền)
// app.UseStaticFiles(); 

app.UseRouting();

app.UseCors("AllowReact"); // CORS Bắt buộc phải nằm GIỮA Routing và Authentication

app.UseAuthentication(); // Bắt buộc phải nằm TRƯỚC Authorization
app.UseAuthorization();

// Đặt Middleware kiểm tra quyền truy cập file TRƯỚC UseStaticFiles
app.Use(async (context, next) =>
{
    // Kiểm tra nếu request bắt đầu bằng /Uploads
    if (context.Request.Path.StartsWithSegments("/Uploads", StringComparison.OrdinalIgnoreCase))
    {
        // 1. Kiểm tra xem user đã đăng nhập chưa
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized access to file.");
            return; // Dừng pipeline
        }

        // 2. Phân quyền nâng cao: Chỉ khoa mình hoặc Admin mới xem được
        try
        {
            // Lấy DbContext từ DI Container
            var dbContext = context.RequestServices.GetRequiredService<MyDbcontext>();
            
            // Lấy tên file từ đường dẫn URL (ví dụ: guid_tenfile.pdf)
            var fileName = System.IO.Path.GetFileName(context.Request.Path.Value);

            if (!string.IsNullOrEmpty(fileName))
            {
                // Tìm file trong Database kèm thông tin Hồ sơ chứa nó
                var fileRecord = await dbContext.DocumentFiles
                    .Include(f => f.DocumentRecord)
                    .FirstOrDefaultAsync(f => f.FilePath == fileName);

                if (fileRecord != null && fileRecord.DocumentRecord != null)
                {
                    // Lấy mã phòng ban của Hồ sơ
                    var fileDepartmentId = fileRecord.DocumentRecord.Id_DepartmentRoom;
                    
                    // Lấy mã phòng ban của User đang đăng nhập
                    var userDepartmentId = context.User.Claims.FirstOrDefault(c => c.Type == "IdDepartmentRoom")?.Value;
                    
                    // Kiểm tra Role Admin
                    var isAdmin = context.User.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value.ToUpper() == "ADMIN");

                    // Nếu không phải Admin và cũng không cùng phòng ban -> Cấm truy cập
                    if (!isAdmin && !string.Equals(userDepartmentId, fileDepartmentId, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = 403; // Forbidden
                        await context.Response.WriteAsync("Forbidden: Bạn không có quyền xem file của khoa khác.");
                        return; // Dừng pipeline
                    }
                }
            }
        }
        catch (Exception)
        {
            // Nếu có lỗi truy vấn DB, cho phép tiếp tục hoặc log lại (ở đây chọn từ chối an toàn)
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error.");
            return;
        }
    }
    
    // Nếu hợp lệ hoặc không phải /Uploads, tiếp tục pipeline
    await next();
});

// Dùng UseStaticFiles bình thường sau khi đã lọc quyền
app.UseStaticFiles();
app.MapControllers();

app.Run();