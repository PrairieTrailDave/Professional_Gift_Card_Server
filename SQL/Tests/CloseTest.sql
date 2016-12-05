-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************

-- testing the close batch function with transfers between merchants
USE [Gift]
GO


-- variables holding test data

DECLARE @MerchantID1       VARCHAR (46);
DECLARE @MerchantID2       VARCHAR (46);
DECLARE @MerchantID3       VARCHAR (46);
DECLARE @Clerk             NVARCHAR (10);
DECLARE @CardNumber        VARCHAR (70);
DECLARE @CardNumber2       VARCHAR (70);
DECLARE @CardNumber3       VARCHAR (70);
DECLARE @WhereFrom         CHAR;
DECLARE @MerchSeqNum       VARCHAR (20);
DECLARE @MerchantGroup     NCHAR(8);
DECLARE @InvoiceNumber     NVARCHAR (15);
DECLARE @PhoneNumber       VARCHAR (20);
DECLARE @ActivateAmount    DECIMAL (19,2);
DECLARE @AmountOfSale      DECIMAL (19,2);
DECLARE @SalesDescription  NVARCHAR (40);
DECLARE @AmountOfCredit    DECIMAL (19,2);
DECLARE @CreditReason      NVARCHAR (40);
DECLARE @TerminalID        NCHAR(10);
DECLARE @LocalTime         DATETIME;

-- variables holding record IDs

DECLARE @MerchID           INT;
DECLARE @MerchID2          INT;
DECLARE @MerchID3          INT;
DECLARE @CardGUID          UNIQUEIDENTIFIER;
DECLARE @CardGUID2         UNIQUEIDENTIFIER;
DECLARE @CardGUID3         UNIQUEIDENTIFIER;
DECLARE @MerchantGUID1     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID2     UNIQUEIDENTIFIER;
DECLARE @MerchantGUID3     UNIQUEIDENTIFIER;
DECLARE @ChainGUID         UNIQUEIDENTIFIER;


DECLARE @WhichTest         VARCHAR (40);

-- Test Results

DECLARE @Transfers         INT;
DECLARE @TransferAmount    DECIMAL (19,2);

-- tables for stored procedure results

DECLARE @CloseBatchResults TABLE (
       [ID]  BIGINT  NOT NULL IDENTITY (1,1) ,
       ResponseCode     CHAR, 
       ErrorCode        CHAR (5), 
       ReceiptTime      DATETIME, 
       TranNumber       BIGINT 
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

DECLARE @GiftSaleResults TABLE (
       [ID]  BIGINT  NOT NULL IDENTITY (1,1) ,
       ResponseCode   CHAR, 
       ErrorCode      CHAR (5), 
       TranNumber     BIGINT, 
       ReceiptTime    DATETIME,
       Balance        DECIMAL (19,2),
       TransactionAmt DECIMAL (19,2),
       Remainder      DECIMAL (19,2)
);

DECLARE @GiftReturnResults TABLE (
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
SET @MerchantID3 = 'TEST497';
SET @Clerk = '';
SET @CardNumber = '1200345';
SET @CardNumber2 = '1200346';
SET @CardNumber3 = '1200347';
SET @WhereFrom = 'T';  -- testing
SET @MerchSeqNum = '0001';
SET @MerchantGroup = 'TESTGRP';
SET @InvoiceNumber = '123456';
SET @SalesDescription ='System Test Sale';
SET @CreditReason ='System Test Return';
SET @MerchantGroup = 'TESTGRP';
SET @ActivateAmount = 10.00;
SET @AmountOfSale = 1.00;
SET @AmountOfCredit = 1.00;
SET @TerminalID = '1';
SET @LocalTime = GETDATE();

-- uncomment this when you want to just clean up after a failed est

--GOTO Cleanup

-- Test 1 - when the merchant is not authorized
SET @WhichTest = 'Test 1'
INSERT INTO @CloseBatchResults
  EXECUTE gp_Close @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime;
IF (SELECT ErrorCode FROM @CloseBatchResults WHERE ID = (SELECT MAX(ID) FROM @CloseBatchResults)) <> 'MERID' GOTO TestFailed


-- Test 2 - create the merchant, but do not authorize it
SET @WhichTest = 'Test 2'
SET @MerchantGUID1 = NEWID();
INSERT INTO Merchant (MerchantID, MerchantGUID, MerchantName, Address1, City, State, PostalCode, Phone, TimeZone, LastSeqNumber, GiftActive, LoyaltyActive)
values (@MerchantID1, @MerchantGUID1, 'Test Merchant 1', 'Adr1', 'City', 'ST', '12345',  '9725551212', 'Central', 1, 'N', 'N');
SET @MerchID = Scope_Identity();

--SELECT 'Should have ResponseCode-E and ErrorCode-MERID';
INSERT INTO @CloseBatchResults
  EXECUTE gp_Close @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime;
IF (SELECT ErrorCode FROM @CloseBatchResults WHERE ID = (SELECT MAX(ID) FROM @CloseBatchResults)) <> 'MERID' GOTO TestFailed


-- Test 3 - authorize the merchant,  but there are no transactions to worry about

SET @WhichTest = 'Test 3'
UPDATE Merchant SET GiftActive='A' WHERE MerchantID=@MerchantID1;
UPDATE Merchant SET PaidUpTo=DATEADD(day, 10, GETDATE()) WHERE MerchantID=@MerchantID1;
--SELECT 'Should have ResponseCode-E and ErrorCode-CRDER';
INSERT INTO @CloseBatchResults
  EXECUTE gp_Close @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime;

SELECT @Transfers = Count(*) FROM [Clearing] WHERE FromMerchantGUID = @MerchantGUID1;
IF @Transfers <> 0 GOTO TestFailed
SELECT @Transfers = Count(*) FROM [Clearing] WHERE ToMerchantGUID = @MerchantGUID1;
IF @Transfers <> 0 GOTO TestFailed


-- Test 4 - Merchant has transactions, but no transaction from another merchant
SET @WhichTest = 'Test 4'

-- put a card on file

SET @CardGUID = NewID();
INSERT INTO Cards (CardGUID, CardNumber, CardNumLast4, GiftBalance, MerchantGUID, Activated, Shipped)
VALUES (@CardGUID, @CardNumber, '0345', 0.00, @MerchantGUID1, 'N', 'Y');

-- activate that card (put an activation into the history table)

INSERT INTO @GiftActivateResults
  EXECUTE gp_GiftActivateCard @MerchantID1, @Clerk, 'T', @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, NULL, 10.00, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftActivateResults WHERE ID = (SELECT MAX(ID) FROM @GiftActivateResults)) <> 'APP  ' GOTO TestFailed


INSERT INTO @GiftSaleResults
  EXECUTE gp_GiftSellFromCard @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, @AmountOfSale, @SalesDescription, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftSaleResults WHERE ID = (SELECT MAX(ID) FROM @GiftSaleResults)) <> 'APP  ' GOTO TestFailed

INSERT INTO @GiftReturnResults
  EXECUTE gp_GiftReturn @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNumber, @AmountOfCredit, @CreditReason, @InvoiceNumber;
IF (SELECT ErrorCode FROM @GiftReturnResults WHERE ID = (SELECT MAX(ID) FROM @GiftReturnResults)) <> 'APP  ' GOTO TestFailed

INSERT INTO @CloseBatchResults
  EXECUTE gp_Close @MerchantID1, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime;









SELECT 'Tests Worked';
GOTO Cleanup

TestFailed:

SELECT @WhichTest, 'Failed';
SELECT * FROM @CloseBatchResults;
    

          
Cleanup:

IF @MerchantGUID1 is null 
BEGIN
  SELECT @MerchantGUID1 = MerchantGUID FROM Merchant WHERE MerchantID=@MerchantID1;
END
IF @MerchantGUID2 is null 
BEGIN
  SELECT @MerchantGUID2 = MerchantGUID FROM Merchant WHERE MerchantID=@MerchantID2;
END
IF @MerchantGUID3 is null 
BEGIN
  SELECT @MerchantGUID3 = MerchantGUID FROM Merchant WHERE MerchantID=@MerchantID3;
END


IF @CardGUID IS NULL
BEGIN
  SELECT @CardGUID = CardGUID FROM Cards WHERE CardNumber = @CardNumber;
END
IF @CardGUID2 IS NULL
BEGIN
  SELECT @CardGUID2 = CardGUID FROM Cards WHERE CardNumber = @CardNumber2;
END
IF @CardGUID3 IS NULL
BEGIN
  SELECT @CardGUID3 = CardGUID FROM Cards WHERE CardNumber = @CardNumber3;
END

DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID3;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID2;
DELETE FROM History WHERE WhichMerchantGUID = @MerchantGUID1;
DELETE FROM CardHolders WHERE EncryptedPhone = @PhoneNumber;
DELETE FROM Cards WHERE CardNumber = @CardNumber;
DELETE FROM Cards WHERE CardNumber = @CardNumber2;
DELETE FROM Cards WHERE CardNumber = @CardNumber3;
DELETE FROM [Clearing] WHERE FromMerchantGUID = @MerchantGUID1; 
DELETE FROM [Clearing] WHERE ToMerchantGUID = @MerchantGUID1; 
DELETE FROM [Clearing] WHERE FromMerchantGUID = @MerchantGUID2; 
DELETE FROM [Clearing] WHERE ToMerchantGUID = @MerchantGUID2; 
DELETE FROM [Clearing] WHERE FromMerchantGUID = @MerchantGUID3; 
DELETE FROM [Clearing] WHERE ToMerchantGUID = @MerchantGUID3; 
DELETE FROM Merchant WHERE MerchantID=@MerchantID1;
DELETE FROM Merchant WHERE MerchantID=@MerchantID2;
DELETE FROM Merchant WHERE MerchantID=@MerchantID3;

