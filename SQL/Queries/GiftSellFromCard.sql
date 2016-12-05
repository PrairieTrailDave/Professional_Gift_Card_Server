-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_GiftSellFromCard'))
	BEGIN
		DROP  Procedure  gp_GiftSellFromCard
	END

GO
IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_GiftTipFromCard'))
	BEGIN
		DROP  Procedure  gp_GiftTipFromCard
	END

GO
IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_GiftSaleCommon'))
	BEGIN
		DROP  Procedure  gp_GiftSaleCommon
	END

GO






  -- ***************************************************************************
  --
  -- 		G i f t   S a l e    C o m m o n
  --
  -- ***************************************************************************
  --
  --    This routine returns a result set of:
  --             ResponseCode        CHAR (1)
  --             Message Indicator   CHAR (5)
  --             Transaction Number  BIGINT
  --             ReceiptTime         TIMESTAMP
  --             Balance Remaining   DECIMAL (19,2)
  --             Transaction Amount  DECIMAL (19,2)
  --             Remainder Amount    DECIMAL (19,2)

CREATE Procedure gp_GiftSaleCommon
    (
    @TransType            CHAR (5),
    @MerchantID        VARCHAR (46),
    @Clerk            NVARCHAR (10),
    @WhereFrom            CHAR,
    @MerchSeqNum       VARCHAR (20),
    @TerminalID        VARCHAR(10),      --  merchant terminal id
    @LocalTime        DATETIME,          --  merchant reported transaction time
    @CardToSellFrom    VARCHAR (70),
    @AmountOfSale      DECIMAL (19,2),
    @SalesDescription NVARCHAR (40),
    @InvoiceNum       NVARCHAR (15),
    @OriginalSalesTran  BIGINT
    )
AS
BEGIN
  DECLARE @CardHolder         INT;
  DECLARE @Merch              INT;
  DECLARE @CurrentBalance     DECIMAL (19,2);
  DECLARE @Balance            DECIMAL (19,2);
  DECLARE @TransactionAmount  DECIMAL (19,2);
  DECLARE @Remainder          DECIMAL (19,2);
  DECLARE @TransactionTotal   DECIMAL (19,2);
  DECLARE @ErrorCode          CHAR (5);
  DECLARE @ResponseCode       CHAR;
  DECLARE @Actv               CHAR;
  DECLARE @Act                CHAR;
  DECLARE @Shipt              CHAR;
  DECLARE @ReceiptTime        DATETIME;
  DECLARE @TranNumber         BIGINT;
  DECLARE @MerchID            INT;
  DECLARE @SeqNum             INT;
  DECLARE @MerchOffset        INT;
  DECLARE @MerchantGUID       UNIQUEIDENTIFIER;
  DECLARE @MerchantAt         UNIQUEIDENTIFIER;
  DECLARE @CardGUID           UNIQUEIDENTIFIER;
  DECLARE @PaidToDate         DATE;
  DECLARE @SplitTender        CHAR(1);
  DECLARE @WhenHap            DATETIME;



               -- set the defaults

    SET @TranNumber = 0;
    SET @Balance = 0.00;
    IF (@TransType = 'SALE')
         SET @TransactionTotal = @AmountOfSale;
    ELSE
         SET @TransactionTotal = 0.00;

    SET @TransactionAmount = 0.00;
    SET @Remainder = @AmountOfSale;
    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';
    SET @MerchantGUID = cast(cast(0 as binary) as uniqueidentifier);
    SET @ReceiptTime = GETDATE();

               -- validate the merchant

    SET @MerchantID = UPPER(@MerchantID);
    SELECT @Actv=GiftActive, 
           @SeqNum=LastSeqNumber, 
           @MerchantGUID=MerchantGUID,
           @SplitTender=SplitTender,
           @PaidToDate=PaidUpTo
       FROM [Merchant] 
        WHERE MerchantID = @MerchantID AND  GiftActive = 'A';
    IF (@@ROWCOUNT=0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MERID';
      GOTO LeaveSubNoLog
    END


    BEGIN TRANSACTION


               -- make sure that the merchant is paid up to date

    IF @PaidToDate IS NULL
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MEREX';
      GOTO LeaveSub
    END
    IF (@PaidToDate < Convert(date, GetDate())) 
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MEREX';
      GOTO LeaveSub
    END


 
               -- see if this card exists

    SELECT @CurrentBalance=GiftBalance, 
           @MerchantAt = MerchantGUID,
           @CardGUID = CardGUID,
           @Shipt=Shipped, 
           @Act=Activated
       FROM [Cards] 
       WHERE CardNumber = @CardToSellFrom;

               -- card not found

    IF @@ROWCOUNT=0
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDER';
      GOTO LeaveSub
    END

               -- card not shipped

    IF @Shipt<>'Y'
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDNS';
      GOTO LeaveSub
    END

               -- card not active

    IF @Act<>'Y'
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'CRDAC';
      GOTO LeaveSub
    END

               -- if the card was not activated at this merchant

    IF (@MerchantAt <> @MerchantGUID)
    BEGIN

               -- card not valid at this location

      SET @ResponseCode = 'E';
      SET @ErrorCode = 'NOTVL';
      GOTO LeaveSub
    END


               -- does the card have enough balance on it?

    IF @CurrentBalance < @AmountOfSale
    BEGIN

               -- does this merchant support split tender?

      IF @SplitTender = 'Y'
      BEGIN
        SET @TransactionAmount = @CurrentBalance;
        SET @Remainder = @AmountOfSale - @TransactionAmount;
      END
      ELSE
               -- Not Sufficient Funds
      BEGIN
        SET @ResponseCode = 'E';
        SET @ErrorCode = 'NSF  ';
        GOTO LeaveSub
      END
    END

    ELSE
    BEGIN
      SET @TransactionAmount = @AmountOfSale;
      SET @Remainder = 0.00;
    END

               -- if so, update it

    UPDATE [Cards]
       SET GiftBalance = GiftBalance-@TransactionAmount
     WHERE CardNumber = @CardToSellFrom;
    SELECT @Balance=GiftBalance
      FROM [Cards] 
     WHERE CardNumber = @CardToSellFrom;



                 -- if tip transaction, update original sale transaction amounts

    IF (@TransType = 'GTIP')
    BEGIN
      UPDATE History
          SET Amount = Amount + @TransactionAmount,
              TipAmount = @TransactionAmount
          WHERE ID = @OriginalSalesTran;
    END;


    SET @ResponseCode = 'A';
    SET @ErrorCode = 'APP  ';


            -- log the transaction
LeaveSub:

            -- use a default card number for logging errors

    IF @CardGUID IS NULL
    BEGIN
       SELECT TOP(1) @CardGUID=CardGUID
          FROM [Cards]  ORDER BY ID ;
    END

    SET @WhenHap = GETDATE();

              -- and store them all in the table

    IF (@TransType = 'SALE')
    BEGIN
      INSERT  INTO History (WhenHappened, CardGUID, WhichMerchantGUID, Clerk, WebCellOrDialup, TransType, TransactionText,     ErrorCode, Amount,         TipAmount, TabAmount,        CardGUID2, MerchSeqNumber, InvoiceNumber, TerminalID, LocalTime)
                    VALUES (@WhenHap,    @CardGUID,    @MerchantGUID,  @Clerk, @WhereFrom,     @TransType, @SalesDescription, @ErrorCode, @TransactionAmount, 0.00, @TransactionAmount, null, @SeqNum, @InvoiceNum,  @TerminalID, @LocalTime);
      SELECT @TranNumber = SCOPE_IDENTITY ();
    END;

    IF (@TransType = 'GTIP')
    BEGIN
      INSERT  INTO History (WhenHappened, CardGUID, WhichMerchantGUID, Clerk, WebCellOrDialup, TransType, TransactionText,     ErrorCode, Amount, TipAmount,    TabAmount, CardGUID2, MerchSeqNumber, InvoiceNumber, SaleTransaction, TerminalID, LocalTime)
                    VALUES (@WhenHap,    @CardGUID,    @MerchantGUID,  @Clerk, @WhereFrom,     @TransType, @SalesDescription, @ErrorCode, 0.00, @TransactionAmount, 0.00,   null,      @SeqNum,       @InvoiceNum, @OriginalSalesTran,  @TerminalID, @LocalTime);
      SELECT @TranNumber = SCOPE_IDENTITY ();
    END;

    
    -- EXECUTE gp_LogTransaction  @MerchantGUID, @Clerk, @CardToSellFrom, @WhereFrom, @TransType, @SalesDescription, @ErrorCode, @TransactionAmount, NULL, @MerchSeqNum, @InvoiceNum, @ReceiptTime OUTPUT, @TranNumber OUTPUT;


    SET @ReceiptTime = @WhenHap;
    COMMIT;



             -- Build the result set for the calling program

LeaveSubNoLog:
    SELECT @ResponseCode      AS ResponseCode, 
           @ErrorCode         AS ErrorCode, 
           @TranNumber        AS TranNumber, 
           @ReceiptTime       AS ReceiptTime, 
           @Balance           AS Balance,
           @TransactionAmount AS TransactionAmount,
           @Remainder         AS Remainder;

END
GO






  -- ***************************************************************************
  --
  -- 		G i f t   S e l l    F r o m   C a r d
  --
  -- ***************************************************************************
  --
  --    This routine returns a result set of:
  --             ResponseCode        CHAR (1)
  --             Message Indicator   CHAR (5)
  --             Transaction Number  BIGINT
  --             ReceiptTime         TIMESTAMP
  --             Balance Remaining   DECIMAL (19,2)
  --             Transaction Amount  DECIMAL (19,2)
  --             Remainder Amount    DECIMAL (19,2)

CREATE Procedure gp_GiftSellFromCard
    (
    @MerchantID        VARCHAR (46),
    @Clerk            NVARCHAR (10),
    @WhereFrom            CHAR,
    @MerchSeqNum       VARCHAR (20),
    @TerminalID        VARCHAR(10),      --  merchant terminal id
    @LocalTime        DATETIME,         --  merchant reported transaction time
    @CardToSellFrom    VARCHAR (70),
    @AmountOfSale      DECIMAL (19,2),
    @SalesDescription NVARCHAR (40),
    @InvoiceNum       NVARCHAR (15)
    )
AS
BEGIN
    EXECUTE gp_GiftSaleCommon 'SALE', @MerchantID, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardToSellFrom, @AmountOfSale, @SalesDescription, @InvoiceNum, null
END
GO

  -- ***************************************************************************
  --
  -- 		G i f t   T i p    F r o m   C a r d
  --
  -- ***************************************************************************
  --
  --    This routine returns a result set of:
  --             ResponseCode        CHAR (1)
  --             Message Indicator   CHAR (5)
  --             Transaction Number  BIGINT
  --             ReceiptTime         TIMESTAMP
  --             Balance Remaining   DECIMAL (19,2)
  --             Transaction Amount  DECIMAL (19,2)
  --             Remainder Amount    DECIMAL (19,2)

CREATE Procedure gp_GiftTipFromCard
    (
    @MerchantID        VARCHAR (46),
    @Clerk            NVARCHAR (10),
    @WhereFrom            CHAR,
    @MerchSeqNum       VARCHAR (20),
    @TerminalID        VARCHAR(10),      --  merchant terminal id
    @LocalTime        DATETIME,          --  merchant reported transaction time
    @CardToSellFrom    VARCHAR (70),
    @AmountOfTip     DECIMAL (19,2),
    @SalesDescription NVARCHAR (40),
    @InvoiceNum       NVARCHAR (15),
    @OriginalSaleTrans BIGINT
    )
AS
BEGIN
  DECLARE @ErrorCode          CHAR (5);
  DECLARE @ResponseCode       CHAR;
  DECLARE @OriginalCardGUID   UNIQUEIDENTIFIER;
  DECLARE @OriginalCard       CHAR (70);
  DECLARE @OriginalClerk      NVARCHAR(10);
  DECLARE @ErrorTranNum       BIGINT;

  IF (@OriginalSaleTrans IS NULL)
  BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'NORIG';
      GOTO LeaveError;
  END;

        -- make sure that the original sales transaction is with the same card

  ELSE
  BEGIN
    SELECT @OriginalCardGUID = CardGUID,
           @OriginalClerk = Clerk
     FROM History 
    WHERE ID= @OriginalSaleTrans;
    IF (@@ROWCOUNT=0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'NORIG';
      GOTO LeaveError;
    END

    SELECT @OriginalCard = CardNumber FROM Cards WHERE CardGUID = @OriginalCardGUID;
    IF (@@ROWCOUNT=0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'NORIG';
      GOTO LeaveError;
    END

    IF NOT @CardToSellFrom = @OriginalCard
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'NCARD';
      GOTO LeaveError;
    END

             -- if the clerk is not specified, use the clerk on the original transaction
             -- that way, a manager can specify the tips for a clerk

    IF (@Clerk = '          ')
    BEGIN
      SET @Clerk = @OriginalClerk;
    END

    EXECUTE gp_GiftSaleCommon 'GTIP', @MerchantID, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardToSellFrom, @AmountOfTip, @SalesDescription, @InvoiceNum, @OriginalSaleTrans
    GOTO LeaveSub
  END

LeaveError:
      SET @ErrorTranNum = 0;

      SELECT @ResponseCode AS ResponseCode, 
             @ErrorCode    AS ErrorCode,   
             @ErrorTranNum AS TranNumber, 
             GETDATE()     AS ReceiptTime, 
             0.00          AS Balance,
             0.00          AS TransactionAmount,
             0.00          AS Remainder;

LeaveSub:
END
GO
















GRANT EXEC ON gp_GiftSaleCommon TO PUBLIC
GO

GRANT EXEC ON gp_GiftSaleCommon TO GiftCardApp
GO


GRANT EXEC ON gp_GiftTipFromCard TO PUBLIC
GO

GRANT EXEC ON gp_GiftTipFromCard TO GiftCardApp
GO



GRANT EXEC ON gp_GiftSellFromCard TO PUBLIC
GO

GRANT EXEC ON gp_GiftSellFromCard TO GiftCardApp
GO
