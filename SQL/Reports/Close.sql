-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************


USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_Close'))
	BEGIN
		DROP  Procedure  gp_Close
	END

GO



  -- ***************************************************************************
  --
  -- 			g p _ C l o s e
  --
  -- ***************************************************************************
  --
  --   The output from this stored procedure is a single row with the columns
  --             ResponseCode        CHAR ( 1 )
  --             Message Indicator   char (5)
  --             HostTime            TimeStamp
  --  In the case of an error, only the first two columns are returned


CREATE Procedure gp_Close
    (
    @MID             VARCHAR (46),
    @Clerk           NVARCHAR (10),
    @WhereFrom       CHAR,
    @MerchSeqNum     VARCHAR (20),
    @TerminalID      VARCHAR(10),      --  merchant terminal id
    @LocalTime       DATETIME          --  merchant reported transaction time

    )
AS
BEGIN
  DECLARE @ErrorCode       CHAR (5);
  DECLARE @ResponseCode    CHAR;
  DECLARE @MerchID         INT;
  DECLARE @SeqNum          INT;
  DECLARE @MerchOffset     INT;
  DECLARE @TranNumber      BIGINT;
  DECLARE @HostTime        DATETIME;
  DECLARE @MerchantGUID    UNIQUEIDENTIFIER;
  DECLARE @DummyCard       CHAR(2);


  BEGIN TRANSACTION;-- so that no transactions run while we do this

       -- set the defaults 

    SET @HostTime = GETDATE();
    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';
    SET @TranNumber = 0;
    SET @DummyCard = 1;


       -- validate the merchant

    EXECUTE gp_ValidateMerchant @MID, @MerchID OUTPUT, @SeqNum OUTPUT, @MerchantGUID OUTPUT;
    IF (@MerchID < 0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MERID';
    END
    ELSE
    BEGIN


            -- say that we are doing today's close

      EXECUTE gp_LogTransaction 'CLOS', @MerchantGUID, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @DummyCard, 'APP  ', 0, 'Close', NULL,  '', @HostTime OUTPUT, @TranNumber OUTPUT;
      --SET @ReceiptTime = DATEADD(HOUR, @MerchOffset, @HostTime);

            -- set the response to the calling program

      SET @ResponseCode = 'A';
      SET @ErrorCode = 'APP  ';
    END

  SELECT @ResponseCode as ResponseCode, 
         @ErrorCode as ErrorCode, 
		 @HostTime as ReceiptTime, 
		 @TranNumber as TranNumber;  
  COMMIT;-- TRANSACTION


END
GO

GRANT EXEC ON gp_Close TO PUBLIC

GO

GRANT EXEC ON gp_Close TO GiftCardApp
GO
