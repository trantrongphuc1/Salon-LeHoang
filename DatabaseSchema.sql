CREATE DATABASE SalonLeHoang;
GO
USE SalonLeHoang;
GO

-- Bảng Người dùng (Quản trị viên và Khách hàng)
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'Customer', -- 'Admin' hoặc 'Customer'
    Points INT NOT NULL DEFAULT 0, -- Điểm thưởng khách hàng
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Bảng Dịch vụ
CREATE TABLE Services (
    ServiceId INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    DurationMinutes INT NOT NULL DEFAULT 30,
    ImageUrl VARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Bảng Lịch hẹn (Appointments)
CREATE TABLE Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    AppointmentDate DATETIME NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Confirmed', 'Completed', 'Cancelled'
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Bảng Chi tiết Lịch hẹn (Một lịch hẹn có thể có nhiều dịch vụ)
CREATE TABLE AppointmentDetails (
    AppointmentDetailId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL FOREIGN KEY REFERENCES Appointments(AppointmentId),
    ServiceId INT NOT NULL FOREIGN KEY REFERENCES Services(ServiceId),
    Price DECIMAL(18, 2) NOT NULL -- Lưu giá trị dịch vụ tại thời điểm đặt
);

-- Bảng Hóa đơn
CREATE TABLE Invoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL FOREIGN KEY REFERENCES Appointments(AppointmentId),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    EarnedPoints INT NOT NULL DEFAULT 0, -- Điểm cộng được từ hóa đơn này (VD: TotalAmount / 10000)
    PaymentMethod VARCHAR(50) DEFAULT 'Cash',
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(MAX)
);

-- Bảng Lịch sử tích điểm / tiêu điểm
CREATE TABLE PointHistories (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    InvoiceId INT NULL FOREIGN KEY REFERENCES Invoices(InvoiceId),
    PointsChanged INT NOT NULL, -- Số điểm thay đổi (+ hoặc -)
    Description NVARCHAR(255),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
