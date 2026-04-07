# Story Web

Ứng dụng web ASP.NET Core MVC để hiển thị truyện và chương truyện.

## Tổng quan

- Nền tảng: ASP.NET Core MVC
- ORM: Entity Framework Core + SQL Server
- Xác thực: ASP.NET Core Identity
- Frontend assets: Bootstrap, jQuery (local trong `wwwroot/assets`)
- Target framework: `net10.0`

## Yêu cầu môi trường

1. .NET SDK 10.x (phù hợp với `TargetFramework=net10.0`)
2. SQL Server (Express/Developer/deployed server)
3. (Tùy chọn) `dotnet-ef` nếu cần chạy migration thủ công

Kiểm tra nhanh:

```bash
dotnet --version
dotnet --list-sdks
```

## Danh sách package cần thiết để chạy

### 1. Top-level package (khai báo trực tiếp trong project)

Theo `story-web.csproj`, dự án đang dùng:

| Package                                             |  Version | Mục đích                                   |
| --------------------------------------------------- | -------: | ------------------------------------------ |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | `10.0.5` | Tích hợp Identity với EF Core              |
| `Microsoft.AspNetCore.Identity.UI`                  | `10.0.5` | UI mặc định cho Identity (Areas/Identity)  |
| `Microsoft.EntityFrameworkCore.SqlServer`           | `10.0.5` | Provider SQL Server cho EF Core            |
| `Microsoft.EntityFrameworkCore.Tools`               | `10.0.5` | Công cụ migration/scaffold (dev-time)      |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design`  | `10.0.2` | Scaffolding code generator cho Identity UI |

Nếu muốn khởi tạo lại tay, có thể chạy:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.5
dotnet add package Microsoft.AspNetCore.Identity.UI --version 10.0.5
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.5
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 10.0.5
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design --version 10.0.2
```

### 2. Framework reference

Dự án dùng `Microsoft.NET.Sdk.Web` nên nhận framework `Microsoft.AspNetCore.App` từ shared framework (không cần add thủ công bằng `dotnet add package`).

### 3. Transitive packages (được restore từ top-level)

Khi chạy `dotnet restore`, các package transitive quan trọng sẽ được kéo về, ví dụ:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Relational`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.Data.SqlClient`
- `Azure.Identity`
- `Azure.Core`

Xem đầy đủ trên máy bạn:

```bash
dotnet list story-web.csproj package --include-transitive
```

## Cấu hình để chạy được

Cập nhật connection string trong `appsettings.json`:

```json
"ConnectionStrings": {
	"DefaultConnection": "Data Source=SERVER_NAME;Initial Catalog=webstorydb;Integrated Security=True;TrustServerCertificate=True"
}
```

Nếu dùng SQL login:

```json
"ConnectionStrings": {
	"DefaultConnection": "Server=SERVER_NAME;Database=webstorydb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
}
```

## Hướng dẫn chạy từ đầu

1. Di chuyển vào thư mục dự án:

```bash
cd c:\ASPNET\story-web
```

2. Restore package:

```bash
dotnet restore
```

3. Build:

```bash
dotnet build
```

4. (Nếu có migration) cập nhật database:

```bash
dotnet ef database update
```

Nếu chưa cài `dotnet-ef`:

```bash
dotnet tool install --global dotnet-ef
```

5. Run:

```bash
dotnet run
```

## URL mặc định

- `https://localhost:5001`
- `http://localhost:5000`

## Cấu trúc chính

- `Program.cs`: cấu hình service MVC, DbContext SQL Server, Identity
- `Controllers/`: controller MVC (HomeController)
- `Areas/Identity/`: scaffolded Identity pages (Login, Register, Manage, etc.)
- `Data/`: DbContext và model định nghĩa
- `Migrations/`: Entity Framework migrations (changelog schema database)
- `Models/`: model dữ liệu (ErrorViewModel, etc.)
- `Views/`: Razor views chính (`Home/`, `Shared/`)
- `Properties/`: cài đặt khởi chạy (launchSettings.json)
- `wwwroot/assets`: CSS/JS/Image static (Bootstrap, jQuery)
- `appsettings.json`: cấu hình logging + connection string
- `appsettings.Development.json`: cấu hình phát triển

## Lệnh hữu ích

```bash
dotnet clean
dotnet restore
dotnet build -c Debug
dotnet run --launch-profile "http"
```

## Xử lý lỗi nhanh

- Lỗi SDK: cài đúng .NET 10 SDK, sau đó chạy lại `dotnet --list-sdks`
- Lỗi kết nối SQL: kiểm tra `DefaultConnection`, tên server, quyền truy cập
- Lỗi migration: đảm bảo có `Microsoft.EntityFrameworkCore.Tools` và `dotnet-ef`
