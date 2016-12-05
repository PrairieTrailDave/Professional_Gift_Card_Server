-- ***************************************************************************
--
--  Copyright (c) 2016 Prairie Trail Software, Inc.
--  All rights reserved.
--
-- ***************************************************************************



USE [Gift]




IF EXISTS (SELECT     *
           FROM         sys.sysobjects
           WHERE     (type = 'P') AND (name = 'gp_ValidateMerchant'))
	BEGIN
		DROP  Procedure  gp_ValidateMerchant
	END

GO



  -- ***************************************************************************
  --
  -- 			V a l i d a t e  M e r c h a n t
  --
  -- ***************************************************************************
  --
  -- The merchant ID is always upper case in this database (so that terminals
  -- can run with the same merchant id)
  --
  -- This routine returns Found = ID if found, -1 if not found
  --                      offset = hourly offset for time calculations

  -- This function is called from most transactions. However, a few places
  -- had to use custom code. If you modify this function, also check out
  --   DepleteAllCards

CREATE Procedure gp_ValidateMerchant
    (
    @MID           VARCHAR (46),
    @MerchID       INT OUTPUT,
    @SeqNum        BIGINT OUTPUT,
    @MerchantGUID  UNIQUEIDENTIFIER OUTPUT
    )
AS
BEGIN
  DECLARE @Actv     CHAR;
  DECLARE @TZone    VARCHAR (15);
  DECLARE @LastSeq  BIGINT;      --  this should prevent SQL injection



    SET @MID = REPLACE(@MID, '''', '''''');
    IF (CHARINDEX(';', @MID) > 0)
    BEGIN
      SET @MID = SUBSTRING(@MID, 1, CHARINDEX(';', @MID) - 1);
    END
    SET @MID = UPPER(@MID);
    SELECT @MerchID=ID, @Actv=GiftActive, @LastSeq=LastSeqNumber, @MerchantGUID=MerchantGUID
       FROM [Merchant] 
        WHERE MerchantID = @MID AND  (GiftActive = 'A' );
    IF (@@ROWCOUNT=0)
    BEGIN
      SET @MerchID = -1;
    END
    ELSE
    BEGIN
      SET @SeqNum = @LastSeq;
    END

END
GO

GRANT EXEC ON gp_ValidateMerchant TO PUBLIC

GO
