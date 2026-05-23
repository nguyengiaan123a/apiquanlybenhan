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
              .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
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

app.UseStaticFiles(); // Đặt trước Routing để load hình ảnh không bị chặn

app.UseRouting();

app.UseCors("AllowReact"); // CORS Bắt buộc phải nằm GIỮA Routing và Authentication

app.UseAuthentication(); // Bắt buộc phải nằm TRƯỚC Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();