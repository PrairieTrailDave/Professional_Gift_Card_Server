-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************

-- testing the gift return / credit
USE [Gift]
GO

DECLARE @AmountOfCredit    DECIMAL (19,2);
DECLARE @MerchantID1       VARCHAR (46);
DECLARE @MerchantID2       VARCHAR (46);
DECLARE @Clerk             NVARCHAR (10);
DECLARE @CardNumber        VARCHAR (70);
DECLARE @CardNumber2       VARCHAR (70);
DECLARE @WhereFrom         CHAR;
DECLARE @MerchSeqNum       VARCHAR (20);
DECLARE @PhoneNumber       VARCHAR (20);
DECLARE @CreditReason      NVARCHAR (40);
DECLARE @MerchantGroup     NCHAR(8);
DECLARE @InvoiceNumber     NVARCHAR (15);
DECLARE @TerminalID        NCHAR(10);
DECLARE @LocalTime         DateTime;

-- variables holding record IDs

DECLARE @MerchID           INT;
DECLARE @MerchID2          INT;
DECLARE @ChainGUID         UNIQUEIDENTIFIER;
DECLARE @CardGUID          UNIQUEIDENTIFIER;
DECLARE @CardGUID2         UNIQUEIDENTIFIER;
DECLARE @RewardCountID     INT;
DECLARE @RewardID          INT;
DECLARE @RewardGUID        UNIQUEIDENTIFIER;
DECLARE @MerchantGUID1     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID2     UNIQUEIDENTIFIER;

DECLARE @WhichTest         VARCHAR (40);

-- tables for stored procedure results

DECLARE @GiftReturnResults TABLE (
       [ID]  BIGINT  NOT NULL IDENTITY (1,1) ,
       ResponseCode   CHAR, 
       ErrorCode      CHAR (5), 
       TranNumber     BIGINT, 
       ReceiptTime    DATETIME,
       Balance        DECIMAL (19,2) 
);

DECLARE @GiftShipResults TABLE (
       [ID]  BIGINT  NOT NULL IDENTITY (1,1) ,
       ResponseCode   CHAR, 
       ErrorCode      CHAR (5), 
       TranNumber     BIGINT, 
       ReceiptTime    DATETIME
);

DECLARE @GiftActivateResults TABLE (
       [ID]  BIGINT  NOT NULL IDENTITY (1,1) ,
       ResponseCode   CHAR, 
       ErrorCode      CHAR (5), 
       TranNumber     BIGINT, 
       ReceiptTime    DATETIME
);

DECLARE @GiftBalanceResults TABLE (
       [ID]  BIGINT  NOT NULL IDENTITY (1,1) ,
       ResponseCode   CHAR, 
       ErrorCode      CHAR (5), 
       TranNumber     BIGINT, 
       ReceiptTime    DATETIME,
       Balance        DECIMAL (19,2) 
);


-- define some test values

SET @MerchantID1 = 'TEST495';
SET @MerchantID2 = 'TEST496';
SET @Clerk = '';
SET @CardNumber = '1200345';
SET @CardNumber2 = '1200346';
SET @WhereFrom = 'T';  -- testing
SET @MerchSeqNum = '0001';
SET @PhoneNumber = '9726185511';
SET @CreditReason ='System Test Return';
SET @MerchantGroup = 'TESTGRP';
SET @AmountOfCredit = 1.00;
SET @InvoiceNumber = '12345876';
SET @TerminalID = '1';
SET @LocalTime = GETDATE();

-- uncomment this when you want to just clean up after a failed est

--GOTO Cleanup

-- Test 1 - when the merchant is not authorized

SET @WhichTest = 'Test 1';
INSERT INTO @GiftReturnResults
  EXECUTE gp_GiftReturn @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, @AmountOfCredit, @CreditReason, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftReturnResults WHERE ID = (SELECT MAX(ID) FROM @GiftReturnResults)) <> 'MERID' GOTO TestFailed


-- Test 2 - create the merchant, but do not authorize it
SET @WhichTest = 'Test 2';
SET @MerchantGUID1 = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID, MerchantName, Address1, City, State, PostalCode, Phone, TimeZone, LastSeqNumber, GiftActive, LoyaltyActive)
values (@MerchantID1, @MerchantGUID1, 'Test Merchant 1', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1, 'N', 'N');
SET @MerchID = Scope_Identity();

--SELECT 'Should have ResponseCode-E and ErrorCode-MERID';
INSERT INTO @GiftReturnResults
  EXECUTE gp_GiftReturn @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, @AmountOfCredit, @CreditReason, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftReturnResults WHERE ID = (SELECT MAX(ID) FROM @GiftReturnResults)) <> 'MERID' GOTO TestFailed


-- Test 3 - authorize the merchant, but the merchant has not paid
--SET @WhichTest = 'Test 3';
-- Credit does not check if the merchant is paid up to date


-- Test 5 - authorize the merchant, but the card is not on file
SET @WhichTest = 'Test 5';

UPDATE Merchant SET GiftActive='A' WHERE MerchantID=@MerchantID1;
UPDATE Merchant SET PaidUpTo=DATEADD(day, 10, GETDATE()) WHERE MerchantID=@MerchantID1;
INSERT INTO @GiftReturnResults
  EXECUTE gp_GiftReturn @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, @AmountOfCredit, @CreditReason, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftReturnResults WHERE ID = (SELECT MAX(ID) FROM @GiftReturnResults)) <> 'CRDER' GOTO TestFailed




-- Test 6 - card is on file, but not shipped or activated
SET @WhichTest = 'Test 6';

SET @CardGUID = NewID();
INSERT INTO Cards (CardGUID, CardNumber, CardNumLast4, GiftBalance, MerchantGUID, Activated)
VALUES (@CardGUID, @CardNumber, '0345', 0.00, @MerchantGUID1, 'N');

INSERT INTO @GiftReturnResults
  EXECUTE gp_GiftReturn @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, @AmountOfCredit, @CreditReason, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftReturnResults WHERE ID = (SELECT MAX(ID) FROM @GiftReturnResults)) <> 'CRDAC' GOTO TestFailed


-- Test 7 - card is on file and shipped but not activated
SET @WhichTest = 'Test 7';

INSERT INTO @GiftShipResults
  EXECUTE gp_ShipCard @MerchantID1, @Clerk, @TerminalID, @LocalTime, @CardNumber, 'Test card Shipment'
IF (SELECT ErrorCode FROM @GiftShipResults WHERE ID = (SELECT MAX(ID) FROM @GiftShipResults)) <> 'APP  ' GOTO TestFailed

INSERT INTO @GiftReturnResults
  EXECUTE gp_GiftReturn @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, @AmountOfCredit, @CreditReason, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftReturnResults WHERE ID = (SELECT MAX(ID) FROM @GiftReturnResults)) <> 'CRDAC' GOTO TestFailed






SELECT 'All Tests Worked'
GOTO Cleanup;






TestFailed:

SELECT @WhichTest, 'Failed';
SELECT * FROM @GiftReturnResults WHERE ID = (SELECT MAX(ID) FROM @GiftReturnResults);
SELECT * FROM @GiftActivateResults WHERE ID = (SELECT MAX(ID) FROM @GiftActivateResults);
SELECT * FROM @GiftBalanceResults WHERE ID = (SELECT MAX(ID) FROM @GiftBalanceResults); 
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

DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID2;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID1;
DELETE FROM MerchantGroups WHERE GroupCode = @MerchantGroup;
--DELETE FROM CardHolders WHERE Phone = @PhoneNumber;
DELETE FROM Cards WHERE CardNumber = @CardNumber;
DELETE FROM Cards WHERE CardNumber = @CardNumber2;
DELETE FROM Merchant WHERE MerchantID=@MerchantID1;
DELETE FROM Merchant WHERE MerchantID=@MerchantID2;
