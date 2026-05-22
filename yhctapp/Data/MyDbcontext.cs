using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Transactions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using yhctapp.Model.Enitity;

namespace yhctapp.Data
{
    public class MyDbcontext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentUserService _currentUserService;

        public MyDbcontext(DbContextOptions<MyDbcontext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }
        #region
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Catogerymenu> Catogerymenus { get; set; }

        public DbSet<Menu> Menus { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }
        // bảng danh mục dịch vụ 
        public DbSet<DepartmentRoom> DepartmentRooms { get; set; }
        // bảng danh mục văn bản 
        public DbSet<DocumentGroup> DocumentGroups { get; set; }
        // bảng hồ sơ
        public DbSet<DocumentRecord> DocumentRecords { get; set; }



        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // table name
            builder.Entity<Catogerymenu>().ToTable("Catogerymenu");
            builder.Entity<Menu>().ToTable("Menus");
            builder.Entity<DepartmentRoom>().ToTable("DepartmentRooms");
            // dịch vụ
           
            // ===== menu cha =====
            builder.Entity<Catogerymenu>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Thumnail).HasMaxLength(200);
                entity.Property(x => x.CreatedDate).HasDefaultValueSql("GETDATE()");

            });

            // ===== menu con =====
            builder.Entity<Menu>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Thumnail).HasMaxLength(300);
                entity.Property(x=>x.Title).HasMaxLength(200);
                entity.Property(x => x.url).HasMaxLength(200);
                entity.Property(x => x.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.HasOne(x => x.Catogerymenu).WithMany(x => x.Menus).HasForeignKey(x => x.Id_menu).OnDelete(DeleteBehavior.Cascade);
            });

            // ===== role permission =====

            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(x => x.Id_RolePermission);
                entity.Property(x => x.Id_RolePermission).ValueGeneratedOnAdd();
                entity.Property(x => x.RoleId).IsRequired();
                entity.HasOne(x => x.Approle).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);

            });

            // ===== DepartmentRoom =====
            builder.Entity<DepartmentRoom>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Room).IsRequired().HasMaxLength(200);
            });
            builder.Entity<ApplicationUser>().HasOne(x => x.DepartmentRoom).WithMany(x => x.ApplicationUsers).HasForeignKey(x => x.IdDepartmentRoom).OnDelete(DeleteBehavior.Cascade);
            // ===== document group =====
            builder.Entity<DocumentGroup>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
                entity.HasOne(x => x.DepartmentRoom).WithMany(x => x.DocumentGroups).HasForeignKey(x => x.Id_DepartmentRoom).OnDelete(DeleteBehavior.Cascade);
            });
            // ===== Global Query Filter cho DocumentGroup =====
            builder.Entity<DocumentGroup>(entity =>
            {
                entity.HasQueryFilter(x => _currentUserService.IsAdmin || x.Id_DepartmentRoom == _currentUserService.DepartmentId);
            });

            // ===== DocumentRecord =====
            builder.Entity<DocumentRecord>(entity =>
            {
                entity.ToTable("DocumentRecords");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Title).IsRequired().HasMaxLength(500);
                entity.Property(x => x.Id_DepartmentRoom).IsRequired().HasMaxLength(200);
                entity.Property(x => x.ViTriLuuTru).HasMaxLength(300);
                entity.Property(x => x.NguoiQuanLy).HasMaxLength(200);
                entity.Property(x => x.TinhTrang).HasMaxLength(100);
                entity.Property(x => x.MucDoBaoMat).HasMaxLength(50);
                entity.Property(x => x.GhiChu).HasMaxLength(1000);
                entity.Property(x => x.CreatedDate).HasDefaultValueSql("GETDATE()");

                // MaHoSo: người dùng tự nhập, lưu vào DB
                entity.Property(x => x.MaHoSo).HasMaxLength(50);

                // Computed properties: EF Core bỏ qua, không map vào DB
                entity.Ignore(x => x.NamHetHan);
                entity.Ignore(x => x.TrangThai);

                // FK → DepartmentRoom
                entity.HasOne(x => x.DepartmentRoom)
                    .WithMany(x => x.DocumentRecords)
                    .HasForeignKey(x => x.Id_DepartmentRoom)
                    .OnDelete(DeleteBehavior.Restrict);

                // FK → DocumentGroup (optional)
                entity.HasOne(x => x.DocumentGroup)
                    .WithMany(x => x.DocumentRecords)
                    .HasForeignKey(x => x.Id_DocumentGroup)
                    .OnDelete(DeleteBehavior.SetNull);
            });


        }
    }
}
