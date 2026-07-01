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
        await SeedPropertyCategoriesAsync(context);
        await SeedPropertyTypesAsync(context);
        await SeedAreasAsync(context);
        await SeedPermissionsAsync(context);
        await SeedPostingPermissionsAsync(context);
        await SeedNotificationPermissionsAsync(context);
        await SeedAdminUserAsync(context);
        await SeedCrawlSourcesAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RealSyncDbContext context)
    {
        var roles = new[]
        {
            new Role { Name = "Admin", Description = "Quản trị viên hệ thống" },
            new Role { Name = "Manager", Description = "Quản lý" },
            new Role { Name = "Agent", Description = "Nhân viên kinh doanh" },
            new Role { Name = "Viewer", Description = "Người xem" },
            new Role { Name = "Marketing", Description = "Nhân viên marketing" },
        };

        foreach (var role in roles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == role.Name))
            {
                context.Roles.Add(role);
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedPropertyTypesAsync(RealSyncDbContext context)
    {
        if (await context.PropertyTypes.AnyAsync()) return;

        var types = new[]
        {
            new PropertyType { Name = "Đất nền", Slug = "dat-nen", Description = "Đất nền dự án, đất thổ cư", SortOrder = 1 },
            new PropertyType { Name = "Nhà phố", Slug = "nha-pho", Description = "Nhà phố, nhà mặt tiền", SortOrder = 2 },
            new PropertyType { Name = "Căn hộ", Slug = "can-ho", Description = "Căn hộ chung cư", SortOrder = 3 },
            new PropertyType { Name = "Biệt thự", Slug = "biet-thu", Description = "Biệt thự, villa", SortOrder = 4 },
            new PropertyType { Name = "Nhà riêng", Slug = "nha-rieng", Description = "Nhà riêng, nhà trong hẻm", SortOrder = 5 },
            new PropertyType { Name = "Shophouse", Slug = "shophouse", Description = "Nhà phố thương mại", SortOrder = 6 },
            new PropertyType { Name = "Penthouse", Slug = "penthouse", Description = "Căn hộ penthouse", SortOrder = 7 },
            new PropertyType { Name = "Đất nông nghiệp", Slug = "dat-nong-nghiep", Description = "Đất nông nghiệp, đất trồng cây", SortOrder = 8 },
        };

        context.PropertyTypes.AddRange(types);
    }

    private static async Task SeedPropertyCategoriesAsync(RealSyncDbContext context)
    {
        if (await context.PropertyCategories.AnyAsync()) return;

        var categories = new[]
        {
            new PropertyCategory { Name = "Nhà đất bán", Slug = "nha-dat-ban", Description = "Bất động sản đang chào bán" },
            new PropertyCategory { Name = "Nhà đất cho thuê", Slug = "nha-dat-cho-thue", Description = "Bất động sản đang cho thuê" },
            new PropertyCategory { Name = "Dự án", Slug = "du-an", Description = "Sản phẩm thuộc dự án bất động sản" },
        };

        context.PropertyCategories.AddRange(categories);
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

    private static async Task SeedPermissionsAsync(RealSyncDbContext context)
    {
        if (await context.Permissions.AnyAsync()) return;

        // Tạo permissions
        var permissions = new List<Permission>
        {
            // Properties
            new() { Name = "properties.read", Group = "properties", Description = "Xem bất động sản" },
            new() { Name = "properties.create", Group = "properties", Description = "Tạo bất động sản" },
            new() { Name = "properties.update", Group = "properties", Description = "Cập nhật bất động sản" },
            new() { Name = "properties.delete", Group = "properties", Description = "Xóa bất động sản" },
            // Leads
            new() { Name = "leads.read", Group = "leads", Description = "Xem khách hàng tiềm năng" },
            new() { Name = "leads.create", Group = "leads", Description = "Tạo khách hàng tiềm năng" },
            new() { Name = "leads.update", Group = "leads", Description = "Cập nhật khách hàng tiềm năng" },
            new() { Name = "leads.delete", Group = "leads", Description = "Xóa khách hàng tiềm năng" },
            new() { Name = "leads.assign", Group = "leads", Description = "Phân công lead" },
            // Customers
            new() { Name = "customers.read", Group = "customers", Description = "Xem khách hàng" },
            new() { Name = "customers.create", Group = "customers", Description = "Tạo khách hàng" },
            new() { Name = "customers.update", Group = "customers", Description = "Cập nhật khách hàng" },
            new() { Name = "customers.delete", Group = "customers", Description = "Xóa khách hàng" },
            // Users
            new() { Name = "users.read", Group = "users", Description = "Xem người dùng" },
            new() { Name = "users.create", Group = "users", Description = "Tạo người dùng" },
            new() { Name = "users.update", Group = "users", Description = "Cập nhật người dùng" },
            new() { Name = "users.delete", Group = "users", Description = "Xóa người dùng" },
            // Dashboard
            new() { Name = "dashboard.view", Group = "dashboard", Description = "Xem dashboard" },
            new() { Name = "dashboard.analytics", Group = "dashboard", Description = "Xem analytics" },
            // System
            new() { Name = "system.settings", Group = "system", Description = "Quản lý cài đặt hệ thống" },
            new() { Name = "system.logs", Group = "system", Description = "Xem activity logs" },
            new() { Name = "system.notifications", Group = "system", Description = "Quản lý thông báo" },
        };

        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        // Lấy roles
        var admin = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var manager = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
        var agent = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Agent");
        var viewer = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Viewer");

        if (admin == null || manager == null || agent == null || viewer == null) return;

        var allPermissions = await context.Permissions.ToListAsync();

        // Admin: tất cả quyền
        foreach (var perm in allPermissions)
            context.RolePermissions.Add(new RolePermission { RoleId = admin.Id, PermissionId = perm.Id });

        // Manager: phần lớn quyền (trừ system.settings, users.delete)
        var managerExclude = new[] { "system.settings", "users.delete" };
        foreach (var perm in allPermissions.Where(p => !managerExclude.Contains(p.Name)))
            context.RolePermissions.Add(new RolePermission { RoleId = manager.Id, PermissionId = perm.Id });

        // Agent: CRUD properties/leads/customers + dashboard view
        var agentPermNames = new[]
        {
            "properties.read", "properties.create", "properties.update",
            "leads.read", "leads.create", "leads.update",
            "customers.read", "customers.create", "customers.update",
            "dashboard.view"
        };
        foreach (var perm in allPermissions.Where(p => agentPermNames.Contains(p.Name)))
            context.RolePermissions.Add(new RolePermission { RoleId = agent.Id, PermissionId = perm.Id });

        // Viewer: chỉ read
        var viewerPermNames = new[] { "properties.read", "leads.read", "customers.read", "dashboard.view" };
        foreach (var perm in allPermissions.Where(p => viewerPermNames.Contains(p.Name)))
            context.RolePermissions.Add(new RolePermission { RoleId = viewer.Id, PermissionId = perm.Id });
    }

    private static async Task SeedAdminUserAsync(RealSyncDbContext context)
    {
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
        var agentRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Agent");
        var viewerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Viewer");

        var users = new List<User>();
        if (adminRole != null)
        {
            users.Add(new User
            {
                FullName = "System Admin",
                Email = "admin@realsync.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Phone = "0900000001",
                IsActive = true,
                RoleId = adminRole.Id,
            });
            users.Add(new User
            {
                FullName = "Cường Manager",
                Email = "cuong@realsync.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cuong@123"),
                Phone = "0900000002",
                IsActive = true,
                RoleId = managerRole.Id,
            });
            users.Add(new User
            {
                FullName = "Lộc Agent",
                Email = "loc@realsync.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Loc@123"),
                Phone = "0900000003",
                IsActive = true,
                RoleId = agentRole.Id,
            });
        }
        if (viewerRole != null)
        {
            users.Add(new User
            {
                FullName = "Danh Viewer",
                Email = "danh@realsync.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Danh@123"),
                Phone = "0900000004",
                IsActive = true,
                RoleId = viewerRole.Id,
            });
        }

        var seedEmails = users.Select(seedUser => seedUser.Email).ToArray();
        var existingEmails = await context.Users
            .Where(u => seedEmails.Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync();

        context.Users.AddRange(users.Where(u => !existingEmails.Contains(u.Email)));
    }

    private static async Task SeedPostingPermissionsAsync(RealSyncDbContext context)
    {
        // Chỉ seed nếu chưa có posting permissions
        if (await context.Permissions.AnyAsync(p => p.Group == "posts")) return;

        var postingPermissions = new List<Permission>
        {
            new() { Name = "posts.read", Group = "posts", Description = "Xem bài đăng" },
            new() { Name = "posts.create", Group = "posts", Description = "Tạo bài đăng" },
            new() { Name = "posts.update", Group = "posts", Description = "Cập nhật bài đăng" },
            new() { Name = "posts.delete", Group = "posts", Description = "Xóa bài đăng" },
            new() { Name = "posts.publish", Group = "posts", Description = "Đăng bài lên kênh" },
        };

        context.Permissions.AddRange(postingPermissions);
        await context.SaveChangesAsync();

        // Gán permissions cho roles
        var admin = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var manager = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
        var agent = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Agent");
        var marketing = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Marketing");

        var allPostPerms = await context.Permissions.Where(p => p.Group == "posts").ToListAsync();

        // Admin: tất cả quyền posting
        if (admin != null)
        {
            foreach (var perm in allPostPerms)
                context.RolePermissions.Add(new RolePermission { RoleId = admin.Id, PermissionId = perm.Id });
        }

        // Manager: tất cả quyền posting
        if (manager != null)
        {
            foreach (var perm in allPostPerms)
                context.RolePermissions.Add(new RolePermission { RoleId = manager.Id, PermissionId = perm.Id });
        }

        // Agent (Sales): read, create, update
        if (agent != null)
        {
            var agentPostPerms = new[] { "posts.read", "posts.create", "posts.update" };
            foreach (var perm in allPostPerms.Where(p => agentPostPerms.Contains(p.Name)))
                context.RolePermissions.Add(new RolePermission { RoleId = agent.Id, PermissionId = perm.Id });
        }

        // Marketing: tất cả quyền posting
        if (marketing != null)
        {
            foreach (var perm in allPostPerms)
                context.RolePermissions.Add(new RolePermission { RoleId = marketing.Id, PermissionId = perm.Id });
        }
    }

    private static async Task SeedNotificationPermissionsAsync(RealSyncDbContext context)
    {
        var notificationPermissions = new[]
        {
            new Permission { Name = "notifications.read", Group = "notifications", Description = "Xem thông báo" },
            new Permission { Name = "notifications.update", Group = "notifications", Description = "Cập nhật trạng thái thông báo" },
            new Permission { Name = "notifications.delete", Group = "notifications", Description = "Xóa thông báo" },
        };

        var existingPermissionNames = await context.Permissions
            .Where(p => p.Group == "notifications")
            .Select(p => p.Name)
            .ToListAsync();

        var missingPermissions = notificationPermissions
            .Where(p => !existingPermissionNames.Contains(p.Name))
            .ToList();

        if (missingPermissions.Count > 0)
        {
            context.Permissions.AddRange(missingPermissions);
            await context.SaveChangesAsync();
        }

        var allNotificationPermissions = await context.Permissions
            .Where(p => p.Group == "notifications")
            .ToListAsync();

        var rolePermissions = new Dictionary<string, string[]>
        {
            ["Admin"] = new[] { "notifications.read", "notifications.update", "notifications.delete" },
            ["Manager"] = new[] { "notifications.read", "notifications.update", "notifications.delete" },
            ["Agent"] = new[] { "notifications.read", "notifications.update", "notifications.delete" },
            ["Viewer"] = new[] { "notifications.read" }
        };

        foreach (var (roleName, permissionNames) in rolePermissions)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) continue;

            var permissions = allNotificationPermissions
                .Where(p => permissionNames.Contains(p.Name))
                .ToList();

            foreach (var permission in permissions)
            {
                var exists = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                if (!exists)
                    context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }
    }

    private static async Task SeedCrawlSourcesAsync(RealSyncDbContext context)
    {
        if (await context.CrawlSources.AnyAsync()) return;

        var sources = new[]
        {
            new CrawlSource
            {
                Name = "Batdongsan.com.vn",
                BaseUrl = "https://batdongsan.com.vn",
                Description = "Cổng thông tin bất động sản hàng đầu Việt Nam",
                IsActive = true,
                CronSchedule = "0 2 * * *"
            },
            new CrawlSource
            {
                Name = "ChoTot.com",
                BaseUrl = "https://www.chotot.com",
                Description = "Chuyên mục Bất động sản Chợ Tốt",
                IsActive = true,
                CronSchedule = "0 3 * * *"
            },
            new CrawlSource
            {
                Name = "NhaDat24h.net",
                BaseUrl = "https://nhadat24h.net",
                Description = "Kênh thông tin quảng cáo nhà đất",
                IsActive = true,
                CronSchedule = "0 4 * * *"
            }
        };

        context.CrawlSources.AddRange(sources);
    }
}
