// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Professional_Gift_Card_System;

namespace Professional_Gift_Card_System.Models
{
    #region Services

    public interface ICardService
    {
        String AddCard(String CardToAdd, String Merchant, String ChainName,
            String GroupCode);
        String AddCards(String CardToAdd, Int32 CountToAdd, String Merchant, String ChainName,
            String GroupCode);
        String AddCards(String CardToAdd, String LastCardToAdd, String Merchant, String ChainName,
            String GroupCode);
        String AddAndShipBatch(String Merchant, String Batch);
        String ShipCards(String MerchantID, String ClerkID, String CardToShip, Int32 CountToShip, String TransactionText);
        String ShipCards(String MerchantID, String ClerkID, String CardToShip, String LastCardToShip, String TransactionText);
        Card Getcard(String CardToFind);

        bool CardAvailableToIssue(String CardToCheck);
        bool CardAvailableToRegister(String CardToCheck);

    }

    public class CardService : ICardService
    {
        //private Random Rndm;

        String ICardService.AddCard(String CardToAdd, String Merchant, String ChainName,
            String GroupCode)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.AddCard(CardToAdd, Merchant, ChainName, GroupCode));
        }
        String ICardService.AddCards(String CardToAdd, Int32 CountToAdd, String Merchant, String ChainName,
            String GroupCode)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.AddCards(CardToAdd, CountToAdd, Merchant, ChainName, GroupCode));
        }

        String ICardService.AddCards(String CardToAdd, String LastCardToAdd, String Merchant, String ChainName,
            String GroupCode)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.AddCards(CardToAdd, LastCardToAdd, Merchant, ChainName, GroupCode));
        }

        String ICardService.AddAndShipBatch(String Merchant, String Batch)
        {
            // before we get here, the validation has made sure that only allowed chars are in the batch

            String CardToAdd;
            int pos;
            String Result = SystemConstants.ApprovedResult;

            ICardRepository CardData = new CardRepository();
            pos = 0;

            while (pos < Batch.Length)
            {
                CardToAdd = NextCardInBatch(ref Batch, ref pos);
                if (CardToAdd.Length < 1) return Result;
                Result = CardData.AddCards(CardToAdd, 1, Merchant, "", "");
                if (Result != SystemConstants.ApprovedResult)
                    break;

                Result = CardData.ShipCards(Merchant, "", 
                    "", DateTime.Now,
                    CardToAdd, 1, "");
                if (Result != SystemConstants.ApprovedResult)
                    break;
            }

            return (Result);

        }

        String NextCardInBatch(ref String Batch, ref int pos)
        {
            StringBuilder Card = new StringBuilder();

            // skip any separators
            while (pos < Batch.Length)
            {
                if (Batch[pos] == '\r') pos++;
                else
                    if (Batch[pos] == '\n') pos++;
                    else
                        if (Batch[pos] == '\t') pos++;
                        else
                            break;
            }
            while (pos < Batch.Length)
            {
                if (Char.IsDigit(Batch[pos]))
                    Card.Append(Batch[pos]);
                if (Batch[pos] == '\r') break;
                if (Batch[pos] == '\n') break;
                if (Batch[pos] == '\t') break;
                pos++;
            }
            return Card.ToString();
        }


        String ICardService.ShipCards(String MerchantID, String ClerkID, String CardToShip, Int32 CountToShip, String TransactionText)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.ShipCards(MerchantID, ClerkID, 
                "", DateTime.Now, 
                CardToShip, CountToShip, TransactionText));
        }
        String ICardService.ShipCards(String MerchantID, String ClerkID, String CardToShip, String LastCardToShip, String TransactionText)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.ShipCards(MerchantID, ClerkID, 
                "", DateTime.Now,
                CardToShip, LastCardToShip, TransactionText));
        }

        Card ICardService.Getcard(String CardToFind)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                ICardRepository CardData = new CardRepository(GiftEntity);
                return (CardData.GetCard(CardToFind));
            }
        }



        //private String GetRandomCardNumber (int NumberOfDigits)
        //{
        //    String result = "";
        //    while (NumberOfDigits > 0)
        //    {
        //        result = result + Rndm.Next(10).ToString();
        //        NumberOfDigits--;
        //    }
        //    return result;
        //}


        bool ICardService.CardAvailableToIssue(String CardToCheck)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.CardAvailableToIssue(CardToCheck));
        }
        bool ICardService.CardAvailableToRegister(String CardToCheck)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.CardAvailableToRegister(CardToCheck));
        }


    }
    #endregion Services

}