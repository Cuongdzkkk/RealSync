# 🗄 Database Guide — RealSync

> Hướng dẫn thiết kế và quản lý database cho hệ thống RealSync.

---

## 1. Database Engine

- **SQL Server 2019+** (hoặc Azure SQL Database)
- **Collation**: `Vietnamese_CI_AS` (case-insensitive, accent-sensitive)
- **Recovery Model**: Full (production), Simple (development)

---

## 2. Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Database | PascalCase | `RealSyncDb` |
| Table | PascalCase, số nhiều | `Properties`, `Leads` |
| Column | PascalCase | `PropertyName`, `CreatedAt` |
| Primary Key | `Id` (GUID) | `Id uniqueidentifier` |
| Foreign Key | `FK_{Child}_{Parent}` | `FK_Properties_PropertyTypes` |
| Index | `IX_{Table}_{Column}` | `IX_Properties_Status` |
| Unique | `UQ_{Table}_{Column}` | `UQ_Users_Email` |
| Check | `CK_{Table}_{Column}` | `CK_Properties_Price` |

---

## 3. Base Entity Pattern

Mọi entity phải kế thừa từ `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

### Soft Delete Query Filter
```csharp
// Trong DbContext OnModelCreating
modelBuilder.Entity<Property>().HasQueryFilter(e => !e.IsDeleted);
```

---

## 4. Core Tables Schema

### 4.1 Properties (Bất động sản)
```sql
CREATE TABLE Properties (
    Id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PropertyCode        NVARCHAR(50) NOT NULL,          -- Mã BĐS (auto-generated)
    Title               NVARCHAR(500) NOT NULL,         -- Tiêu đề
    Description         NVARCHAR(MAX),                  -- Mô tả chi tiết
    PropertyTypeId      UNIQUEIDENTIFIER NOT NULL,      -- FK → PropertyTypes
    ProjectId           UNIQUEIDENTIFIER NULL,           -- FK → Projects (nullable)
    
    -- Location
    AreaId              UNIQUEIDENTIFIER NOT NULL,       -- FK → Areas
    Address             NVARCHAR(500),                   -- Địa chỉ cụ thể
    Ward                NVARCHAR(100),                   -- Phường/Xã
    District            NVARCHAR(100),                   -- Quận/Huyện
    Province            NVARCHAR(100),                   -- Tỉnh/TP
    Latitude            DECIMAL(10,8),                   -- Vĩ độ
    Longitude           DECIMAL(11,8),                   -- Kinh độ
    
    -- Specs
    Area                DECIMAL(18,2),                   -- Diện tích (m²)
    Price               DECIMAL(18,0),                   -- Giá (VND)
    PriceUnit           NVARCHAR(20) DEFAULT N'VND',    -- Đơn vị giá
    Bedrooms            INT DEFAULT 0,                   -- Số phòng ngủ
    Bathrooms           INT DEFAULT 0,                   -- Số phòng tắm
    Floors              INT DEFAULT 1,                   -- Số tầng
    Direction           NVARCHAR(20),                    -- Hướng nhà
    LegalStatus         NVARCHAR(100),                   -- Pháp lý
    
    -- Status
    Status              NVARCHAR(50) DEFAULT 'Draft',    -- Draft, Active, Sold, Rented, Expired
    ListingType         NVARCHAR(20) DEFAULT 'Sale',     -- Sale, Rent
    FeaturedLevel       INT DEFAULT 0,                   -- 0=Normal, 1=Featured, 2=VIP
    
    -- Source
    SourceType          NVARCHAR(50),                    -- Manual, Crawled, Imported
    SourceUrl           NVARCHAR(1000),                  -- URL nguồn (nếu crawl)
    CrawlJobId          UNIQUEIDENTIFIER NULL,           -- FK → CrawlJobs
    
    -- SEO
    Slug                NVARCHAR(500),                   -- URL slug
    MetaTitle           NVARCHAR(200),
    MetaDescription     NVARCHAR(500),
    
    -- Audit
    CreatedAt           DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2 NULL,
    CreatedBy           NVARCHAR(100),
    UpdatedBy           NVARCHAR(100),
    IsDeleted           BIT DEFAULT 0,
    DeletedAt           DATETIME2 NULL
);
```

### 4.2 Leads (Khách hàng tiềm năng)
```sql
CREATE TABLE Leads (
    Id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName            NVARCHAR(200) NOT NULL,
    Phone               NVARCHAR(20),
    Email               NVARCHAR(200),
    
    -- Classification
    Status              NVARCHAR(50) DEFAULT 'New',      -- New, Contacted, Qualified, Proposal, Won, Lost
    Priority            NVARCHAR(20) DEFAULT 'Normal',   -- Low, Normal, High, Urgent
    Score               INT DEFAULT 0,                   -- Lead score (0-100)
    
    -- Interest
    InterestedPropertyId UNIQUEIDENTIFIER NULL,           -- FK → Properties
    Budget              DECIMAL(18,0),                   -- Ngân sách
    Requirements        NVARCHAR(MAX),                   -- Yêu cầu
    PreferredArea       NVARCHAR(200),                   -- Khu vực mong muốn
    PreferredType       NVARCHAR(100),                   -- Loại BĐS mong muốn
    
    -- Assignment
    AssignedToId        UNIQUEIDENTIFIER NULL,            -- FK → Users
    SourceChannel       NVARCHAR(100),                   -- Website, Facebook, Zalo, Phone, Referral
    
    -- Dates
    LastContactedAt     DATETIME2 NULL,
    NextFollowUpAt      DATETIME2 NULL,
    ConvertedAt         DATETIME2 NULL,
    
    -- Audit
    CreatedAt           DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2 NULL,
    CreatedBy           NVARCHAR(100),
    UpdatedBy           NVARCHAR(100),
    IsDeleted           BIT DEFAULT 0,
    DeletedAt           DATETIME2 NULL
);
```

### 4.3 CrawlJobs (Job crawl)
```sql
CREATE TABLE CrawlJobs (
    Id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CrawlSourceId       UNIQUEIDENTIFIER NOT NULL,       -- FK → CrawlSources
    
    Status              NVARCHAR(50) DEFAULT 'Pending',  -- Pending, Running, Completed, Failed
    StartedAt           DATETIME2 NULL,
    CompletedAt         DATETIME2 NULL,
    
    TotalPages          INT DEFAULT 0,
    TotalItems          INT DEFAULT 0,
    SuccessCount        INT DEFAULT 0,
    ErrorCount          INT DEFAULT 0,
    DuplicateCount      INT DEFAULT 0,
    
    ErrorMessage        NVARCHAR(MAX),
    ExecutionLog        NVARCHAR(MAX),
    
    -- Audit
    CreatedAt           DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy           NVARCHAR(100)
);
```

---

## 5. Relationships

```
PropertyTypes  1 ──── N  Properties
Projects       1 ──── N  Properties
Areas          1 ──── N  Properties
Properties     1 ──── N  PropertyImages
Leads          1 ──── N  LeadActivities
Users          1 ──── N  Leads (AssignedTo)
CrawlSources   1 ──── N  CrawlJobs
CrawlJobs      1 ──── N  CrawlResults
CrawlJobs      1 ──── N  Properties (SourceJobId)
Users          1 ──── N  Posts (AuthorId)
Properties     1 ──── N  Posts (PropertyId, nullable)
Posts          1 ──── N  PostChannels
Posts          1 ──── 1  PostAnalytics
Posts          1 ──── N  PostSchedules
Posts          1 ──── N  AIContentGenerations
```

---

## 6. Indexes

```sql
-- Properties
CREATE INDEX IX_Properties_Status ON Properties(Status) WHERE IsDeleted = 0;
CREATE INDEX IX_Properties_PropertyTypeId ON Properties(PropertyTypeId);
CREATE INDEX IX_Properties_AreaId ON Properties(AreaId);
CREATE INDEX IX_Properties_Price ON Properties(Price) WHERE IsDeleted = 0;
CREATE INDEX IX_Properties_ListingType ON Properties(ListingType) WHERE IsDeleted = 0;
CREATE INDEX IX_Properties_CreatedAt ON Properties(CreatedAt DESC);
CREATE UNIQUE INDEX UQ_Properties_PropertyCode ON Properties(PropertyCode);
CREATE UNIQUE INDEX UQ_Properties_Slug ON Properties(Slug) WHERE Slug IS NOT NULL;

-- Leads
CREATE INDEX IX_Leads_Status ON Leads(Status) WHERE IsDeleted = 0;
CREATE INDEX IX_Leads_AssignedToId ON Leads(AssignedToId);
CREATE INDEX IX_Leads_CreatedAt ON Leads(CreatedAt DESC);
CREATE INDEX IX_Leads_Score ON Leads(Score DESC) WHERE IsDeleted = 0;

-- CrawlJobs
CREATE INDEX IX_CrawlJobs_Status ON CrawlJobs(Status);
CREATE INDEX IX_CrawlJobs_CrawlSourceId ON CrawlJobs(CrawlSourceId);
```

---

## 7. EF Core Configuration Example

```csharp
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.PropertyCode)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,0)");
            
        builder.Property(e => e.Area)
            .HasColumnType("decimal(18,2)");
        
        builder.HasQueryFilter(e => !e.IsDeleted);
        
        builder.HasIndex(e => e.PropertyCode).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        
        builder.HasOne(e => e.PropertyType)
            .WithMany(pt => pt.Properties)
            .HasForeignKey(e => e.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## 8. Migration Commands

```bash
# Tạo migration mới
dotnet ef migrations add <MigrationName> -p RealSync.Data -s RealSync.Api

# Apply migration
dotnet ef database update -p RealSync.Data -s RealSync.Api

# Rollback
dotnet ef database update <PreviousMigrationName> -p RealSync.Data -s RealSync.Api

# Script migration (cho production)
dotnet ef migrations script -p RealSync.Data -s RealSync.Api -o migration.sql
```

---

## 9. Performance Tips

1. **Pagination**: Luôn dùng `Skip/Take`, KHÔNG load toàn bộ.
2. **Projection**: Dùng `.Select()` để chỉ lấy cột cần thiết.
3. **AsNoTracking**: Dùng cho read-only queries.
4. **Compiled Queries**: Dùng cho queries chạy thường xuyên.
5. **Batch Operations**: Dùng `ExecuteUpdate/ExecuteDelete` cho bulk operations.
6. **Connection Pooling**: Mặc định EF Core đã có, đảm bảo `MaxPoolSize` đủ.

---

> Xem thêm: `architecture-guide.md`, `api-guide.md`
