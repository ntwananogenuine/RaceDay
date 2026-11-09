CREATE DATABASE RaceDayDB;
USE RaceDayDB;


CREATE TABLE dbo.Roles (
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    VARCHAR(20) NOT NULL UNIQUE
);

CREATE TABLE dbo.Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    RoleId          INT NOT NULL,
    FullName        VARCHAR(100) NOT NULL,
    Email           VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255) NOT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    ProfilePicture  VARCHAR(255) NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);

CREATE TABLE dbo.Events (
    EventId         INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId     INT NOT NULL,
    Name            VARCHAR(150) NOT NULL,
    Description     VARCHAR(1000) NULL,
    EventDate       DATE NOT NULL,
    Location        VARCHAR(150) NOT NULL,
    EventType       VARCHAR(20) NOT NULL DEFAULT 'Run',
    DistanceKm      DECIMAL(5,2) NULL,
    BannerImage     VARCHAR(255) NULL,
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES dbo.Users(UserId)
);

CREATE TABLE dbo.Categories (
    CategoryId      INT IDENTITY(1,1) PRIMARY KEY,
    EventId         INT NOT NULL,
    Name            VARCHAR(100) NOT NULL,
    DistanceKm      DECIMAL(5,2) NOT NULL,
    MaxParticipants INT NOT NULL DEFAULT 100,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId)
);

CREATE TABLE dbo.Enrolments (
    EnrolmentId     INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId   INT NOT NULL,
    CategoryId      INT NOT NULL,
    EnrolmentDate   DATETIME NOT NULL DEFAULT GETDATE(),
    BibNumber       VARCHAR(10) NOT NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT 'Registered',
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantId, CategoryId),
    CONSTRAINT UQ_Enrolments_Bib_Category UNIQUE (CategoryId, BibNumber)
);

CREATE TABLE dbo.Results (
    ResultId        INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId     INT NOT NULL UNIQUE,
    FinishTime      TIME NULL,
    Position        INT NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES dbo.Enrolments(EnrolmentId)
);

INSERT INTO dbo.Roles (RoleName) VALUES ('Organiser'), ('Participant');

INSERT INTO dbo.Users (RoleId, FullName, Email, PasswordHash)
VALUES
    ((SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Organiser'), 'Brian Nkosi', 'briannkosi@raceday.co.za', '614B1F421A1F52727FF72A13CAC74F56'),
    ((SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Organiser'), 'Emma Lusani Ngema', 'emmalucii@raceday.co.za', '645E2A7B0C1F4D45EF859725386B605D');

INSERT INTO dbo.Users (RoleId, FullName, Email, PasswordHash)
VALUES
    ((SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Participant'), 'Lesedi Phiri', 'lesedi@raceday@gmail.com', '8598500975B68DD9F2616A2B1A471F4E'),
    ((SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Participant'), 'Sinqobile Mmagauta', 'sinqobilemm@gmail.com', '14BC2B3E56370B1FF4B8EFFC5DA13226');

INSERT INTO dbo.Events (OrganiserId, Name, Description, EventDate, Location)
VALUES
    ((SELECT UserId FROM dbo.Users WHERE Email = 'briannkosi@raceday.co.za'), 'Johannesburg City Marathon', 'Annual road race through the city centre', '2026-10-11', 'Johannesburg'),
    ((SELECT UserId FROM dbo.Users WHERE Email = 'emmalucii@raceday.co.za'), 'Soweto Fun Run', 'Community fun run for all ages', '2026-11-02', 'Soweto'),
    ((SELECT UserId FROM dbo.Users WHERE Email = 'emmalucii@raceday.co.za'), 'Cape Town Trail Challenge', 'Off-road trail running event', '2026-09-20', 'Cape Town');

INSERT INTO dbo.Categories (EventId, Name, DistanceKm, MaxParticipants)
VALUES
    ((SELECT EventId FROM dbo.Events WHERE Name = 'Johannesburg City Marathon'), 'Full Marathon', 42.20, 500),
    ((SELECT EventId FROM dbo.Events WHERE Name = 'Johannesburg City Marathon'), 'Half Marathon', 21.10, 500),
    ((SELECT EventId FROM dbo.Events WHERE Name = 'Soweto Fun Run'), '5km Fun Run', 5.00, 300),
    ((SELECT EventId FROM dbo.Events WHERE Name = 'Cape Town Trail Challenge'), '15km Trail', 15.00, 150);

INSERT INTO dbo.Enrolments (ParticipantId, CategoryId, BibNumber, Status)
VALUES
    ((SELECT UserId FROM dbo.Users WHERE Email = 'lesedi@raceday@gmail.com'),
     (SELECT CategoryId FROM dbo.Categories WHERE Name = 'Half Marathon'), 'B001', 'Registered'),
    ((SELECT UserId FROM dbo.Users WHERE Email = 'sinqobilemm@gmail.com'),
     (SELECT CategoryId FROM dbo.Categories WHERE Name = '5km Fun Run'), 'B002', 'Registered'),
    ((SELECT UserId FROM dbo.Users WHERE Email = 'lesedi@raceday@gmail.com'),
     (SELECT CategoryId FROM dbo.Categories WHERE Name = '15km Trail'), 'B003', 'Registered');

INSERT INTO dbo.Results (EnrolmentId, FinishTime, Position, Status)
VALUES
    ((SELECT EnrolmentId FROM dbo.Enrolments WHERE BibNumber = 'B002'), '00:28:45', 1, 'Final');

SELECT * FROM [Roles];
SELECT * FROM [Users];
SELECT * FROM [Events];
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;