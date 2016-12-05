-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_GiftBalanceInq'))
	BEGIN
		DROP  Procedure  gp_GiftBalanceInq
	END

GO



  -- ***************************************************************************
  --
  -- 			G i f t   B a l a n c e   I n q u i r y
  --
  -- ***************************************************************************
  --
  --    Balance Inquiry doesn't have a register entry - no invoice number
  --
  --    This routine returns a result set of:
  --             ResponseCode        char (1)
  --             Message Indicator   char (5)
  --             Transaction Number  BIGINT
  --             ReceiptTime         TimeStamp
  --             Remaining Balance  decimal (16,2)

CREATE Procedure gp_GiftBalanceInq
    (
    @Merchant        VARCHAR (46),
    @Clerk           NVARCHAR (10),
    @WhereFrom       CHAR,
    @MerchSeqNum     VARCHAR (20),
    @TerminalID      VARCHAR(10),      --  merchant terminal id
    @LocalTime       DATETIME,         --  merchant reported transaction time
    @CardToFind      VARCHAR (70)
    )
AS
BEGIN
  DECLARE @ErrorCode         CHAR (5);
  DECLARE @ResponseCode      CHAR;
  DECLARE @Shipt             CHAR;
  DECLARE @Act               CHAR;
  DECLARE @Actv              CHAR;
  DECLARE @Balance           DECIMAL (16,2);
  DECLARE @ReceiptTime       DATETIME;
  DECLARE @TranNumber        BIGINT;
  DECLARE @SeqNum            INT;
  DECLARE @MerchOffset       INT;
  DECLARE @MerchantGUID      UNIQUEIDENTIFIER;
  DECLARE @MerchantAt        UNIQUEIDENTIFIER;
  DECLARE @CardGUID          UNIQUEIDENTIFIER;



         -- set the defaults

    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';
    SET @TranNumber = 0;
    SET @ReceiptTime = GETDATE();
    SET @Balance = 0.00;


        -- validate the merchant

    SET @Merchant = UPPER(@Merchant);
    SELECT @MerchantGUID=MerchantGUID, 
           @Actv=GiftActive, 
           @SeqNum=LastSeqNumber
       FROM [Merchant] 
       WHERE MerchantID = @Merchant AND  GiftActive = 'A';
    IF (@@ROWCOUNT=0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MERID';
      GOTO LeaveSubNoLog
    END


                 -- see if it is in the database

    SELECT @Shipt=Shipped, 
           @Act=Activated, 
           @Balance=GiftBalance, 
           @CardGUID = CardGUID,
           @MerchantAt = MerchantGUID
      FROM [Cards] 
      WHERE CardNumber = @CardToFind;
          
                 -- card not found

    IF @@ROWCOUNT = 0
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDER';
      GOTO LeaveSub
    END

                 -- card not shipped

    IF @Shipt <> 'Y'
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDNS';
      GOTO LeaveSub
    END

                -- card not active

    IF @Act <> 'Y'
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDAC';
      GOTO LeaveSub
    END


               -- if the card was not activated at this merchant
               -- and the merchant is not part of the same chain 
               -- or group as the merchant that it was activated at

               -- card not valid at this location

    IF (@MerchantAt <> @MerchantGUID) 
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'NOTVL';
      GOTO LeaveSub
    END


    SET @ResponseCode = 'A';
    SET @ErrorCode = 'APP  ';

LeaveSub:
    EXECUTE gp_LogTransaction 'BALN', @MerchantGUID, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardToFind, @ErrorCode, @Balance, 'Balance Inquiry', NULL, '', @ReceiptTime OUTPUT, @TranNumber OUTPUT;

LeaveSubNoLog:
                 -- Build the result set for the calling program

    SELECT @ResponseCode AS ResponseCode, @ErrorCode AS ErrorCode, @TranNumber AS TranNumber, @ReceiptTime AS ReceiptTime, @Balance AS Balance;

END
GO

GRANT EXEC ON gp_GiftBalanceInq TO PUBLIC
GO
GRANT EXEC ON gp_GiftBalanceInq TO GiftCardApp
GO