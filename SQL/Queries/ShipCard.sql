-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_ShipCard'))
	BEGIN
		DROP  Procedure  gp_ShipCard
	END

GO



  -- ***************************************************************************
  --
  -- 			S h i p  C a r d
  --
  -- ***************************************************************************
  --
  --    This routine returns a result set of:
  --             ResponseCode        char (1)
  --             Message Indicator   char (5)
  --             Transaction Number  BIGINT
  --             ReceiptTime         TimeStamp

-- ----------------------------------------------------
    -- may want to have a test to see if this merchant
    -- belongs to the chain that the card is assigned to
    -- need the proper error message
       -- SELECT ChainID=ChainID
       --       FROM Cards
       --       WHERE CardNumber=CardNumber
       -- SELECT TestChain=ChainID
       --       FROM Merchant
       --       WHERE MerchantID=MerchantID
       -- IF ChainID <> TestChain THEN
       --   BEGIN
       --     SELECT DefaultCh = MIN(ID) from Chains
       --     IF ChainID <> DefaultCh THEN
       --       BEGIN
       --         ROLLBACK TRANSACTION ShipCardRecord;
       --         SET rc = 1;
       --         SET ResponseCode = 'E';
       --         CALL gp_GetMessage 'MERCH', Language, Message OUTPUT;
       --         GOTO OnExit;
       --     END IF;
       -- END IF;

-- -----------------------------------------------------------------

CREATE Procedure gp_ShipCard
    (
    @Merchant       VARCHAR (46),
    @Clerk         NVARCHAR (10),      --  clerk that did the transaction
    @TerminalID      VARCHAR(10),      --  merchant terminal id
    @LocalTime          DATETIME,         --  merchant reported transaction time
    @CardShipped    VARCHAR (70),
    @TransText     NVARCHAR (40)      --  transaction description
    )
AS
BEGIN
  DECLARE @TestMerchant    INT;
  DECLARE @DefaultCh       INT;
  DECLARE @ChainID         INT;
  DECLARE @TestChain       INT;
  DECLARE @TestActive      CHAR;
  DECLARE @TestShipped     CHAR;
  DECLARE @ErrorCode       CHAR (5);
  DECLARE @ResponseCode    CHAR;
  DECLARE @ReceiptTime     DATETIME;
  DECLARE @WhichCard       INT;
  DECLARE @WhereFrom       CHAR;      --  set the defaults
  DECLARE @TranNumber      BIGINT;
  DECLARE @MerchID         INT;
  DECLARE @SeqNum          INT;
  DECLARE @MerchOffset     INT;
  DECLARE @MerchantGUID    UNIQUEIDENTIFIER;



    SET @TranNumber = 0;
    SET @ReceiptTime = GETDATE();
    SET @ResponseCode = 'E';
    SET @ErrorCode = 'HSTER';
    SET @WhereFrom = 'W';

  -- make sure that the merchant id exists

    EXECUTE gp_ValidateMerchant @Merchant, @MerchID OUTPUT, @SeqNum OUTPUT, @MerchantGUID OUTPUT;
    IF (@MerchID < 0)
    BEGIN
      SET @ResponseCode = 'E';
      SET @ErrorCode = 'MERID';
    END
    ELSE
    BEGIN

      -- check to see if this card is already activated
      -- it doesn't matter who this card is shipped to

      SELECT TOP(1) @WhichCard=ID, @TestShipped=Shipped, @TestActive=Activated
         FROM [Cards] 
          WHERE CardNumber = @CardShipped;

      IF (@@ROWCOUNT=0) OR (@TestActive IS NULL)
      BEGIN

        -- card not found
        SET @ResponseCode = 'E';
        SET @ErrorCode = 'CRDER';
      END
      ELSE
      BEGIN
        IF @TestActive='Y'
        BEGIN

        -- card already active
          SET @ResponseCode = 'E';
          SET @ErrorCode = 'CRDAL';
        END
        ELSE
        BEGIN

          -- see if the card is already shipped
          IF (@TestShipped='Y')
          BEGIN
            -- card already sent to a Merchant
            SET @ResponseCode = 'E';
            SET @ErrorCode = 'CRDAS';
          END
          ELSE
          BEGIN

                  -- actually say that the merchant has these cards
            UPDATE [Cards]
                SET MerchantGUID = @MerchantGUID, Shipped = 'Y',  DateShipped = GETDATE()
                WHERE CardNumber = @CardShipped AND  ID = @WhichCard;
            SET @ResponseCode = 'A';
            SET @ErrorCode = 'APP  ';
          END
        END
      END
      EXECUTE gp_LogTransaction 'SHIP', @MerchantGUID, @Clerk, @WhereFrom, 1, @TerminalID, @LocalTime, @CardShipped, @ErrorCode, '0.00', @TransText, NULL,  '', @ReceiptTime OUTPUT, @TranNumber OUTPUT;
    END
    SELECT @ResponseCode AS ResponseCode, @ErrorCode AS ErrorCode, @TranNumber AS TranNumber, @ReceiptTime AS ReceiptTime;
END
GO

GRANT EXEC ON gp_ShipCard TO PUBLIC

GO
GRANT EXEC ON gp_ShipCard TO GiftCardApp

GO