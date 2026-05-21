using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Data.Context;

namespace RealSync.Data.Seeders;

/// <summary>
/// Seed data cơ bản cho hệ thống RealSync.
/// Gọi trong Program.cs khi khởi động (Development).
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(RealSyncDbContext context)
    {
        await SeedRolesAsync(context);
        await SeedPropertyTypesAsync(context);
        await SeedAreasAsync(context);
        await SeedAdminUserAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RealSyncDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        var roles = new[]
        {
            new Role { Name = "Admin", Description = "Quản trị viên hệ thống" },
            new Role { Name = "Manager", Description = "Quản lý" },
            new Role { Name = "Agent", Description = "Nhân viên kinh doanh" },
            new Role { Name = "Viewer", Description = "Người xem" },
        };

        context.Roles.AddRange(roles);
    }

    private static async Task SeedPropertyTypesAsync(RealSyncDbContext context)
    {
        if (await context.PropertyTypes.AnyAsync()) return;

        var types = new[]
        {
            new PropertyType { Name = "Đất nền", Description = "Đất nền dự án, đất thổ cư", SortOrder = 1 },
            new PropertyType { Name = "Nhà phố", Description = "Nhà phố, nhà mặt tiền", SortOrder = 2 },
            new PropertyType { Name = "Căn hộ", Description = "Căn hộ chung cư", SortOrder = 3 },
            new PropertyType { Name = "Biệt thự", Description = "Biệt thự, villa", SortOrder = 4 },
            new PropertyType { Name = "Nhà riêng", Description = "Nhà riêng, nhà trong hẻm", SortOrder = 5 },
            new PropertyType { Name = "Shophouse", Description = "Nhà phố thương mại", SortOrder = 6 },
            new PropertyType { Name = "Penthouse", Description = "Căn hộ penthouse", SortOrder = 7 },
            new PropertyType { Name = "Đất nông nghiệp", Description = "Đất nông nghiệp, đất trồng cây", SortOrder = 8 },
        };

        context.PropertyTypes.AddRange(types);
    }

    private static async Task SeedAreasAsync(RealSyncDbContext context)
    {
        if (await context.Areas.AnyAsync()) return;

        // Tỉnh/Thành phố (Level 1)
        var hcm = new Area { Name = "TP. Hồ Chí Minh", Slug = "tp-ho-chi-minh", Level = 1, SortOrder = 1 };
        var hn = new Area { Name = "TP. Hà Nội", Slug = "tp-ha-noi", Level = 1, SortOrder = 2 };
        var dn = new Area { Name = "TP. Đà Nẵng", Slug = "tp-da-nang", Level = 1, SortOrder = 3 };
        var bd = new Area { Name = "Bình Dương", Slug = "binh-duong", Level = 1, SortOrder = 4 };
        var dna = new Area { Name = "Đồng Nai", Slug = "dong-nai", Level = 1, SortOrder = 5 };

        context.Areas.AddRange(hcm, hn, dn, bd, dna);
        await context.SaveChangesAsync();

        // Quận/Huyện mẫu cho TP.HCM (Level 2)
        var hcmDistricts = new[]
        {
            new Area { Name = "Quận 1", Slug = "quan-1", Level = 2, SortOrder = 1, ParentId = hcm.Id },
            new Area { Name = "Quận 2 (TP. Thủ Đức)", Slug = "quan-2", Level = 2, SortOrder = 2, ParentId = hcm.Id },
            new Area { Name = "Quận 3", Slug = "quan-3", Level = 2, SortOrder = 3, ParentId = hcm.Id },
            new Area { Name = "Quận 7", Slug = "quan-7", Level = 2, SortOrder = 4, ParentId = hcm.Id },
            new Area { Name = "Quận 9 (TP. Thủ Đức)", Slug = "quan-9", Level = 2, SortOrder = 5, ParentId = hcm.Id },
            new Area { Name = "Quận Bình Thạnh", Slug = "quan-binh-thanh", Level = 2, SortOrder = 6, ParentId = hcm.Id },
            new Area { Name = "Quận Gò Vấp", Slug = "quan-go-vap", Level = 2, SortOrder = 7, ParentId = hcm.Id },
            new Area { Name = "Quận Tân Bình", Slug = "quan-tan-binh", Level = 2, SortOrder = 8, ParentId = hcm.Id },
        };

        // Quận mẫu cho Hà Nội (Level 2)
        var hnDistricts = new[]
        {
            new Area { Name = "Quận Hoàn Kiếm", Slug = "quan-hoan-kiem", Level = 2, SortOrder = 1, ParentId = hn.Id },
            new Area { Name = "Quận Ba Đình", Slug = "quan-ba-dinh", Level = 2, SortOrder = 2, ParentId = hn.Id },
            new Area { Name = "Quận Cầu Giấy", Slug = "quan-cau-giay", Level = 2, SortOrder = 3, ParentId = hn.Id },
            new Area { Name = "Quận Đống Đa", Slug = "quan-dong-da", Level = 2, SortOrder = 4, ParentId = hn.Id },
            new Area { Name = "Quận Thanh Xuân", Slug = "quan-thanh-xuan", Level = 2, SortOrder = 5, ParentId = hn.Id },
        };

        context.Areas.AddRange(hcmDistricts);
        context.Areas.AddRange(hnDistricts);
    }

    private static async Task SeedAdminUserAsync(RealSyncDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null) return;

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@realsync.vn",
            // BCrypt hash của "Admin@123" — PHẢI đổi password sau khi deploy
            PasswordHash = "$2a$11$placeholder.hash.will.be.replaced.in.production",
            Phone = "0900000000",
            IsActive = true,
            RoleId = adminRole.Id,
        };

        context.Users.Add(admin);
    }
}
