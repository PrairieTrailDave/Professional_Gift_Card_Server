// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using Professional_Gift_Card_System;




namespace Professional_Gift_Card_System.Models
{
    public class GiftCardBalance
    {
        public int ID;
        public int CardID;
        public Guid CardGUID;
        public String CardNumber;
        public String GiftBalance;
        public String LoyaltyPointBalance;
        public String MerchantAt;
    }



    public interface ICardRepository
    {
        // stored procedures

        gp_GiftActivateCard_Result ActivateGiftCard(
            String MerchantID, String Clerk, 
            String WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime,
            String CardToActivate, Decimal Amount, String InvoiceNumber);
        gp_GiftSellFromCard_Result GiftCardSale(
            String MerchantID, String Clerk, 
            String WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime,
            String CardNumber, Decimal Amount, String InvoiceNumber, String Description);
        gp_GiftReturn_Result GiftCardReturn(String MerchantID, String Clerk,
            String WebOrDial, String MerchantSequenceNumber,
            String TerminalID, DateTime LocalTime,
            String CardNumber, Decimal Amount, String Description, String InvoiceNumber);
        gp_GiftBalanceInq_Result GiftCardInquiry(String MerchantID, String Clerk,
            String WebOrDial, String MerchantSequenceNumber,
            String TerminalID, DateTime LocalTime,
            String CardNumber);

        // regular code transactions

        List<GiftCardBalance> GiftBalance(Guid CardGUID);

        // other transactions (CRUD)

        String AddCard(String AddThisCard, String MerchantID, String ChainName,
            String GroupCode);
        String AddCards(String CardToAdd, Int32 CountToAdd, String MerchantID, String ChainName,
            String GroupCode);
        String AddCards(String CardToAdd, String LastCardToAdd, String MerchantID, String ChainName,
            String GroupCode);
        void DeactivateCard(String CardToDeactivate);
        String FindNextCardToIssue(Guid MerchantGUID);
        Card GetCard(String CardToFind);
        Card GetCard(Int32 CardID);
        Card GetCard(Guid CardGUID);
        Card[] GetActiveCards(Guid MerchantGUID);

        int GetCardCount(String MerchantID);

        String ShipCards(String MerchantID, String ClerkID,
            String TerminalID, DateTime LocalTime,
            String CardToShip, Int32 CountToShip, String TransactionText);
        String ShipCards(String MerchantID, String ClerkID,
            String TerminalID, DateTime LocalTime,
            String CardToShip, String LastCardToShip, String TransactionText);
        void RegisterCard(int CardHolderID, Guid? CardHolderGUID, String CardToRegister);
        void UnregisterCard(Guid CardToUnregister);

        bool CardAvailableToIssue(String CardToCheck);
        bool CardAvailableToRegister(String CardToCheck);

        // support functions
        String GetCleanCard(String CardToExtract);
        List<string> Dump();

    }
    public class CardRepository : BaseDAO<Card>, ICardRepository
    {
        public CardRepository()
        {
        }
        public CardRepository(GiftEntities nGiftEntity)
        {
            GiftEntity = nGiftEntity;
        }


        #region StoredProcedureCalls

        gp_GiftActivateCard_Result ICardRepository.ActivateGiftCard(
            String MerchantID, String Clerk, 
            String WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime, 
            String CardToActivate, Decimal Amount, String InvoiceNumber)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToActivate);
            String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                gp_GiftActivateCard_Result Res = GiftEntity.gp_GiftActivateCard(MerchantID, Clerk, WebOrDial, MerchantSequenceNumber, TerminalID, LocalTime, EncryptedCardNumber, 0, Amount, InvoiceNumber).FirstOrDefault();
                return Res;
        }

        gp_GiftSellFromCard_Result ICardRepository.GiftCardSale(
            String MerchantID, String Clerk, 
            String WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime,
            String CardNumber, Decimal Amount, String InvoiceNumber, String Description)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardNumber);
            String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                gp_GiftSellFromCard_Result Res = GiftEntity.gp_GiftSellFromCard(
                    MerchantID, Clerk, WebOrDial, MerchantSequenceNumber,
                    TerminalID, LocalTime,
                    EncryptedCardNumber, Amount, Description, InvoiceNumber).FirstOrDefault();

                return Res;
        }



        gp_GiftReturn_Result ICardRepository.GiftCardReturn(
            String MerchantID, String Clerk,
            String WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime, 
            String CardNumber, Decimal Amount, String Description, String InvoiceNumber)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardNumber);
            String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                gp_GiftReturn_Result Res = GiftEntity.gp_GiftReturn(
                    MerchantID, Clerk, WebOrDial, MerchantSequenceNumber,
                    TerminalID, LocalTime,
                    EncryptedCardNumber, Amount, Description, InvoiceNumber).FirstOrDefault();

                return Res;
        }

        gp_GiftBalanceInq_Result ICardRepository.GiftCardInquiry(
            String MerchantID, String Clerk, 
            String WebOrDial, String MerchantSequenceNumber,
            String TerminalID, DateTime LocalTime,
            String CardNumber)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardNumber);
            String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                gp_GiftBalanceInq_Result Res = GiftEntity.gp_GiftBalanceInq(
                    MerchantID, Clerk, WebOrDial, MerchantSequenceNumber,
                    TerminalID, LocalTime,
                    EncryptedCardNumber).FirstOrDefault();

                return Res;
        }





        
        
        










        String ICardRepository.AddCard(String CardToAdd, String MerchantID, String ChainName,
            String GroupCode)
        {
            String Result = "ERROR";
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToAdd);
            if (CleanCardNumber.Length == 0)
                return "BDCRD";
            String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
            String CardLast4 = CleanCardNumber.Substring(CleanCardNumber.Length - 4);
            Result = AddThisCard(EncryptedCardNumber, CardLast4,
                MerchantID, ChainName, GroupCode);
                return Result;
        }
        String ICardRepository.AddCards(String CardToAdd, Int32 CountToAdd, String MerchantID, String ChainName,
            String GroupCode)
        {
            int count;

            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToAdd);
            if (CleanCardNumber.Length == 0)
                return "BDCRD";

            String Result = "ERROR";
                for (count = 0; count < CountToAdd; count++)
                {
                    String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                    String CardLast4 = CleanCardNumber.Substring(CleanCardNumber.Length - 4);
                    Result = AddThisCard(EncryptedCardNumber, CardLast4,
                        MerchantID, ChainName, GroupCode);
                    if (Result != "APP  ")
                        return Result;
                    CleanCardNumber = increment(CleanCardNumber);
                }
                return Result;
        }
        String ICardRepository.AddCards(String CardToAdd, String LastCardToAdd, String MerchantID, String ChainName,
            String GroupCode)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToAdd);
            if (CleanCardNumber.Length == 0)
                return "BDCRD";
            String CleanLastCardNumber = extractCardNumber(LastCardToAdd);
            if (CleanLastCardNumber.Length == 0)
                return "BDCRD";
            String Result = "ERROR";

            // need one more than the difference to include both start and ending number
            // if the difference is negative, error off with a card error

            int NumberToAdd = StringDiff(CleanLastCardNumber,CleanCardNumber);
            if (NumberToAdd < 0) return "BDCRD";
            NumberToAdd++;

            while (NumberToAdd > 0)
            {
                String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                String CardLast4 = CleanCardNumber.Substring(CleanCardNumber.Length - 4);
                Result = AddThisCard(EncryptedCardNumber, CardLast4,
                    MerchantID, ChainName, GroupCode);
                if (Result != "APP  ")
                    return Result;
                CleanCardNumber = increment(CleanCardNumber);
                NumberToAdd--;
            }
            return Result;
        }


        public String AddThisCard(String EncryptedCard, String CardNumberLast4,
            String MerchantID, String ChainID, String GroupCode)
        {

            Card NewCard = GiftEntity.Cards.Create();
            NewCard.CardGUID = Guid.NewGuid();
            NewCard.Activated = "N";
            NewCard.Shipped = "N";
            NewCard.CardNumber = EncryptedCard;
            NewCard.CardNumLast4 = CardNumberLast4;
            NewCard.GiftBalance = 0.00M;
            NewCard.MerchantGUID = null;
            GiftEntity.Cards.Add(NewCard);
            GiftEntity.SaveChanges();
            return "APP  ";
        }

        String ICardRepository.ShipCards(String MerchantID, String ClerkID,
            String TerminalID, DateTime LocalTime,
            String CardToShip, Int32 CountToShip, String TransactionText)
        {
            int count;
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToShip);
            if (CleanCardNumber.Length == 0)
                return "BDCRD";

                gp_ShipCard_Result Res = new gp_ShipCard_Result();

                for (count = 0; count < CountToShip; count++)
                {
                    String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                    Res = GiftEntity.gp_ShipCard(
                        MerchantID, ClerkID, 
                        TerminalID, LocalTime,
                        EncryptedCardNumber, TransactionText).FirstOrDefault();
                    if (Res.ResponseCode != "A")
                        return Res.ErrorCode;
                    CleanCardNumber = increment(CleanCardNumber);
                }
                return Res.ErrorCode;
        }
        String ICardRepository.ShipCards(String MerchantID, String ClerkID,
            String TerminalID, DateTime LocalTime,
            String CardToShip, String LastCardToShip, String TransactionText)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToShip);
            if (CleanCardNumber.Length == 0)
                return "BDCRD";
            String CleanLastCardNumber = extractCardNumber(LastCardToShip);
            if (CleanLastCardNumber.Length == 0)
                return "BDCRD";
            int NumberToShip = StringDiff(CleanLastCardNumber, CleanCardNumber);
            if (NumberToShip < 0) return "BDCRD";
            gp_ShipCard_Result Res = new gp_ShipCard_Result();
                while (NumberToShip > 0)
                {
                    String EncryptedCardNumber = GiftEncryption.Encrypt(CleanCardNumber);
                    Res = GiftEntity.gp_ShipCard(
                        MerchantID, ClerkID, 
                        TerminalID, LocalTime,
                        EncryptedCardNumber, TransactionText).FirstOrDefault();
                    if (Res.ResponseCode != "A")
                        return Res.ErrorCode;
                    CleanCardNumber = increment(CleanCardNumber);
                    NumberToShip --;
                }
                return Res.ErrorCode;
        }





        #endregion StoreProcedureCalls








        // this will always return a balance item in the list
        List<GiftCardBalance> ICardRepository.GiftBalance(Guid CardGUID)
        {
            List<GiftCardBalance> Results = new List<GiftCardBalance>();
            Card DBCard = (from c in GiftEntity.Cards
                           where c.CardGUID == CardGUID
                           select c).FirstOrDefault();
            if (DBCard == null)
            {
                GiftCardBalance tBalance = new GiftCardBalance();
                tBalance.GiftBalance = "0.00";
                tBalance.LoyaltyPointBalance = "0";
                Results.Add(tBalance);
                return (Results);
            }

            else
            {
                GiftCardBalance tBalance = new GiftCardBalance();
                tBalance.GiftBalance = DBCard.GiftBalance.ToString();
                tBalance.LoyaltyPointBalance = DBCard.LoyaltyBalance.ToString();
                tBalance.CardGUID = CardGUID;
                tBalance.CardID = DBCard.ID;
                tBalance.CardNumber = SystemConstants.Xs + DBCard.CardNumLast4;
                Results.Add(tBalance);
            }
            return (Results);
        }





        void ICardRepository.DeactivateCard(String CardToDeactivate)
        {
            String CleanCardNumber = extractCardNumber(CardToDeactivate);
            String EncryptedCard = GiftEncryption.Encrypt(CleanCardNumber);
            Card DBCard = (from c in GiftEntity.Cards
                           where c.CardNumber == EncryptedCard
                           select c).FirstOrDefault();
            DBCard.Activated = "N";
            GiftEntity.SaveChanges();
        }


        String ICardRepository.FindNextCardToIssue(Guid MerchantGUID)
        {
            InitializeConnection();
            Card DBCard = (from c in GiftEntity.Cards
                           where c.MerchantGUID == MerchantGUID && c.Activated == "N"
                           select c).FirstOrDefault();
            if (DBCard != null)
                return GiftEncryption.Decrypt(DBCard.CardNumber);
            return null;
        }


        Card ICardRepository.GetCard(String CardToFind)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToFind);
            String EncryptedCard = GiftEncryption.Encrypt(CleanCardNumber);
            Card DBCard = (from c in GiftEntity.Cards
                           where c.CardNumber == EncryptedCard
                           select c).FirstOrDefault();
            return DBCard;

        }

        Card ICardRepository.GetCard(Int32 CardID)
        {
            InitializeConnection();
            Card DBCard = (from c in GiftEntity.Cards
                           where c.ID == CardID
                           select c).FirstOrDefault();
            return DBCard;
        }

        Card ICardRepository.GetCard(Guid CardGUID)
        {
            InitializeConnection();
            Card DBCard = (from c in GiftEntity.Cards
                           where c.CardGUID == CardGUID
                           select c).FirstOrDefault();
            return DBCard;
        }



        Card[] ICardRepository.GetActiveCards(Guid MerchantGUID)
        {
            InitializeConnection();
            List<Card> Results = new List<Card>();
            var CardList = (from c in GiftEntity.Cards
                            where c.MerchantGUID == MerchantGUID && c.Activated == "Y"
                            orderby c.GiftBalance
                            select c).DefaultIfEmpty();
            return CardList.ToArray();
        }

        int ICardRepository.GetCardCount(String MerchantID)
        {
            InitializeConnection();
            int CardCount = (from c in GiftEntity.Cards
                           where c.MerchantGUID == 
                           ((from m in GiftEntity.Merchants
                            where m.MerchantID == MerchantID.ToUpper()
                            select m.MerchantGUID).FirstOrDefault())
                           select c).Count();
            return CardCount;
        }


        /// <summary>
        /// linkes the card to the cardholder
        /// </summary>
        /// <param name="CardHolderID"></param>
        /// <param name="CardToRegister"></param>
        void ICardRepository.RegisterCard(int CardHolderID, Guid? CardHolderGUID, String CardToRegister)
        {
            InitializeConnection();
            String CleanCardNumber = extractCardNumber(CardToRegister);
            String EncryptedCard = GiftEncryption.Encrypt(CleanCardNumber);
            Card DBCard = (from c in GiftEntity.Cards
                           where c.CardNumber == EncryptedCard
                           select c).FirstOrDefault();
            if (DBCard != null)
            {
                DBCard.CardHolderGUID = CardHolderGUID;
                GiftEntity.SaveChanges();
            }
        }


        /// <summary>
        /// removes the link to the cardholder
        /// </summary>
        /// <param name="CardToUnregister"></param>
        void ICardRepository.UnregisterCard(Guid CardToUnregister)
        {
            InitializeConnection();
            Card DBCard = (from c in GiftEntity.Cards
                           where c.CardGUID == CardToUnregister
                           select c).FirstOrDefault();
            if (DBCard == null) return;
            DBCard.CardHolderGUID = null;
            GiftEntity.SaveChanges();
        }






        bool ICardRepository.CardAvailableToIssue(String CardToCheck)
        {
            Card DbCard = (this as ICardRepository).GetCard(CardToCheck);
            if (DbCard == null)
                return false;
            if (DbCard.CardHolderGUID != null)
                return false;
            if (DbCard.Activated == null)
                return true;
            if (DbCard.Activated == "Y")
                return false;

            return true;
        }

        bool ICardRepository.CardAvailableToRegister(String CardToCheck)
        {
            Card DbCard = (this as ICardRepository).GetCard(CardToCheck);
            if (DbCard == null)
                return false;
            if (DbCard.CardHolderGUID != null)
                return false;

            return true;
        }



        List<String> ICardRepository.Dump()
        {
            List<String> Results = new List<string>();


            using (var GiftEntity = new GiftEntities())
            {
                var DBCard = (from c in GiftEntity.Cards
                            select c);
                foreach (Card c in DBCard)
                {
                    String CardLine = "INSERT INTO [CARDS] " +
                        "(CardGUID,CardNumber, CardNumLast4,  MerchantGUID, " +
                        "GroupCode, CardholderGUID, Shipped, Activated," +
                        "GiftBalance, LoyaltyBalance, LoyaltyLifetimePoints, LoyaltyVisits, DateShipped, DateActivated) " +
                        Environment.NewLine +
                        "Values (" +
                        "'" + c.CardGUID.ToString() + "'," +
                        "'" + c.CardNumber + "'," +
                        "'" + c.CardNumLast4 + "'," +
                        "'" + c.MerchantGUID + "'," +
                        "'" + c.CardHolderGUID + "'," +
                        "'" + c.Shipped + "'," +
                        "'" + c.Activated+ "'," +
                        "'" + c.GiftBalance + "'," +
                        "'" + c.DateShipped + "'," +
                        "'" + c.DateActivated + "'"
                        +");";

                    Results.Add(CardLine);
                }
            }

            return Results;
        }


        String ICardRepository.GetCleanCard(String CardToExtract)
        {
            return extractCardNumber(CardToExtract);
        }


        #region SupportRoutines

        /**
         * increment the decimal value in a string
         * 
         * This routine is used when dealing with card numbers
         * which can be far larger than standard integers
         * 
         */
        public static String increment(String value)
        {
            int posit;
            int carry;
            char dig;
            StringBuilder results;

            results = new StringBuilder(value);
            posit = results.Length - 1;
            carry = 1;

            while (carry > 0)
            {
                if (Char.IsDigit(results[posit]))
                {
                    dig = results[posit];
                    if (dig > '8')
                    {
                        carry = 1;
                        dig = '0';
                    }
                    else
                    {
                        carry = 0;
                        dig = (char)(dig + 1);
                    }
                    results[posit] = dig;
                }
                posit--;
                if ((posit < 0) && (carry > 0))
                {
                    results.Insert(0, '1');
                    carry = 0;
                }
            }
            return results.ToString();
        }


        int StringDiff(String Number1, String Number2)
        {
            int diff = 0;
            diff = Convert.ToInt32(SubtractBuffers(Number1, Number2));
            return diff;
        }

        /*****************************************************************
         *          S u b t r a c t B u f f e r s
         *****************************************************************
                    dest = src1 - src2

            This routine subtracts src2 from src1 as strings of digits. If either
            string contains alphabetic characters, they will be ignored so as
            to treat the numbers as integers; i.e., "$ 41.29" becomes 4129 cents.
            It puts the result into dest. dest may overlap either src1 or src2.
            If the result is more than the dest length, it is truncated
            on the left.

            This routine evaluates the sign when it exists as the first character
            in src1.
            This version subtracts the absolute value of src2 from src1. It is a
            low level tool.
          
            If a decimal point exists in the strings, both strings must have the 
            same number of decimal places. It is the responsibility of the calling
            routine to insure that.

            Enter   src1 points to string to subtract from
                    src2 points to string to subtract

            Exit    The string has been laid into dest

            Return Values:  the result string

            Test 22 - 333, 1022 - 333, and 10022 - 333.
        */



        //private bool SubtractTestSuite(ref String Results)
        //{
        //    try
        //    {

        //        Results = SubtractBuffers("1", "0");
        //        Results = SubtractBuffers("1", "1");
        //        Results = SubtractBuffers("1", "2");
        //        Results = SubtractBuffers("10", "20");
        //        Results = SubtractBuffers("-1", "2");
        //        Results = SubtractBuffers("-1", "-2");
        //        Results = SubtractBuffers("100", "99");
        //        Results = SubtractBuffers("99", "100");
        //        Results = SubtractBuffers("12345678901234567890123456", "12345678901234567890123455");
        //        //Results = SubtractBuffers("10.56", "9.25");
        //        //Results = SubtractBuffers("100.34", "1.1");
        //        //Results = SubtractBuffers("100.34", "1.");
        //        //Results = SubtractBuffers(".25", ".99");
        //    }
        //    catch (Exception Ex)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

                
        String SubtractBuffers(String src1, String src2)
        {
           int pos1;
           int pos2;
           char sign;
           int borrow;
           int sum;
           int minuend;
           int subtrahend;
           String dest;
           bool NotDone = true;
           int carry;



           // find the sign character of src1

           // move to the sign character of the first string

           pos1 = 0;
           while ((pos1 < src1.Length) &&
               (src1[pos1] != '-') &&
               (!Char.IsDigit(src1[pos1])))
               pos1++;

           if (src1[pos1] == '-')
               sign = '-';
           else
               sign = ' ';


           // and point to the sign character of the second string
           pos2 = 0;
           while ((pos2 < src2.Length) &&
               (src2[pos2] != '-') &&
               (!Char.IsDigit(src2[pos2])))
               pos2++;
           if ((src2[pos2] == '-') && (sign == ' '))
           {
               // add the unsigned value of src2

               return SimpleAddBuffers(src1, src2.Substring(pos2 + 1));
           }
           if ((src2[pos2] != '-') && (sign == '-'))
           {
               // add the two values and set as negative
               return "-" + SimpleAddBuffers(src1.Substring(pos1 + 1), src2);
           }


           // point to the end of each part 

           pos1 = src1.Length - 1;
           pos2 = src2.Length - 1;

           // do: p = src1 - src2 

           dest = "";
           borrow = 0;
           while (NotDone)
           {

               // if a decimal point exists at the same point in both strings
               // put one in the result

               if (pos1 > -1)
               {
                   if ((src1[pos1] == '.') && (src2[pos2] == '.'))
                   {
                       dest = "." + dest;
                       pos1--;
                       pos2--;
                       if ((pos1 < 0) && (pos2 < 0))
                           NotDone = false;
                       continue;
                   }
               }

               // get the arguments 

               minuend = prevdigit(src1, ref pos1);
               subtrahend = prevdigit(src2, ref pos2);

               // see if we have a borrow  

               minuend -= borrow;
               if (subtrahend > minuend)
                   borrow = 1;
               else
                   borrow = 0;
               sum = (minuend + (borrow * 10)) - subtrahend;

               // The problem is: we have to work our way across
               // src1 to the left, to find something to borrow from.
               // And, we have to retain that information. This works,
               // even if n1 is 0, and the borrow takes it to -1. */

               dest = sum.ToString() + dest;

               if ((pos1 < 0) && (pos2 < 0))
                   NotDone = false;
           }

           // if a borrow exists at the end, 
           // we need to do a two's complement of the result

           if (borrow > 0)
           {
               String NewDest = "";
               // first do the 9's complement
               for (pos1 = 0; pos1 < dest.Length; pos1++)
               {
                   if (Char.IsDigit(dest[pos1]))
                   {
                       int NewDigit = 9 - Int32.Parse(dest.Substring(pos1, 1));
                       NewDest = NewDest + NewDigit.ToString();
                   }
               }
               dest = NewDest;

               // then add one to the result

               NewDest = "";
               carry = 1;
               for (pos1 = dest.Length - 1; pos1 >= 0; pos1--)
               {
                   if (Char.IsDigit(dest[pos1]))
                   {
                       sum = Int32.Parse(dest.Substring(pos1, 1)) + carry;
                       carry = sum / 10;
                       sum = sum % 10;
                       NewDest = sum.ToString() + NewDest;
                   }
               }
               if (carry > 0) NewDest = carry.ToString() + NewDest;
               dest = NewDest;
           }

           // ensure at least one '0'

           pos1 = 0;
           if (dest[0] == '-')
               pos1 = 1;

           if (!Char.IsDigit(dest[pos1]))
               if (pos1 > 0)
                   dest = dest.Substring(0, 1) + "0" + dest.Substring(1);

           // figure out the sign

           // if was negative and no borrow, then still is negative
           if (sign == '-')
           {
               if (borrow == 0)
                   dest = "-" + dest;
               // else, was negative, but now is positive
           }
           else
           {       // was positive, but now is negative
               if (borrow > 0)
                   dest = "-" + dest;
           }

           return dest;
        }

        int prevdigit(String src, ref int pos)
        {
            int result = 0;

            if (pos > -1)
            {
                if (pos < src.Length)
                {
                    while (pos >= 0)
                    {
                        if (Char.IsDigit(src[pos]))
                        {
                            result = Int32.Parse(src.Substring(pos, 1));
                            pos--;
                            return result;
                        }
                        pos--;
                    }
                }
            }
            return result;
        }

        // simple add buffers - do the add without worrying about
        // sign, decimal points
        private String SimpleAddBuffers(String src1, String src2)
        {
            int pos1;
            int pos2;
            int sum;
            int add1;
            int add2;
            int carry;
            String dest;
            bool NotDone = true;

            dest = "";

            // point to the end of each part 

            pos1 = src1.Length - 1;
            pos2 = src2.Length - 1;
            carry = 0;

            while (NotDone)
            {
                // get the arguments 

                add1 = prevdigit(src1, ref pos1);
                add2 = prevdigit(src2, ref pos2);

                sum = add1 + add2 + carry;
                carry = sum / 10;
                sum = sum % 10;
                dest = sum.ToString() + dest;

                // see if we are done yet (at start of both strings)

                if ((pos1 < 0) && (pos2 < 0))
                    NotDone = false;
            }
            if (carry > 0) dest = carry.ToString() + dest;
            return dest;
        }



        /**
         * calculate the LUHN10 check digit for the given card number
         *  
         * @param cardNumber
         * @return char holding the valid LUHN10 check digit
         */
        public static char calculateLUHN10(String cardNumber)
        {
            int results;
            int posit;
            int multiplier;
            int partial;

            results = 0;
            multiplier = 2;
            // calculate the check sum

            posit = cardNumber.Length - 1;
            while (posit >= 0)
            {
                if (Char.IsDigit(cardNumber[posit]))
                {
                    partial = ((cardNumber[posit] - '0') * multiplier);
                    results = results + (partial / 10) + (partial % 10);
                    if (multiplier == 1)
                        multiplier = 2;
                    else
                        multiplier = 1;
                }
                posit--;
            }
            results = '0' + ((10 - (results % 10)) % 10);
            return ((char)results);
        }

        /**
         * checks the card number for a valid LUHN10 check digit
         * 
         * @param card
         * @return true if check digit matches
         */
        public static Boolean checkLUHN10(String card)
        {
            char cardDigit;
            char calculated;
            String cardNumber;

            cardNumber = extractCardNumber(card);

            cardDigit = cardNumber[cardNumber.Length - 1];
            calculated = calculateLUHN10(cardNumber.Substring(0, cardNumber.Length - 1));
            return (cardDigit == calculated);
        }
        /**
         * Pulls the card number out of a card swipe image
         * 
         * @param cardswipe
         * @return the card number
         */
        public static String extractCardNumber(String cardswipe)
        {
            StringBuilder results;
            int posit;
            int digits;

            results = new StringBuilder();

            if (cardswipe != null)
            {
                if (cardswipe.Length > 0)
                {
                    // find the card number in the string

                    posit = 0;
                    digits = 0;
                    if (cardswipe[posit] == '%') posit++;  // card start sentinel
                    if (cardswipe[posit] == 'B')  // leading track 1
                        posit++;
                    if (cardswipe[posit] == ';') posit++; // leading track 2 indicator
                    while (posit < cardswipe.Length)
                    {
                        if (Char.IsDigit(cardswipe[posit]))
                        {
                            results.Append(cardswipe[posit]);
                            posit++;
                            digits++;
                        }
                        else if (cardswipe[posit] == ' ')
                            posit++;
                        else
                            if (cardswipe[posit] == ';')
                            {
                                if (digits > 1) break;
                                else posit++;
                            }
                            else
                                break;
                    }
                }
            }

            return results.ToString();
        }






        #endregion SupportRoutines


    }


}