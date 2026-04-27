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

-- Bảng Nhân viên
CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    Position NVARCHAR(100),
    BaseSalary DECIMAL(18, 2) NOT NULL DEFAULT 0,
    CommissionRate DECIMAL(5, 2) NOT NULL DEFAULT 3,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Bảng Chấm công
CREATE TABLE Attendances (
    AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL FOREIGN KEY REFERENCES Employees(EmployeeId),
    AttendanceMonth INT NOT NULL,
    AttendanceYear INT NOT NULL,
    DaysOff INT NOT NULL DEFAULT 0,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Attendance UNIQUE(EmployeeId, AttendanceMonth, AttendanceYear)
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

-- Bảng Lịch hẹn (Appointments) - giữ lại cho tương thích
CREATE TABLE Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    AppointmentDate DATETIME NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Bảng Chi tiết Lịch hẹn
CREATE TABLE AppointmentDetails (
    AppointmentDetailId INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL FOREIGN KEY REFERENCES Appointments(AppointmentId),
    ServiceId INT NOT NULL FOREIGN KEY REFERENCES Services(ServiceId),
    Price DECIMAL(18, 2) NOT NULL
);

-- Bảng Hóa đơn
CREATE TABLE Invoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    DiscountAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    FinalAmount DECIMAL(18, 2) NOT NULL,
    EarnedPoints INT NOT NULL DEFAULT 0,
    PointsUsed INT NOT NULL DEFAULT 0,
    PaymentMethod VARCHAR(50) DEFAULT N'Tiền mặt',
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(MAX)
);

-- Bảng Chi tiết hóa đơn
CREATE TABLE InvoiceDetails (
    InvoiceDetailId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(InvoiceId),
    ServiceId INT NOT NULL FOREIGN KEY REFERENCES Services(ServiceId),
    EmployeeId INT NOT NULL FOREIGN KEY REFERENCES Employees(EmployeeId),
    Price DECIMAL(18, 2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1
);

-- Bảng Lịch sử tích điểm / tiêu điểm
CREATE TABLE PointHistories (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    InvoiceId INT NULL FOREIGN KEY REFERENCES Invoices(InvoiceId),
    PointsChanged INT NOT NULL,
    Description NVARCHAR(255),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Thêm tài khoản Admin mặc định
IF NOT EXISTS (SELECT * FROM Users WHERE Role = 'Admin')
BEGIN
    INSERT INTO Users (FullName, PhoneNumber, PasswordHash, Role, Points, IsActive, CreatedAt)
    VALUES (N'Quản trị viên', 'admin', 'admin123', 'Admin', 0, 1, GETDATE());
END
GO

-- Bảng Chi phí khác
CREATE TABLE Expenses (
    ExpenseId INT IDENTITY(1,1) PRIMARY KEY,
    ExpenseName NVARCHAR(200) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    Month INT NOT NULL,
    Year INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO
