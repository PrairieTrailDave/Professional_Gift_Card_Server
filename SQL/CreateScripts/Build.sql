-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************


USE [Gift]


-- when building this system, you also need to add
-- Security Role: GiftApplications
--  with execute permissions on the stored procedures
--  and access to the tables
-- Database login: GiftCardApp  password AllowMeIn
--
-- Security User: GiftCardApp
--   with Role: GiftApplications

-- Note: with SQL Server 2008, all the permissions were assigned to the role and the user was assigned to that role
--       with SQL Server 2012, all the permissions are assigned to the user



-- have to delete the tables in reverse order because of the 
-- foreign key contraints



IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'History'))
	BEGIN
		DROP  TABLE  History
	END
GO

IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'Cards'))
	BEGIN
		DROP  TABLE  Cards
	END
GO

IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'Merchant'))
	BEGIN
		DROP  TABLE  Merchant
	END
GO

IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'Clerks'))
	BEGIN
		DROP  TABLE  Clerks
	END
GO

IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'Prices'))
	BEGIN
		DROP  TABLE  Prices
	END
GO


IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'MerchantBilling'))
	BEGIN
		DROP  TABLE  MerchantBilling
	END
GO

IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'CardHolders'))
	BEGIN
		DROP  TABLE  CardHolders
	END
GO

IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'U') AND (name = 'GiftSystem'))
	BEGIN
		DROP  TABLE  GiftSystem
	END
GO



-- This has to be done manually
-- 
-- IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'GiftCardApp')
-- DROP USER [GiftCardApp]
-- GO
-- 
-- 
-- DECLARE @RoleName sysname
-- set @RoleName = N'GiftApplications'
-- IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @RoleName AND type = 'R')
-- Begin
-- 	DECLARE @RoleMemberName sysname
-- 	DECLARE Member_Cursor CURSOR FOR
-- 	select [name]
-- 	from sys.database_principals 
-- 	where principal_id in ( 
-- 		select member_principal_id 
-- 		from sys.database_role_members 
-- 		where role_principal_id in (
-- 			select principal_id
-- 			FROM sys.database_principals where [name] = @RoleName  AND type = 'R' ))
-- 
-- 	OPEN Member_Cursor;
-- 
-- 	FETCH NEXT FROM Member_Cursor
-- 	into @RoleMemberName
-- 
-- 	WHILE @@FETCH_STATUS = 0
-- 	BEGIN
-- 
-- 		exec sp_droprolemember @rolename=@RoleName, @membername= @RoleMemberName
-- 
-- 		FETCH NEXT FROM Member_Cursor
-- 		into @RoleMemberName
-- 	END;
-- 
-- 	CLOSE Member_Cursor;
-- 	DEALLOCATE Member_Cursor;
-- End
-- 
-- GO
-- 
-- IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'GiftApplications' AND type = 'R')
-- DROP ROLE [GiftApplications]
-- GO
-- 
-- CREATE ROLE [GiftApplications] AUTHORIZATION [dbo]
-- GO
-- 
-- 
-- 
-- 
-- 
-- 
-- CREATE USER [GiftCardApp] FOR LOGIN [GiftCardApp] WITH DEFAULT_SCHEMA=[dbo]
-- GO
-- 
-- EXEC sp_addrolemember 'GiftApplications', 'GiftCardApp'




-- This table holds some system wide constants

CREATE Table [GiftSystem]
(
    [ID]  INT  NOT NULL IDENTITY (1,1) 
    PRIMARY KEY (ID),
    [ConstantKey]  VARCHAR (15) NOT NULL,
    [ConstantValue]  VARCHAR (100) NOT NULL
) ON [PRIMARY]
GO

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [GiftSystem] TO [GiftCardApp]
GO

    -- set if the cards have a LUHN10 check digit on them
INSERT INTO GiftSystem( ConstantKey, ConstantValue) VALUES( 'LUHN10', 'FALSE');

    -- set the leading digits on all the cards
INSERT INTO GiftSystem( ConstantKey, ConstantValue) VALUES( 'ABA', '');

    -- set the system time zone
INSERT INTO GiftSystem( ConstantKey, ConstantValue) VALUES( 'TimeZone', 'Central');






-- See the MSUser.SQL file for the user and roles & stored procedures needed to manage
-- the MSSQL compatible users, roles, & passwords












CREATE Table [CardHolders]
(
    [ID]  INT IDENTITY (1,1) NOT NULL,
    [CardHolderGUID]     UNIQUEIDENTIFIER NOT NULL
    PRIMARY KEY (CardHolderGUID),
    [EncryptedFirstName] NVARCHAR (100) NOT NULL,
    [LastName]           NVARCHAR (30) NOT NULL,
    [EncryptedLastName]  NVARCHAR (100) NOT NULL,
    [EncryptedCardHolderName] NVARCHAR (150) NOT NULL, -- this is a construct from the first name, last name to be unique on the system
    [EncryptedAddress1]  NVARCHAR (150) NULL,
    [EncryptedAddress2]  NVARCHAR (150) NULL,
    [EncryptedCity]      NVARCHAR (150) NOT NULL,
    [State]              NCHAR (2) NOT NULL,
    [EncryptedPostalCode] VARCHAR (32) NOT NULL,
    [Country]            NVARCHAR (50) NULL,
    [EncryptedPhone]     VARCHAR (75) NOT NULL,                -- phone number must be unique on system
    [EncryptedEmail]     VARCHAR (400) NOT NULL,               -- email must be unique on the system
    [Card1]              UNIQUEIDENTIFIER  NULL,
    [Card2]              UNIQUEIDENTIFIER  NULL,
    [Card3]              UNIQUEIDENTIFIER  NULL,
    [Card4]              UNIQUEIDENTIFIER  NULL,
    [Card5]              UNIQUEIDENTIFIER  NULL
) ON [PRIMARY]
GO
Create UNIQUE INDEX eMailSearch ON CardHolders (EncryptedEmail)
GO
Create UNIQUE INDEX PhoneSearch ON CardHolders (EncryptedPhone)
GO

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [CardHolders] TO [GiftCardApp]
GO





CREATE Table [MerchantBilling] -- this is actually the merchant billing information 
-- all entries in this file must be encrypted
(
    [ID]  INT IDENTITY (1,1) NOT NULL,
    [MerchantBillingGUID]  UNIQUEIDENTIFIER NOT NULL
    PRIMARY KEY (MerchantBillingGUID),
        -- card information for periodic Chargings
    [CardType]             CHAR (2) NULL,
    [EncryptedCardNumber]  VARCHAR(100) NULL,
    [ExpiryDate]           DECIMAL (4,0) NULL,
    [CVV]                  CHAR(32) NULL,
        -- bank account number
    [EncryptedRouting]     VARCHAR(100) NULL,
    [EncryptedAccount]     VARCHAR(100) NULL
) ON [PRIMARY]
GO
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [MerchantBilling] TO [GiftCardApp]
GO




-- this table allows us to have different pricings per card or per transaction
-- for different merchants


-- This table holds the data used in invoicing a merchant or a chain.
-- When a merchant is part of a chain, then the chain rates are used
-- Unless the chain allows for individual merchant pricing

CREATE Table [Prices]
(
    [ID]  INT IDENTITY (1,1) NOT NULL,
    [PriceGUID]                   UNIQUEIDENTIFIER NOT NULL
    PRIMARY KEY (PriceGUID),
    [PricingName]                 NVARCHAR(50)    NOT NULL,
    [CardPrice]                   DECIMAL (16,2)  NOT NULL,
    [TransactionPrice]            DECIMAL (16,2)  NOT NULL,
    [SupportTransactionPrice]     DECIMAL (16,2)  NOT NULL,
    [GiftMonthlyFee]              DECIMAL (16,2)  NOT NULL,
    [CardholderMonthlyFee]        DECIMAL (16,2)  NULL,
    [CardHolderPercentageCharge]  INT  NULL,
    [AfterXMonths]                INT  NULL
) ON [PRIMARY]
GO
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [Prices] TO [GiftCardApp]
GO





CREATE Table [Clerks]
(
    [ID]  INT IDENTITY (1,1) NOT NULL
    PRIMARY KEY (ID),
    [MerchantGUID]  UNIQUEIDENTIFIER NOT NULL,               -- which merchant this clerk works for
    [ClerkID]        NVARCHAR (10) NOT NULL,     -- what the clerk uses to sign on with and listed on reports/transactions
    [UserName]       NVARCHAR (50) NOT NULL,     -- used to manage the permissions
    [Whom]           NVARCHAR (40) NULL          -- some way to identify this person for maintenance
) ON [PRIMARY]
GO
-- this will keep us from being able to add multiple copies
Create UNIQUE INDEX BothofEm ON Clerks (MerchantGUID, ClerkID)
GO
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [Clerks] TO [GiftCardApp]
GO







CREATE Table [Merchant]
(
    [ID]  INT IDENTITY (1,1) NOT NULL,
    [MerchantGUID]         UNIQUEIDENTIFIER NOT NULL
    PRIMARY KEY (MerchantGUID),
    [MerchantID]           VARCHAR (46) NOT NULL ,             -- this field is always upper case to allow for terminals to run transactions
    [MerchantName]         NVARCHAR (50) NOT NULL,
    [MerchantUserName]     NVARCHAR (50) NULL,
    [Address1]             NVARCHAR (50) NOT NULL,
    [Address2]             NVARCHAR (50) NULL,
    [City]                 NVARCHAR (50) NOT NULL,
    [State]                NCHAR (2) NOT NULL,
    [Country]              NVARCHAR (50) NULL,
    [PostalCode]           VARCHAR (10) NOT NULL,
    [Phone]                VARCHAR (25) NOT NULL,
    [ContactPerson]        NVARCHAR (50) NULL,
    [ContactPhone]         VARCHAR (25) NULL,
    [email]                VARCHAR (50) NULL,
    [TimeZone]             VARCHAR (40) NOT NULL,   -- too many variations to have a fixed hour offset
    [LastSeqNumber]        BIGINT NOT NULL,         -- contains the last seq number successful for this merchant
                                                    -- this is used for dialup transactions to prevent duplicates
    [LastShiftClose]       DATETIME NULL,           -- for those merchants that want shift information
    [GiftActive]           CHAR (1) NOT NULL,       -- values are 'A' and 'N'
    [SplitTender]          CHAR (1) NULL,           -- if the merchant supports split tender on gift 'Y'/'N'
    [Restaurant]           CHAR (1) NULL,           -- if the merchant is a restaurant or retail 'Y' - restaurant 'N' or null - retail

--            Billing Information

    [BillingCycle]          NCHAR (1),              -- flag indicating billing cycle - M/Q/Y
    [LastBillingDate]       DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [LastBillingNumber]     BIGINT DEFAULT 0 NOT NULL,       -- what the seq # was at the last invoicing
    [BillingInfo]           UNIQUEIDENTIFIER NULL,
    FOREIGN KEY ( BillingInfo ) REFERENCES MerchantBilling ( MerchantBillingGUID ),

    [PaidUpTo]              DATE NULL,                             -- date that this merchant is paid up to
    [EncryptedTaxID]        VARCHAR(70) NULL,                      -- tax identification
    [PricingCol]            UNIQUEIDENTIFIER NULL,                 -- pricing used for this merchant
    FOREIGN KEY ( PricingCol ) REFERENCES Prices ( PriceGUID ),

    [CurrencyCode]          NCHAR(5) NULL,                         -- which currency symbol to put on screens / receipts
                                                               -- note: this system does not convert between currencies

    [AskForClerkServer]     INT DEFAULT 0 NOT NULL,
    [ShippingAddressLine1]  NVARCHAR (40) NULL,
    [ShippingAddressLine2]  NVARCHAR (40) NULL,
    [ShippingAddressLine3]  NVARCHAR (40) NULL,
    [ShippingAddressLine4]  NVARCHAR (40) NULL,

    [ReceiptHeaderLine1]    NVARCHAR (40) NULL,
    [ReceiptHeaderLine2]    NVARCHAR (40) NULL,
    [ReceiptHeaderLine3]    NVARCHAR (40) NULL,
    [ReceiptHeaderLine4]    NVARCHAR (40) NULL,
    [ReceiptHeaderLine5]    NVARCHAR (40) NULL,
    [ReceiptFooterLine1]    NVARCHAR (40) NULL,
    [ReceiptFooterLine2]    NVARCHAR (40) NULL,
    [BackgroundImageName]   NVARCHAR (50) NULL,                -- what image to put on loyalty pages
    [ActivateAmount1]       INT NULL,                          -- values to put on the gift activate screen
    [ActivateAmount2]       INT NULL,
    [ActivateAmount3]       INT NULL,
    [ActivateAmount4]       INT NULL,
    [ActivateAmount5]       INT NULL,
    [ActivateAmount6]       INT NULL,
    [ActivateAmount7]       INT NULL,
    [ActivateAmount8]       INT NULL,
    [ActivateAmount9]       INT NULL,
    [ActivateAmount10]      INT NULL
) ON [PRIMARY]
GO
Create INDEX Merchant_index ON Merchant (MerchantID)
GO
GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [Merchant] TO [GiftCardApp]
GO






 -- insert a base merchant that holds some system information


INSERT INTO Merchant( MerchantGUID, 
 MerchantID,
 MerchantName,
 Address1,
 City,
 State,
 PostalCode,
 Phone,
 LastSeqNumber,
 TimeZone,
 GiftActive,
 AskForClerkServer
)
 VALUES( NEWID(), 
 'INTERNAL',
 'Prairie Trail Software, Inc.',
 '3821 Beaumont Lane',
 'Plano',
 'TX',
 '750231018',
 '9726184199',
 0,                      -- last sequence number
 'Central',              -- time zone 
 'A',               -- active
 0
);







-- The merchant ID can be null because the card has not been shipped anywhere. 
-- The card number is encrypted before storing in the database

CREATE Table [Cards]
(
    [ID]  INT IDENTITY (1,1) NOT NULL,
    [CardGUID]         UNIQUEIDENTIFIER NOT NULL            -- used for links back to this record
    PRIMARY KEY (CardGUID),
    [CardNumber]              VARCHAR (70) NOT NULL,        -- can also be the cell phone number (encrypted)
    [CardNumLast4]            CHAR (4),                     -- to have a printable value for the card number
    [MerchantGUID]            UNIQUEIDENTIFIER NULL,        -- which merchant this card shipped to / activated at 

    [CardHolderGUID]          UNIQUEIDENTIFIER NULL,        -- who holds this card
    FOREIGN KEY ( CardholderGUID ) REFERENCES CardHolders ( CardHolderGUID ),-- allow for no cardholder info

    [Shipped]                 CHAR NOT NULL DEFAULT 'N',    -- values 'Y', 'N'
    [Activated]               CHAR NOT NULL DEFAULT 'N',    -- values 'Y', 'N'
    [GiftBalance]             DECIMAL (16,2) NOT NULL,
    [LoyaltyBalance]          INT NOT NULL DEFAULT 0,
    [DateShipped]             DATETIME NULL,
    [DateActivated]           DATETIME NULL
) ON [PRIMARY]
GO
-- we search most often on the card number
Create INDEX card_index ON Cards (CardNumber)
GO
-- to allow a merchant to get reports on just his cards
Create INDEX cardmerch_index ON Cards (MerchantGUID)
GO

ALTER TABLE Cards
ADD CONSTRAINT fk_MerchantGUID FOREIGN KEY(MerchantGUID)REFERENCES Merchant(MerchantGUID)
GO 


GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [Cards] TO [GiftCardApp]
GO



-- put a dummy card into the database which will be used in History for merchant close record

INSERT INTO Cards( CardGUID, 
 CardNumber,
 MerchantGUID,
 CardholderGUID,
 Shipped,
 Activated,
 GiftBalance,
 DateShipped)
 VALUES( NewID(),
 '1',                              -- card number
 (select MerchantGUID from Merchant where id = 1),                                -- merchant id
 null,                             -- cardholder
 'N',                              -- shipped
 'N',                              -- activated
 0.00,                             -- amount
 null);                            -- date shipped







CREATE Table [History]
(
    [ID]  BIGINT  NOT NULL IDENTITY (1,1) 
    PRIMARY KEY (ID),
    [WhenHappened]  DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [CardGUID]  UNIQUEIDENTIFIER NOT NULL,
                          -- someone could run an end of day close with no cards on the system
    FOREIGN KEY ( CardGUID ) REFERENCES Cards ( CardGUID ),

    [WhichMerchantGUID]  UNIQUEIDENTIFIER NOT NULL,

    [MerchSeqNumber]  BIGINT NULL,                     --  merchant sequence number for this transaction
                                                       -- why allow NULL? Because web transactions don't have seq nums
    [Clerk]            VARCHAR (10) NULL,
    [WebCellOrDialup]  CHAR (1) NOT NULL DEFAULT 'W',
    [TerminalID]       VARCHAR(10) NULL,
    [LocalTime]        DATETIME NULL,                  -- time reported by the terminal / web
    [TransType]        CHAR (4) NOT NULL,
        -- ADDC - add card
        -- ACTV - gift activate
        -- SALE - gift sell item using this card
        -- GTIP - gift card tip sale using this card
        -- CRED - gift credit to the card
        -- DECC - deactivate
        -- BALN - gift balance inquiry
        -- SHIP - ship card to merchant
        -- TRAN - gift balance transfer
        -- TPUP - gift top up 
        -- DYRP - daily report
        -- DTRP - detail report
        -- CLOS - a daily close
        -- CLSH - close shift (future)
        -- VOID - a void transaction
        -- VDAC - a voided activate
        -- VDSL - a voided sale
        -- VDCR - a voided credit
        -- VDUP - a voided top up
        -- VDSH - a voided shipment
        -- VDEC - a voided deactivate
        -- VDTR - a voided balance transfer
        -- CUHI - a customer history inquiry
        -- CUCR - a customer support initiated credit
        -- CUDB - a customer support initiated debit
        -- DEPL - a depletion action


    [ErrorCode]        CHAR(5) NOT NULL,               -- error code of the transaction
    [TransactionText]  NVARCHAR (40) NULL,             -- info about trans
    [Amount]           DECIMAL (16,2) NOT NULL,        -- total amount of transaction
    [TipAmount]        DECIMAL (16,2) NULL,            -- tip amount
    [TabAmount]        DECIMAL (16,2) NULL,            -- tab amount
    [CardGUID2]        UNIQUEIDENTIFIER NULL,          -- second card for balance transfers
    [InvoiceNumber]    NCHAR (15) NULL,                -- invoice number for this transaction
    [SaleTransaction]  BIGINT NULL                     -- sale transaction for the tip
) ON [PRIMARY]
GO
Create INDEX historymerch_index ON History (WhichMerchantGUID)
GO
-- this requires that MerchantGUID be a primary key to the merchant table
ALTER TABLE [History] 
ADD CONSTRAINT fk_HS_MerchantGUID FOREIGN KEY ( WhichMerchantGUID ) REFERENCES Merchant ( MerchantGUID )
GO
ALTER TABLE [History] 
ADD CONSTRAINT fk_HS_CardGUID FOREIGN KEY ( CardGUID ) REFERENCES Cards ( CardGUID )
GO

GRANT DELETE, INSERT, REFERENCES, SELECT, UPDATE ON [History] TO [GiftCardApp]
GO

