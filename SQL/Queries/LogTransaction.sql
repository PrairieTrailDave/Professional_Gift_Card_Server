-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_LogTransaction'))
	BEGIN
		DROP  Procedure  gp_LogTransaction
	END

GO



  -- ***************************************************************************
  --
  -- 			g p _ L o g T r a n s a c t i o n
  --
  -- ***************************************************************************

CREATE Procedure gp_LogTransaction
    (
    @TransType       VARCHAR (4),      --  transaction type
    @MerchGUID       UNIQUEIDENTIFIER, --  which merchant 
    @Clerk           NVARCHAR (10),    --  clerk that did the transaction
    @WhereFrom       CHAR,             --  where the transaction happened
    @SeqNum          BIGINT,           --  merchant sequence number
    @TerminalID      VARCHAR(10),      --  merchant terminal id
    @LocalTime       DATETIME,         --  merchant reported transaction time
    @CardNum         VARCHAR (70),     --  card or cell number
    @ErrorCode       CHAR(5),          --  error code for this transaction
    @Amount          DECIMAL (19,4),   --  transaction amount
    @TransText       NVARCHAR (40),    --  transaction description
    @NewCard         VARCHAR (70),     --  second card for transfers
    @InvoiceNum      VARCHAR(15),      --  merchant invoice number
    @WhenHap         DATETIME OUTPUT,  -- when the transaction happened
    @SystemTransactionNumber BIGINT OUTPUT    
    )
AS
BEGIN
  DECLARE @CardGUID   UNIQUEIDENTIFIER;
  DECLARE @CardGUID2  UNIQUEIDENTIFIER;
  


             -- then get the card id to store in the log file

    SET @CardGUID = NULL;
    IF @CardNum IS NOT NULL
    BEGIN
      SELECT @CardGUID=CardGUID
         FROM [Cards] 
          WHERE CardNumber = @CardNum;
          -- tried to include merchant id as a constraint, but that was a bug
          -- as cards can be by group or chain as well
    END
        --  get the first card in the file for those transactions that don't have a card

    IF @CardGUID IS NULL
    BEGIN
       SELECT TOP(1) @CardGUID=CardGUID
          FROM [Cards]  ORDER BY ID ;
    END

              -- if there is a second card, get it's id also

    SET @CardGUID2 =  NULL ;
    IF @NewCard IS NOT NULL
    BEGIN
      SELECT @CardGUID2 = CardGUID
         FROM [Cards] 
          WHERE CardNumber = @NewCard;
    END


              -- get when this transaction happened

    SET @WhenHap = GETDATE();

              -- and store then all in the table

    INSERT  INTO History (WhenHappened, CardGUID, WhichMerchantGUID, Clerk, WebCellOrDialup, TransType, TransactionText, ErrorCode, Amount, CardGUID2, MerchSeqNumber, InvoiceNumber, TerminalID, LocalTime)
                  VALUES (@WhenHap,    @CardGUID,    @MerchGUID,    @Clerk, @WhereFrom,     @TransType, @TransText,    @ErrorCode, @Amount, @CardGUID2, @SeqNum,        @InvoiceNum,  @TerminalID, @LocalTime);
    SELECT @SystemTransactionNumber = SCOPE_IDENTITY ();
      
END
GO

GRANT EXEC ON gp_LogTransaction TO PUBLIC

GO
GRANT EXEC ON gp_LogTransaction TO GiftCardApp

GO


