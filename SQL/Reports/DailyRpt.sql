-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************


USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_DailyReport'))
	BEGIN
		DROP  Procedure  gp_DailyReport
	END

GO



  -- ***************************************************************************
  --
  -- 			g p _ D a i l y R e p o r t
  --
  -- ***************************************************************************
  --
  --   The output from this stored procedure is a single row with the columns
  --             ResponseCode         CHAR ( 1 )
  --             Message Indicator    char (5)
  --             ReceiptTime          TimeStamp
  --             GiftActive           CHAR
  --             LoyaltyActive        CHAR
  --             CardsActivated       INT
  --             CardActivateAmt      DECIMAL(19,2)
  --             Sales                INT
  --             SalesTotal           DECIMAL(19,2)
  --             Credits              INT
  --             CreditTotal          DECIMAL(19,2)
  --             NetTotal             DECIMAL(19,4)
  --             Voids                INT
  --
  --  In the case of an error, only the first two columns are returned


CREATE Procedure gp_DailyReport
(
    @MID             VARCHAR (46),
    @Clerk           NVARCHAR (10),
    @Type            CHAR,            -- if detail or daily report
    @WhereFrom       CHAR,
    @MerchSeqNum     VARCHAR (20),
    @TerminalID      VARCHAR(10),      --  merchant terminal id
    @LocalTime       DATETIME          --  merchant reported transaction time
)
AS
BEGIN
  DECLARE @ErrorCode            CHAR (5);
  DECLARE @ResponseCode         CHAR;
  DECLARE @LastRun              DATETIME;
  DECLARE @TestID               INT;
  DECLARE @GiftActive           CHAR;
  DECLARE @LoyaltyActive        CHAR;
  DECLARE @GiftActivations      INT;
  DECLARE @GiftActivationTotal  MONEY;
  DECLARE @GiftSales            INT;
  DECLARE @GiftSalesTotal       MONEY;
  DECLARE @GiftCredits          INT;
  DECLARE @GiftCreditTotal      MONEY;
  DECLARE @NetGiftTotal         MONEY;
  DECLARE @ReceiptTime          DATETIME;
  DECLARE @Voids                INT;
  DECLARE @MerchID              INT;
  DECLARE @SeqNum               INT;
  DECLARE @MerchOffset          INT;
  DECLARE @TranNumber           BIGINT;
  DECLARE @HostTime             DATETIME;
  DECLARE @FirstCard            VARCHAR(70);
  DECLARE @MerchantGUID         UNIQUEIDENTIFIER;


    SET @ReceiptTime = GETDATE();
    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';

    SET @GiftActivations = 0;
    SET @GiftActivationTotal = 0.00;
    SET @GiftSales = 0;
    SET @GiftSalesTotal = 0.00;
    SET @GiftCredits = 0;
    SET @GiftCreditTotal = 0.00;
    SET @NetGiftTotal = 0.00;
    SET @Voids = 0;





       -- validate the merchant

    EXECUTE gp_ValidateMerchant @MID, @MerchID OUTPUT, @SeqNum OUTPUT, @MerchantGUID OUTPUT;
    IF (@MerchID < 0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MERID';
    END
    ELSE
    BEGIN

         -- get what services are active

      SELECT @GiftActive = GiftActive
       FROM  [Merchant]
       WHERE MerchantGUID = @MerchantGUID;

         -- get when the last run was done

      SELECT TOP(1) @LastRun=WhenHappened
         FROM [History] 
        WHERE WhichMerchantGUID = @MerchantGUID AND 
              TransType = 'CLOS'
        ORDER BY ID DESC ;
      IF @@ROWCOUNT=0
      BEGIN
        SET @LastRun = 0;
      END

          -- get the sums from the history

      IF @GiftActive = 'A'
      BEGIN
        SELECT @GiftActivations=COUNT(*), @GiftActivationTotal=ISNULL(SUM(Amount), 0.00)
           FROM [History] 
            WHERE WhichMerchantGUID = @MerchantGUID AND 
                  TransType = 'ACTV' AND 
                  ErrorCode = 'APP  ' AND
                  WhenHappened > @LastRun;
        IF @GiftActivationTotal IS NULL
        BEGIN
          SET @GiftActivationTotal = 0.00;
        END

        SELECT @GiftSales=COUNT(*), @GiftSalesTotal=ISNULL(SUM(Amount), 0.00)
           FROM [History] 
            WHERE WhichMerchantGUID = @MerchantGUID AND 
                  (TransType = 'SALE' OR TransType = 'GTIP') AND 
                  ErrorCode = 'APP  ' AND
                  WhenHappened > @LastRun;
        IF @GiftSalesTotal IS NULL
        BEGIN
          SET @GiftSalesTotal = 0.00;
        END

        SELECT @GiftCredits=COUNT(*), @GiftCreditTotal=ISNULL(SUM(Amount), 0.00)
           FROM [History] 
            WHERE WhichMerchantGUID = @MerchantGUID AND 
                 (TransType = 'CRED' OR TransType= 'TPUP') AND 
                  ErrorCode = 'APP  ' AND
                 WhenHappened > @LastRun;
        IF @GiftCreditTotal IS NULL
        BEGIN
          SET @GiftCreditTotal = 0.00;
        END
        SET @NetGiftTotal = @GiftActivationTotal - @GiftSalesTotal + @GiftCreditTotal;
      END

      SELECT @Voids=COUNT(*)
         FROM [History] 
          WHERE WhichMerchantGUID = @MerchantGUID AND 
                TransType = 'VOID' AND 
                ErrorCode = 'APP  ' AND
                WhenHappened > @LastRun;


      SELECT Top(1) @FirstCard=CardNumber
         FROM [Cards]
          ORDER BY ID;
        
      IF @Type = 'A' 
      BEGIN  
          EXECUTE gp_LogTransaction 'DYRP', @MerchantGUID, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @FirstCard, 'APP  ', 
		                            @NetGiftTotal, NULL, NULL, '', @HostTime OUTPUT, @TranNumber OUTPUT;

          --SET @ReceiptTime = DATEADD(HOUR, @MerchOffset, @HostTime);
      END
            -- set the response to the calling program

      SET @ResponseCode = 'A';
      SET @ErrorCode = 'APP  ';
    END

    SELECT @ResponseCode as ResponseCode, 
           @ErrorCode as ErrorCode, 
           @HostTime as ReceiptTime, 
           @GiftActive AS GiftActive,
           @LoyaltyActive as LoyaltyActive,
           @GiftActivations as GiftActivates, 
           @GiftActivationTotal as GiftActiveTotal, 
           @GiftSales as GiftSales, 
           @GiftSalesTotal as GiftSalesTotal, 
           @GiftCredits as GiftCredits, 
           @GiftCreditTotal as GiftCreditTotal, 
           @NetGiftTotal as NetGiftTotal, 
           @Voids as Voids;


END
GO

GRANT EXEC ON gp_DailyReport TO PUBLIC

GO

GRANT EXEC ON gp_DailyReport TO GiftCardApp
GO
