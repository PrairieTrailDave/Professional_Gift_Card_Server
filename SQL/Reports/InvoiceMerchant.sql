-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_InvoiceMerchant'))
	BEGIN
		DROP  Procedure  gp_InvoiceMerchant
	END

GO



  -- ***************************************************************************
  --
  -- 			g p _ I n v o i c e M e r c h a n t
  --
  -- ***************************************************************************
  --
  --    This routine counts all transactions, approved or not
  --
  --    This routine returns a result set of:
  --             ResponseCode        char (1)
  --             Message Indicator   char (5)
  --             Receipt Time        timestamp
  --             WebTransactions         INT
  --             WebTransactionCost      DECIMAL(19,2)
  --             DialTransactions        INT
  --             DialTransactionCost     DECIMAL(19,2)
  --             CellTransactions        INT
  --             CellTransactionCost     DECIMAL(19,2)
  --             CardsShipped            INT
  --             CardCost                DECIMAL(19,2)
  --             MonthlyFee              DECIMAL(16,2)
  --             TotalCosts              DECIMAL(19,2)

CREATE Procedure gp_InvoiceMerchant
    (
    @WhichMerchant INT,
    @TrialOrFinal INT          -- values of 0 for trial, 1 for final
    )
AS
BEGIN
  DECLARE @ErrorCode            CHAR (5);
  DECLARE @ResponseCode         CHAR;
  DECLARE @MerchantID           VARCHAR(46);
  DECLARE @MerchantGUID         UNIQUEIDENTIFIER;
  DECLARE @LastBilling          DATETIME;
  DECLARE @GiftActive           CHAR;
  DECLARE @LoyaltyActive        CHAR;
  DECLARE @PremiumLoyalty       INT;

  DECLARE @IfChain              UNIQUEIDENTIFIER;
  DECLARE @PricingGUID          UNIQUEIDENTIFIER;
  DECLARE @LastInvoiceNum       BIGINT;
  DECLARE @PricePerCard         DECIMAL (16,2);
  DECLARE @PricePerWebTrans     DECIMAL (16,2);
  DECLARE @PricePerSupport      DECIMAL (16,2);
  DECLARE @PriceGiftPerMonth    DECIMAL (16,2);
  DECLARE @GiftMonthlyFee       DECIMAL (16,2);
  DECLARE @WebTransactions      INT;
  DECLARE @WebTransactionCost   DECIMAL (19,2);
  DECLARE @OtherTrans           INT;
  DECLARE @OtherNetValue        DECIMAL (19,2);
  DECLARE @GiftTransactions     INT;
  DECLARE @CardsShipped         INT;
  DECLARE @CardCost             DECIMAL (19,2);
  DECLARE @TotalCosts           DECIMAL (19,2);
  DECLARE @ReceiptTime          DATETIME;
  DECLARE @MerchPricing         INT;      --  set defaults
  DECLARE @TranNumber           BIGINT;
  DECLARE @SeqNum               INT;
  DECLARE @MerchOffset          INT;



    SET @TranNumber = 0;
    SET @ReceiptTime = GETDATE();
    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';
    SET @WebTransactions = 0;
    SET @WebTransactionCost = 0.00;
    SET @OtherTrans = 0;
    SET @OtherNetValue = 0.00;
    SET @CardsShipped = 0;
    SET @CardCost = 0.00;
    SET @TotalCosts = 0.00;
    SET @GiftMonthlyFee  = 0.00;
    SET @GiftTransactions = 0;

    BEGIN TRANSACTION;-- so that no transactions run while we do this

        -- validate the merchant
        -- get when the last time billing was done
        -- and get the pointers to the other tables


      SELECT @MerchantID=MerchantID, 
             @MerchantGUID=MerchantGUID,
             @LastBilling=LastBillingDate, 
             @LastInvoiceNum=LastBillingNumber, 
             @PricingGUID=PricingCol,
             @GiftActive=GiftActive
         FROM [Merchant] 
          WHERE ID = @WhichMerchant;

      IF (@@ROWCOUNT < 1)
      BEGIN
        SET @ResponseCode = 'E';
        SET @ErrorCode = 'MERID';
      END
      ELSE
      BEGIN


        SET @ReceiptTime = DATEADD(HOUR, @MerchOffset, GETDATE());


        IF @PricingGUID IS NULL
        BEGIN
          SET @ErrorCode = 'NOPRI';
        END
        ELSE
        BEGIN
          SELECT 
            @PricePerCard=CardPrice, 
            @PricePerWebTrans=TransactionPrice, 
            @PricePerSupport=SupportTransactionPrice,
            @PriceGiftPerMonth=GiftMonthlyFee
           FROM [Prices] 
             WHERE PriceGUID = @PricingGUID;

         -- get the transaction costs

          SELECT @WebTransactions=COUNT(*)
             FROM [History] 
              WHERE WhichMerchantGUID = @MerchantGUID AND  ID > @LastInvoiceNum AND  WebCellOrDialup = 'W' AND 
              ( 
              TransType = 'ACTV' OR 
              TransType = 'SALE' OR 
              TransType = 'GTIP' OR
              TransType = 'CRED' OR 
              TransType = 'DECC' OR 
              TransType = 'BALN' OR 
              TransType = 'CLOS' OR 
              TransType = 'CLSH' OR 
              TransType = 'VOID' OR 
              TransType = 'VDAC' OR 
              TransType = 'VDSL' OR 
              TransType = 'VDCR' OR 
              TransType = 'VDUP' OR 
              TransType = 'VDSH' OR 
              TransType = 'VDEC' 
            );

          SELECT @GiftTransactions=COUNT(*)
            FROM [History] 
              WHERE WhichMerchantGUID = @MerchantGUID AND  ID > @LastInvoiceNum  AND 
              ( 
               TransType = 'ACTV' OR  -- gift activate
               TransType = 'SALE' OR     -- gift sale
               TransType = 'GTIP' OR     -- gift tip
               TransType = 'CRED' OR     -- gift credit
               TransType = 'DECC' OR     -- gift deactivate
               TransType = 'BALN' OR     -- balance inquiry
               TransType = 'TRAN' OR     -- balance transfer
               TransType = 'TPUP' OR     -- gift top up
               TransType = 'CLOS' OR     -- close 
               TransType = 'CLSH' OR     -- close shift
               TransType = 'VOID' OR     -- void
               TransType = 'VDAC' OR     -- void active
               TransType = 'VDSL' OR     -- void gift sale
               TransType = 'VDCR' OR     -- void gift credit
               TransType = 'VDUP' OR     -- void gift top up
               TransType = 'VDEC' OR     -- void deactivate
               TransType = 'VDTR'        -- void balance transfer
               );

         -- get the support transaction costs

          SELECT @OtherTrans=COUNT(*)
            FROM [History] 
              WHERE WhichMerchantGUID = @MerchantGUID AND  ID > @LastInvoiceNum AND 
              ( 
              TransType = 'CUHI' OR 
              TransType = 'CUCR' OR 
              TransType = 'CUDB'
              );

          SET @WebTransactionCost = @WebTransactions * @PricePerWebTrans;
          SET @OtherNetValue = @OtherTrans * @PricePerSupport;

         -- get the number of cards shipped

          SELECT @CardsShipped=COUNT(*)
            FROM [History] 
              WHERE WhichMerchantGUID = @MerchantGUID AND 
                    ID > @LastInvoiceNum AND 
                    TransType = 'SHIP';
          SET @CardCost = @PricePerCard * @CardsShipped;


            -- get the total cost

          IF @GiftActive = 'A'
          BEGIN
            SET @GiftMonthlyFee = @PriceGiftPerMonth;
          END;
          SET @TotalCosts = @WebTransactionCost +  @CardCost + 
                @GiftMonthlyFee;

            -- say that we ran this today

          IF @TrialOrFinal = 1
          BEGIN
            UPDATE [Merchant]
              SET LastBillingDate = GETDATE(),  
                  LastBillingNumber = (SELECT MAX(ID) FROM [History]) 
              WHERE MerchantID = @MerchantID;
          END
          SET @ResponseCode = 'A';
          SET @ErrorCode = 'APP  ';
        END
      END


    COMMIT;-- TRANSACTION
              -- bulid the result set for the calling program

    SELECT 
        @ResponseCode as ResponseCode, 
        @ErrorCode as ErrorCode, 
        @ReceiptTime as ReceiptTime, 
        @WebTransactions as WebTransactions, @WebTransactionCost as WebTransactionCost, 
        @OtherTrans as OtherTransactions, @OtherNetValue as OtherTransactionCosts, 
        @CardsShipped as CardsShipped, @CardCost as CardCosts, 
        @GiftTransactions as GiftTransactionCount,
        @GiftMonthlyFee as GiftMonthlyFee,
        @TotalCosts as TotalCosts;

END
GO

GRANT EXEC ON gp_InvoiceMerchant TO PUBLIC

GO
