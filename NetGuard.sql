CREATE DATABASE NetGuard;


IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
CREATE TABLE Users (UserID int PRIMARY KEY, UserName nvarchar(50), Name nvarchar(100), Phone nvarchar(15),
Email nvarchar (100), Password nvarchar (255), Address nvarchar(255), IsActive bit,UserRole nvarchar(20));
INSERT INTO Users (UserID, UserName, Name, Phone, Email, Password, Address, IsActive, UserRole) VALUES
(1, 'ahmed_tech', 'Ahmed Mansour', '01012345678', 'ahmed@gmail.com', 'P@ssw0rd1', 'Cairo, Egypt', 1, 'Admin'),
(2, 'sara_admin', 'Sara Ibrahim', '01155667788', 'sara.dev@outlook.com', 'Admin!2026', 'Giza, Egypt', 1, 'Employee'),
(3, 'khaled_99', 'Khaled Hassan', '01200001122', 'k.hassan@company.com', 'Kha#99_secure', 'Alex, Egypt', 0, NULL),
(4, 'mona_systems', 'Mona El-Sayed', '01566778899', 'mona.s@webmail.com', 'Mona_Sys*88', 'Mansoura,Egypt', 1, 'Employee');
GO

IF OBJECT_ID('DeviceType', 'U') IS NOT NULL DROP TABLE DeviceType;
CREATE TABLE DeviceType (DeviceTypeID INT PRIMARY KEY, Description VARCHAR(255),
TypeName VARCHAR(100));
INSERT INTO DeviceType (DeviceTypeID, Description, TypeName) VALUES
(1, 'Portable personal computers', 'Laptop'),
(2, 'Network printing and scanning devices', 'Printer'),
(3, 'High-performance computing units', 'Server'),
(4, 'Wireless internet connectivity devices', 'Router');
GO

IF OBJECT_ID('Devices', 'U') IS NOT NULL DROP TABLE Devices;
CREATE TABLE Devices (DeviceID INT PRIMARY KEY, IpAddress VARCHAR(45),           
 DeviceName VARCHAR(100), Status VARCHAR(50), CreateDate DATE, DeviceTypeID_FK INT, UserID_FK INT,

 CONSTRAINT FK_Device_Type FOREIGN KEY (DeviceTypeID_FK) REFERENCES DeviceType(DeviceTypeID),
 CONSTRAINT FK_Device_User FOREIGN KEY (UserID_FK) REFERENCES Users(UserID));

INSERT INTO Devices (DeviceID, IpAddress, DeviceName, Status,
CreateDate, DeviceTypeID_FK, UserID_FK) VALUES
(1, '192.168.1.10', 'Main Server', 'Active', '2024-01-15', 3, 1),      -- سيرفر مسؤول عنه أحمد
(2, '192.168.1.55', 'Office Printer', 'Offline', '2024-02-10', 2, 2),  -- طابعة مسؤولة عنها سارة
(3, '10.0.0.5', 'Security Camera 01', 'Active', '2024-03-01', 3, 1),   -- كاميرا تابعة للسيرفر (أحمد)
(4, '172.16.25.4', 'Reception Laptop', 'Maintenance', '2024-03-04', 1, 4); -- لابتوب مع منى
GO

IF OBJECT_ID('AlertType', 'U') IS NOT NULL DROP TABLE AlertType;
CREATE TABLE AlertType (AlertTypeID INT PRIMARY KEY, TypeName NVARCHAR(50) NOT NULL, Description NVARCHAR(MAX)); 
INSERT INTO AlertType (AlertTypeID, TypeName, Description) VALUES
(1, 'System Check', N'فحص روتيني لحالة الأجهزة'),
(2, 'Performance', N'تحليل سرعة الاستجابة والأداء'),
(3, 'Connectivity', N'رصد حالة الاتصال (Online/Offline)'),
(4, 'Security', N'رصد التهديدات والأنشطة المشبوهة');
GO

IF OBJECT_ID('Severity', 'U') IS NOT NULL DROP TABLE Severity;
CREATE TABLE Severity (SeverityID INT PRIMARY KEY, SeverityLevel INT NOT NULL, SeverityName NVARCHAR(50) NOT NULL, TimeLimit NVARCHAR(50));
INSERT INTO Severity (SeverityID, SeverityLevel, SeverityName, TimeLimit) VALUES
(1, 1, 'Low', '24 Hours'),
(2, 2, 'Medium', '4 Hours'),
(3, 3, 'High', '30 Minutes'),
(4, 4, 'Critical', 'Immediate');
GO

IF OBJECT_ID('Alerts', 'U') IS NOT NULL DROP TABLE Alerts;
CREATE TABLE Alerts (AlertID INT PRIMARY KEY IDENTITY(1,1), 
Message NVARCHAR(MAX) NOT NULL, 
TimeStamp DATETIME DEFAULT GETDATE(), DeviceID_FK INT, AlertTypeID_FK INT, SeverityID_FK INT,

CONSTRAINT FK_Alerts_Devices FOREIGN KEY (DeviceID_FK) REFERENCES Devices(DeviceID),
CONSTRAINT FK_Alerts_AlertType FOREIGN KEY (AlertTypeID_FK) REFERENCES AlertType(AlertTypeID),
CONSTRAINT FK_Alerts_Severity FOREIGN KEY (SeverityID_FK) REFERENCES Severity(SeverityID));

INSERT INTO Alerts (Message, DeviceID_FK, AlertTypeID_FK, SeverityID_FK) VALUES 
(N'تم الاتصال بنجاح بالسيرفر الرئيسي',1,1,1),
(N'تحذير: الطابعة غير متصلة بالشبكة (Offline)',2,1,2),
(N'خطر: ارتفاع درجة حرارة المعالج',3,2,3),
(N'إشعار: لابتوب الاستقبال قيد الصيانة الآن',4,4,4);
GO

SELECT * FROM Users;
SELECT * FROM DeviceType;
SELECT * FROM Devices;
SELECT Name, UserRole, 'Login Successful' AS Status
FROM Users WHERE UserName = 'ahmed_tech' AND Password = 'P@ssw0rd1' AND IsActive = 1;
SELECT * FROM Devices WHERE Status IN ('Active', 'Offline');
SELECT*FROM AlertType;
SELECT*FROM Severity;
SELECT
A.AlertID, A.Message, T.TypeName AS [Alert_Type], S.SeverityName AS [Severity_Level]
FROM Alerts A JOIN AlertType T ON A.AlertTypeID_FK = T.AlertTypeID
JOIN Severity S ON A.SeverityID_FK = S.SeverityID;



-- جزء تجربة
-- التنبيه الأول: ارتفاع حرارة
INSERT INTO Alerts (Message, DeviceID_FK, AlertTypeID_FK, SeverityID_FK, TimeStamp)
VALUES (N'AI Test: ارتفاع مفاجئ في حرارة المعالج', 1, 2, 3, GETDATE());

-- التنبيه الثاني: بطء استجابة) بعدها بدقيقة)
INSERT INTO Alerts (Message, DeviceID_FK, AlertTypeID_FK, SeverityID_FK, TimeStamp)
VALUES (N'AI Test: بطء شديد في استجابة السيرفر', 1, 2, 4, GETDATE());

-- (التنبيه الثالث: محاولات دخول فاشلة)
INSERT INTO Alerts (Message, DeviceID_FK, AlertTypeID_FK, SeverityID_FK, TimeStamp)
VALUES (N'AI Test: محاولات اختراق متكررة مرصودة', 1, 4, 4, GETDATE());
