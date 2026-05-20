using System.Transactions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using yhctapp.Model.Enitity;

namespace yhctapp.Data
{
    public class MyDbcontext : IdentityDbContext<ApplicationUser>
    {
        public MyDbcontext(DbContextOptions<MyDbcontext> options) : base(options)
        {
        }
        #region
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Catogerymenu> Catogerymenus { get; set; }

        public DbSet<Menu> Menus { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }
        // bảng danh mục dịch vụ 



        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // table name
            builder.Entity<Catogerymenu>().ToTable("Catogerymenu");
            builder.Entity<Menu>().ToTable("Menus");
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

          
          


        }
    }
}
