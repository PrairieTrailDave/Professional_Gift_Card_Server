-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************

use [Gift]

IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'Users'))
	BEGIN
		DROP  TABLE  Users
	END
GO


IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'UsersInRoles'))
	BEGIN
		DROP  TABLE  UsersInRoles
	END
GO




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'Roles'))
	BEGIN
		DROP  TABLE  Roles
	END
GO



CREATE Table [Roles]
(
    [Rolename]        NVARCHAR (50) NOT NULL,
    [ApplicationName] NVARCHAR (50) NOT NULL
    CONSTRAINT PKRoles PRIMARY KEY (Rolename, ApplicationName)
)
GO

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [Roles] TO [GiftCardApp]
GO

INSERT INTO Roles (Rolename, ApplicationName) Values ('SystemAdministrator', 'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('ClientAdministrator', 'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('Agent',               'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('Merchant',            'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('Clerk',               'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('CustomerSupport',     'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('CardHolder',          'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('Demo',                'Gift');
INSERT INTO Roles (Rolename, ApplicationName) Values ('DemoAgent',           'Gift');



CREATE Table [UsersInRoles]
(
      [Username] NVARCHAR (50) NOT NULL,
      [Rolename] NVARCHAR (50) NOT NULL,
      [ApplicationName] NVARCHAR (50) NOT NULL
        CONSTRAINT PKUsersInRoles PRIMARY KEY (Username, Rolename, ApplicationName)
)
GO

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [UsersInRoles] TO [GiftCardApp]
GO

CREATE Table [Users]
(
    [PKID]               UNIQUEIDENTIFIER PRIMARY KEY,
    [UserName]           NVARCHAR  (255) NOT NULL,
    [ApplicationName]    NVARCHAR  (255) NOT NULL,
    [Email]              NVARCHAR  (128) NOT NULL,
    [Comment]            NVARCHAR (255) NULL,
    [Password]           VARCHAR (128) NOT NULL,
    [PasswordQuestion]   NVARCHAR (255) NULL,
    [PasswordAnswer]     NVARCHAR (255) NULL,
    [IsApproved]         BIT NULL, 
    [LastActivityDate]   DateTime NULL,
    [LastLoginDate]      DateTime NULL,
    [LastPasswordChangedDate] DateTime NULL,
    [CreationDate]       DateTime NULL, 
    [IsOnLine]           BIT NULL,
    [IsLockedOut]        BIT NULL,
    [LastLockedOutDate]  DateTime NULL,
    [FailedPasswordAttemptCount] Integer NULL,
    [FailedPasswordAttemptWindowStart] DateTime NULL,
    [FailedPasswordAnswerAttemptCount] Integer NULL,
    [FailedPasswordAnswerAttemptWindowStart] DateTime NULL
)

-- There is code to ask for the super user log on with the first power up of the system
-- Thus, you don't have to figure out what the encrypted values needed here.
-- create the system administrator log on
--  You will have to create the right passwords for the super user
--  The super user has a two password sequence to enter the system
--INSERT INTO Users (PKID,    UserName, ApplicationName, Email,                   Comment,      Password, IsApproved) 
--           Values (NEWID(), 'daver', 'GiftAndLoyalty', 'dave@prairietrail.com', 'super user', '1', 1);
--INSERT INTO Users (PKID,    UserName, ApplicationName, Email,                   Comment,      Password, IsApproved) 
--           Values (NEWID(), 'secondPassword', 'GiftAndLoyalty', 'dave2@prairietrail.com', 'second password to enter system', '1', 1);

--
--INSERT INTO UsersInRoles (Username, Rolename, ApplicationName) Values ('daver', 'SystemAdministrator', 'GiftAndLoyalty');
--INSERT INTO UsersInRoles (Username, Rolename, ApplicationName) Values ('Demo_User', 'Demo', 'GiftAndLoyalty');

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [Users] TO [GiftCardApp]
GO


