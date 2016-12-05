-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]



IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_GiftActivateCard'))
	BEGIN
		DROP  Procedure  gp_GiftActivateCard
	END

GO



  -- ***************************************************************************
  --
  -- 			G i f t   A c t i v a t e   C a r d
  --
  -- ***************************************************************************
  --
  --    This routine returns a result set of:
  --             ResponseCode        char (1)
  --             Message Indicator   char (5)
  --             Transaction Number  BIGINT
  --             ReceiptTime         TimeStamp

CREATE Procedure gp_GiftActivateCard
    (
    @Merchant        VARCHAR (46),
    @Clerk           NVARCHAR (10),
    @WhereFrom       CHAR,             -- if from web, terminal, cell
    @MerchSeqNum     VARCHAR (20),
    @TerminalID      VARCHAR(10),      --  merchant terminal id
    @LocalTime       DATETIME,         --  merchant reported transaction time
    @CardToActivate  VARCHAR (70),
    @NewCardHolder   INT,
    @Amount          DECIMAL (19,2),
    @InvoiceNum      NVARCHAR (15)
    )
AS
BEGIN
  DECLARE @ResponseCode    CHAR;
  DECLARE @temp            INT;
  DECLARE @TestShipped     CHAR;
  DECLARE @TestActive      CHAR;
  DECLARE @ErrorCode       CHAR (5);
  DECLARE @ReceiptTime     DATETIME;
  DECLARE @MerchID         INT;
  DECLARE @Actv            CHAR;
  DECLARE @SeqNum          INT;
  DECLARE @MerchOffset     INT;
  DECLARE @TranNumber      BIGINT;
  DECLARE @MerchantGUID    UNIQUEIDENTIFIER;
  DECLARE @CardGUID        UNIQUEIDENTIFIER;
  DECLARE @CardHolderGUID  UNIQUEIDENTIFIER;
  DECLARE @Balance         DECIMAL (19,2);
  DECLARE @PaidToDate      DATE;


      -- set the defaults

    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';
    SET @TranNumber = 0;
    SET @ReceiptTime = GETDATE();



    SET @Clerk = REPLACE(@Clerk, '''', '''''');
    IF (CHARINDEX(';', @Clerk) > 0)
    BEGIN
      SET @Clerk = SUBSTRING(@Clerk, 1, CHARINDEX(';', @Clerk) - 1);
    END

     -- When a card is activated, it gets an amount and possible cardholder
     -- validate the merchant

        -- validate the merchant

    SET @Merchant = REPLACE(@Merchant, '''', '''''');
    IF (CHARINDEX(';', @Merchant) > 0)
    BEGIN
      SET @Merchant = SUBSTRING(@Merchant, 1, CHARINDEX(';', @Merchant) - 1);
    END
    SET @Merchant = UPPER(@Merchant);
    SELECT @MerchID=ID, 
           @Actv=GiftActive, 
           @SeqNum=LastSeqNumber,
           @MerchantGUID=MerchantGUID,
           @PaidToDate=PaidUpTo
       FROM [Merchant] 
        WHERE MerchantID = @Merchant AND  GiftActive = 'A';
    IF (@@ROWCOUNT=0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MERID';
      GOTO LeaveSubNoLog;
    END

        -- make sure that the merchant is paid up to date

    IF @PaidToDate IS NULL
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MEREX';
      GOTO LeaveSub;
    END
    IF (@PaidToDate < GETDATE()) 
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MEREX';
      GOTO LeaveSub;
    END

                -- validate the card

    SELECT @TestShipped=Shipped, @TestActive=Activated, 
           @CardGUID = CardGUID
       FROM [Cards] 
        WHERE CardNumber = @CardToActivate;

                 -- card not found

    IF (@@ROWCOUNT = 0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDER';
      GOTO LeaveSub;
    END

                 -- card not shipped

    IF (@TestShipped != 'Y')
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDNS';
      GOTO LeaveSub;
    END

                 -- at present, if the card was shipped to another merchant
                 -- is can still be activated here. 
                 -- we are not checking to see if the card was shipped to this merchant



                 -- card already active

    IF (@TestActive = 'Y')
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDAL';
      GOTO LeaveSub
    END


                 -- card ready to activate


    IF @NewCardHolder IS NOT NULL AND @NewCardHolder > 0 
    BEGIN
      SELECT @CardHolderGUID = CardHolderGUID 
        FROM CardHolders
       WHERE ID = @NewCardHolder;

      UPDATE [Cards]
         SET GiftBalance = @Amount, 
             Activated = 'Y', 
             CardHolderGUID = @CardHolderGUID, 
             MerchantGUID = @MerchantGUID,
             DateActivated = GETDATE()
       WHERE CardNumber = @CardToActivate;
    END
    ELSE
    BEGIN
      UPDATE [Cards]
         SET GiftBalance = @Amount, 
             Activated = 'Y', 
             MerchantGUID = @MerchantGUID,
             DateActivated = GETDATE()
      WHERE CardNumber = @CardToActivate;
    END
            


                 -- set the response message

    SET @ResponseCode = 'A';
    SET @ErrorCode = 'APP  ';

LeaveSub:
                  -- and log the transaction

    EXECUTE gp_LogTransaction 'ACTV', @MerchantGUID, @Clerk, @WhereFrom,  @MerchSeqNum, @TerminalID, @LocalTime, @CardToActivate, @ErrorCode, @Amount, 'Activate', NULL, @InvoiceNum, @ReceiptTime OUTPUT, @TranNumber OUTPUT;

LeaveSubNoLog:
    SELECT @ResponseCode AS ResponseCode, @ErrorCode AS ErrorCode, @TranNumber AS TranNumber, @ReceiptTime AS ReceiptTime;
END
GO

GRANT EXEC ON gp_GiftActivateCard TO PUBLIC
GO

GRANT EXEC ON gp_GiftActivateCard TO GiftCardApp
GO
