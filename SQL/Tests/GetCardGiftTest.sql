-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************

USE [Gift];
GO

DECLARE @AmountOfSale      DECIMAL (19,2);
DECLARE @MerchantID1       VARCHAR (46);
DECLARE @MerchantID2       VARCHAR (46);
DECLARE @MerchantID3       VARCHAR (46);
DECLARE @MerchantID4       VARCHAR (46);
DECLARE @MerchantID5       VARCHAR (46);
DECLARE @MerchantID6       VARCHAR (46);
DECLARE @Clerk             NVARCHAR (10);
DECLARE @CardNumber        VARCHAR (70);
DECLARE @CardNumber2       VARCHAR (70);
DECLARE @CardNumber3       VARCHAR (70);
DECLARE @CardNumber4       VARCHAR (70);
DECLARE @CardNumber5       VARCHAR (70);
DECLARE @WhereFrom         CHAR;
DECLARE @MerchSeqNum       VARCHAR (20);
DECLARE @PhoneNumber       VARCHAR (20);
DECLARE @SalesDescription  NVARCHAR (40);
DECLARE @MerchantGroup     NCHAR(8);
DECLARE @CardHolder         INT;
DECLARE @CardHolderGUID        UNIQUEIDENTIFIER;

DECLARE @EncryptedPhoneNumber  VARCHAR (70);
DECLARE @MerchantGUID          UNIQUEIDENTIFIER;
DECLARE @ErrorCode             CHAR (5);
DECLARE @EncryptedFirstName    NVARCHAR (100);
DECLARE @EncryptedLastName     NVARCHAR (100);

-- variables holding record IDs

DECLARE @MerchID1          INT;
DECLARE @MerchID2          INT;
DECLARE @MerchID3          INT;
DECLARE @MerchID4          INT;
DECLARE @MerchID5          INT;
DECLARE @MerchID6          INT;
DECLARE @ChainID           INT;
DECLARE @CardGUID          UNIQUEIDENTIFIER;
DECLARE @CardGUID2         UNIQUEIDENTIFIER;
DECLARE @CardGUID3         UNIQUEIDENTIFIER;
DECLARE @CardGUID4         UNIQUEIDENTIFIER;
DECLARE @CardGUID5         UNIQUEIDENTIFIER;
DECLARE @MerchantGUID1     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID2     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID3     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID4     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID5     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID6     UNIQUEIDENTIFIER;

DECLARE @WhichTest         VARCHAR (40);


-- define some test values

SET @MerchantID1 = 'TEST495';
SET @MerchantID2 = 'TEST496';
SET @MerchantID3 = 'TEST497';
SET @MerchantID4 = 'TEST498';
SET @MerchantID5 = 'TEST499';
SET @MerchantID6 = 'TEST500';
SET @Clerk = '';
SET @CardNumber = '1200345';
SET @CardNumber2 = '1200346';
SET @CardNumber3 = '1200347';
SET @CardNumber4 = '1200348';
SET @CardNumber5 = '1200349';
SET @WhereFrom = 'T';  -- testing
SET @MerchSeqNum = '0001';
SET @PhoneNumber = '9726185511';
SET @SalesDescription ='System Test Sale';
SET @MerchantGroup = 'TESTGRP';
SET @AmountOfSale = 1.00;

-- uncomment this when you want to just clean up after a failed est

--GOTO Cleanup

-- Test 1 - merchant not found

SET @WhichTest = 'Test 1';
EXECUTE gp_GetCard @PhoneNumber, @MerchantGUID, @ErrorCode OUTPUT, @CardGUID OUTPUT, @EncryptedFirstName OUTPUT, @EncryptedLastName OUTPUT;
IF @ErrorCode <> 'MERID' GOTO TestFailed

-- need to build the merchants where these transactions happen

SET @MerchantGUID1 = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID,     MerchantName,  Address1, City, State, PostalCode, Phone,       TimeZone, LastSeqNumber, GiftActive, LoyaltyActive, PaidUpTo)
            values (@MerchantID1, @MerchantGUID1, 'Test Merchant 1', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1,            'Y',        'Y', DATEADD(month, 1, SYSDATETIME()));
SET @MerchID1 = Scope_Identity();

SET @MerchantGUID2 = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID,     MerchantName,  Address1, City, State, PostalCode, Phone,       TimeZone, LastSeqNumber, GiftActive, LoyaltyActive, PaidUpTo)
            values (@MerchantID2, @MerchantGUID2, 'Test Merchant 2', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1,            'Y',        'Y', DATEADD(month, 1, SYSDATETIME()));
SET @MerchID2 = Scope_Identity();

SET @MerchantGUID3 = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID,     MerchantName,  Address1, City, State, PostalCode, Phone,       TimeZone, LastSeqNumber, GiftActive, LoyaltyActive, PaidUpTo)
            values (@MerchantID3, @MerchantGUID3, 'Test Merchant 3', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1,            'Y',        'Y', DATEADD(month, 1, SYSDATETIME()));
SET @MerchID3 = Scope_Identity();

SET @MerchantGUID4 = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID,     MerchantName,  Address1, City, State, PostalCode, Phone,       TimeZone, LastSeqNumber, GiftActive, LoyaltyActive, PaidUpTo)
            values (@MerchantID4, @MerchantGUID4, 'Test Merchant 4', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1,            'Y',        'Y', DATEADD(month, 1, SYSDATETIME()));
SET @MerchID4 = Scope_Identity();

SET @MerchantGUID5 = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID,     MerchantName,  Address1, City, State, PostalCode, Phone,       TimeZone, LastSeqNumber, GiftActive, LoyaltyActive, PaidUpTo)
            values (@MerchantID5, @MerchantGUID5, 'Test Merchant 5', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1,            'Y',        'Y', DATEADD(month, 1, SYSDATETIME()));
SET @MerchID5 = Scope_Identity();

SET @MerchantGUID = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID,     MerchantName,  Address1, City, State, PostalCode, Phone,       TimeZone, LastSeqNumber, GiftActive, LoyaltyActive, PaidUpTo)
            values (@MerchantID6, @MerchantGUID, 'Test Merchant 6', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1,            'Y',        'Y', DATEADD(month, 1, SYSDATETIME()));
SET @MerchID6 = Scope_Identity();

-- Test 2 - phone number not found - no cardholder on file

SET @WhichTest = 'Test 2';
EXECUTE gp_GetCard @PhoneNumber, @MerchantGUID, @ErrorCode OUTPUT, @CardGUID OUTPUT, @EncryptedFirstName OUTPUT, @EncryptedLastName OUTPUT;
IF @ErrorCode <> 'NOPHN' GOTO TestFailed



-- Test 3 - phone number not found - no phone number on cardholder

SET @CardHolderGUID = NewID();
INSERT INTO CardHolders (CardHolderGUID, EncryptedFirstName, LastName, EncryptedLastName, EncryptedCardHolderName, EncryptedAddress1, EncryptedCity, State, EncryptedPostalCode, EncryptedPhone, Encryptedemail) 
  VALUES (@CardHolderGUID, 'Test', 'CardHolder', 'CardHolder', 'Test CardHolder', 'Adr1', 'City', 'TX', '75023', @PhoneNumber, 'test@cardholder.fam');
SET @CardHolder=SCOPE_IDENTITY();  -- since not parallel, we can use it


SET @WhichTest = 'Test 3';
EXECUTE gp_GetCard @PhoneNumber, @MerchantGUID, @ErrorCode OUTPUT, @CardGUID OUTPUT, @EncryptedFirstName OUTPUT, @EncryptedLastName OUTPUT;
IF @ErrorCode <> 'CRDER' GOTO TestFailed


-- Test 4 - phone number not found - card number on cardholder not valid for this merchant

SET @CardGUID = NewID();
INSERT INTO Cards (CardGUID, CardNumber, CardNumLast4, GiftBalance, MerchantGUID, Activated)
VALUES (@CardGUID, @CardNumber, '0345', 0.00, @MerchantGUID1, 'N');


UPDATE [CardHolders] SET Card1 = @CardGUID WHERE ID=@CardHolder;
UPDATE [Cards] SET [CardHolderGUID] = @CardHolderGUID WHERE CardNumber = @CardNumber;

SET @WhichTest = 'Test 4';
EXECUTE gp_GetCard @PhoneNumber, @MerchantGUID, @ErrorCode OUTPUT, @CardGUID OUTPUT, @EncryptedFirstName OUTPUT, @EncryptedLastName OUTPUT;
IF @ErrorCode <> 'CRDER' GOTO TestFailed


-- Test 5 - phone number not found - all phone numbers on cardholder not valid for this merchant

SET @CardGUID2 = NewID();
INSERT INTO Cards (CardGUID, CardNumber, CardNumLast4, GiftBalance, MerchantGUID, Activated)
VALUES (@CardGUID2, @CardNumber2, '0346', 0.00, @MerchantGUID2, 'N');

SET @CardGUID3 = NewID();
INSERT INTO Cards (CardGUID, CardNumber, CardNumLast4, GiftBalance, MerchantGUID, Activated)
VALUES (@CardGUID3, @CardNumber3, '0347', 0.00, @MerchantGUID3, 'N');

SET @CardGUID4 = NewID();
INSERT INTO Cards (CardGUID, CardNumber, CardNumLast4, GiftBalance, MerchantGUID, Activated)
VALUES (@CardGUID4, @CardNumber4, '0348', 0.00, @MerchantGUID4, 'N');

SET @CardGUID5 = NewID();
INSERT INTO Cards (CardGUID, CardNumber, CardNumLast4, GiftBalance, MerchantGUID, Activated)
VALUES (@CardGUID5, @CardNumber5, '0349', 0.00, @MerchantGUID5, 'N');

UPDATE [CardHolders] SET Card2 = @CardGUID2,
                         Card3 = @CardGUID3, 
                         Card4 = @CardGUID4, 
                         Card5 = @CardGUID5 
						 WHERE ID=@CardHolder;


SET @WhichTest = 'Test 5';
EXECUTE gp_GetCard @PhoneNumber, @MerchantGUID, @ErrorCode OUTPUT, @CardGUID OUTPUT, @EncryptedFirstName OUTPUT, @EncryptedLastName OUTPUT;
IF @ErrorCode <> 'CRDER' GOTO TestFailed


-- Test 8 - middle phone number on cardholder is valid - same merchant

UPDATE [Cards] SET [MerchantGUID] = @MerchantGUID WHERE CardNumber = @CardNumber3;

SET @WhichTest = 'Test 8';
EXECUTE gp_GetCard @PhoneNumber, @MerchantGUID, @ErrorCode OUTPUT, @CardGUID OUTPUT, @EncryptedFirstName OUTPUT, @EncryptedLastName OUTPUT;
IF @ErrorCode <> 'VALID' GOTO TestFailed





SELECT 'All Tests Worked'
GOTO Cleanup;






TestFailed:

SELECT @WhichTest, 'Failed';
              
Cleanup:

IF @MerchantGUID1 is null 
BEGIN
  SELECT @MerchantGUID1 = MerchantGUID FROM Merchant WHERE MerchantID=@MerchantID1;
END
IF @MerchantGUID2 is null 
BEGIN
  SELECT @MerchantGUID2 = MerchantGUID FROM Merchant WHERE MerchantID=@MerchantID2;
END


IF @CardGUID IS NULL
BEGIN
  SELECT @CardGUID = CardGUID FROM Cards WHERE CardNumber = @CardNumber;
END
IF @CardGUID2 IS NULL
BEGIN
  SELECT @CardGUID2 = CardGUID FROM Cards WHERE CardNumber = @CardNumber2;
END

DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID5;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID4;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID3;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID2;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID1;
DELETE FROM MerchantGroups WHERE GroupCode = @MerchantGroup;
DELETE FROM Cards WHERE CardNumber = @CardNumber;
DELETE FROM Cards WHERE CardNumber = @CardNumber2;
DELETE FROM Cards WHERE CardNumber = @CardNumber3;
DELETE FROM Cards WHERE CardNumber = @CardNumber4;
DELETE FROM Cards WHERE CardNumber = @CardNumber5;
DELETE FROM CardHolders WHERE EncryptedPhone = @PhoneNumber;
DELETE FROM Merchant WHERE MerchantID=@MerchantID1;
DELETE FROM Merchant WHERE MerchantID=@MerchantID2;
DELETE FROM Merchant WHERE MerchantID=@MerchantID3;
DELETE FROM Merchant WHERE MerchantID=@MerchantID4;
DELETE FROM Merchant WHERE MerchantID=@MerchantID5;
DELETE FROM Merchant WHERE MerchantID=@MerchantID6;
