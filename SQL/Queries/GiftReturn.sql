-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]


IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_GiftReturn'))
	BEGIN
		DROP  Procedure  gp_GiftReturn
	END

GO


  -- ***************************************************************************
  --
  -- 			g p _ G i f t   R e t u r n
  --
  -- ***************************************************************************
  --
  --    This routine returns a result set of:
  --             ResponseCode        char (1)
  --             Message Indicator   char (5)
  --             Transaction Number  BIGINT
  --             ReceiptTime         TimeStamp
  --             Balance Remaining  decimal (19,2)

CREATE Procedure gp_GiftReturn
    (
    @Merchant        VARCHAR (46),
    @Clerk           NVARCHAR (10),
    @WhereFrom       CHAR,
    @MerchSeqNum     VARCHAR (20),
    @TerminalID      VARCHAR(10),      --  merchant terminal id
    @LocalTime       DATETIME,         --  merchant reported transaction time
    @CardNum         VARCHAR (70),
    @CreditAmount    DECIMAL (19,2),
    @WhyCredit       NVARCHAR (40),
    @InvoiceNum      NVARCHAR (15)
    )
AS
BEGIN
  DECLARE @ErrorCode       CHAR (5);
  DECLARE @ResponseCode    CHAR;
  DECLARE @CardAmount      DECIMAL (16,2);
  DECLARE @Balance         DECIMAL (16,2);
  DECLARE @CurrentBalance  DECIMAL (19,2);
  DECLARE @Actv            CHAR;
  DECLARE @ReceiptTime     DATETIME;
  DECLARE @TranNumber      BIGINT;
  DECLARE @MerchID         INT;
  DECLARE @MerchOffset     INT;
  DECLARE @MerchantGUID    UNIQUEIDENTIFIER;
  DECLARE @CardGUID        UNIQUEIDENTIFIER;


        -- set defaults

    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';
    SET @TranNumber = 0;
    SET @ReceiptTime = GETDATE();
    SET @Balance = 0.00;


        -- validate the merchant

    SET @Merchant = UPPER(@Merchant);
    SELECT @MerchID=ID, 
           @Actv=GiftActive, 
           @MerchSeqNum=LastSeqNumber, 
           @MerchantGUID=MerchantGUID
       FROM [Merchant] 
       WHERE MerchantID = @Merchant AND  GiftActive = 'A';
    IF (@@ROWCOUNT=0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MERID';
    END
    ELSE
    BEGIN

         -- see if this card exists
      SELECT @CardAmount=GiftBalance, 
             @Actv=Activated, 
             @CardGUID = CardGUID
         FROM [Cards] 
          WHERE CardNumber = @CardNum;

      IF @@ROWCOUNT > 0
      BEGIN
        IF @Actv='Y'
        BEGIN

                 -- credit this card with the credit amount

          UPDATE [Cards]
            SET GiftBalance = GiftBalance + @CreditAmount, Activated = 'Y'
            WHERE CardNumber = @CardNum;

                 -- get the values to return to the calling routine

          SELECT @Balance=GiftBalance
            FROM [Cards] 
            WHERE CardNumber = @CardNum;
 

          SET @ResponseCode = 'A';
          SET @ErrorCode = 'APP  ';

        END
        ELSE
        BEGIN
                -- card not active

          SET @ResponseCode = 'E';
          SET @ErrorCode = 'CRDAC';
        END
      END
      ELSE
      BEGIN
             -- card not found

        SET @ResponseCode = 'E';
        SET @ErrorCode = 'CRDER';

      END


               -- and log the transaction

          EXECUTE gp_LogTransaction 'CRED', @MerchantGUID, @Clerk, @WhereFrom, @MerchSeqNum, @TerminalID, @LocalTime, @CardNum, @ErrorCode, @CreditAmount, @WhyCredit, NULL, @InvoiceNum, @ReceiptTime OUTPUT, @TranNumber OUTPUT;

    END
              -- bulid the result set for the calling program
LeaveSub:
    SELECT @ResponseCode AS ResponseCode, @ErrorCode AS ErrorCode, @TranNumber AS TranNumber, @ReceiptTime AS ReceiptTime, @Balance AS Balance;

END
GO

GRANT EXEC ON gp_GiftReturn TO PUBLIC
GO
GRANT EXEC ON gp_GiftReturn TO GiftCardApp
GO