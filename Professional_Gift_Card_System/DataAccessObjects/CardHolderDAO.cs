// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using Professional_Gift_Card_System;

namespace Professional_Gift_Card_System.Models
{
    // I wanted to have the encoding closer to the database, 
    // but to do that, I would need to make this inherit the membership class

    interface ICardHolderRepository
    {
        bool CreateCardHolder(CreateCardHolderModel WebData);
        EditCardHolderModel GetWebCardHolder(String CardHolderName);
        CardHolder GetCardHolder(int ID);
        CardHolder GetCardHolder(string CardHolderName);
        CardHolder GetCardHolderByPhoneNumber(string PhoneNumber);
        CardHolder GetCardHolderByCardGUID(Guid CardGUID);
        String GetCardHolderName(String email);
        void DeactivateCardHolder(int ID);
        Int32 GetCardHolderID(String CardHolderName);
        bool DeleteCardHolder(string CardHolderName);
        bool GetCardForTransaction(String MerchantID, String PhoneNumber, out String CardNumber);
        int GetCardHolderCount();
        int GetCardCount(String CardHolderName);
//        List<CardHolder> GetCardHolderPage(int pageNumber, int pageSize);
        List<CardHolder> ListCardHolders();
//        List<CardHolder> ListCardHoldersLike(String CardHolderName, int pageNumber, int pageSize);
//        List<CardHolder> ListCardHoldersLikeEmail(String CardHolderEmail, int pageNumber, int pageSize);
        CardHolder GetCardholderByEmail(String email);
        CardHolder RegisterThisCard(String CardHolderName, Guid NewCard);
        void UnregisterCard(String CardHolderName, Guid CardToUnregister);
        void UpdateCardHolder(EditCardHolderModel WebData, String CardHolderName);
        bool UpdateEmail(String CardHolderName, String email, String comment, bool isapproved);
        String EncryptPhone(String Phone);
        String DecryptCardHolderValue(String Value);
        String GetCleanPhone(String UglyPhone);
        List<String> GetPhoneList();

        void MoveToWebFormat(EditCardHolderModel WebData, CardHolder DBData);
//        List<String> CleanExistingPhoneList();
}

    public class CardHolderRepository : BaseDAO<CardHolder>, ICardHolderRepository
    {
        public CardHolderRepository()
        {
        }
        public CardHolderRepository(GiftEntities nGiftEntity)
        {
            GiftEntity = nGiftEntity;
        }
        #region ICardHolderRepository Members


        bool ICardHolderRepository.CreateCardHolder(CreateCardHolderModel WebData)
        {
            CardHolder DBCardHolder = new CardHolder();
            DBCardHolder.CardHolderGUID = Guid.NewGuid();
            DBCardHolder.EncryptedFirstName = GiftEncryption.Encrypt(WebData.FirstName);
            DBCardHolder.LastName = WebData.LastName;
            DBCardHolder.EncryptedLastName = GiftEncryption.Encrypt(WebData.LastName);
            DBCardHolder.EncryptedCardHolderName = GiftEncryption.Encrypt(WebData.UserName);
            DBCardHolder.EncryptedEmail = GiftEncryption.Encrypt(WebData.email.ToLower());
            DBCardHolder.EncryptedAddress1 = GiftEncryption.Encrypt(WebData.Address1);
            DBCardHolder.EncryptedAddress2 = GiftEncryption.Encrypt(WebData.Address2);
            DBCardHolder.EncryptedCity = GiftEncryption.Encrypt(WebData.City);
            if (WebData.State != null)
            DBCardHolder.State = WebData.State.ToUpper();
            DBCardHolder.EncryptedPostalCode = GiftEncryption.Encrypt(WebData.PostalCode);
            String CleanPhoneNumber = extractPhoneNumber(WebData.CellPhoneNumber);
            DBCardHolder.EncryptedPhone = GiftEncryption.Encrypt(CleanPhoneNumber);
            DBCardHolder.Card1 = WebData.Card1GUID;
            DBCardHolder.Card2 = WebData.Card2GUID;
            DBCardHolder.Card3 = WebData.Card3GUID;
            DBCardHolder.Card4 = WebData.Card4GUID;
            DBCardHolder.Card5 = WebData.Card5GUID;

            GiftEntity.CardHolders.Add(DBCardHolder);
            GiftEntity.SaveChanges();
            WebData.ID = DBCardHolder.ID;
            WebData.CardHolderGUID = (Guid)DBCardHolder.CardHolderGUID;
            return true;
        }


        CardHolder ICardHolderRepository.GetCardHolder(int ID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                 where c.ID == ID
                                 select c).FirstOrDefault();
                return DBCardHolder;
            }
        }

        EditCardHolderModel ICardHolderRepository.GetWebCardHolder(String CardHolderName)
        {
            InitializeConnection();
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);
            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                           where c.EncryptedCardHolderName == EncryptedCardHolderName
                                           select c).FirstOrDefault();
            EditCardHolderModel WebData = new EditCardHolderModel();
            if (DBCardHolder == null) return WebData;

            MoveToWebFormat(WebData, DBCardHolder);
            return WebData;
        }

        CardHolder ICardHolderRepository.GetCardHolder(String CardHolderName)
        {
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);
            using (var GiftEntity = new GiftEntities())
            {
                CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                           where c.EncryptedCardHolderName == EncryptedCardHolderName
                                           select c).FirstOrDefault();
                return DBCardHolder;
            }
        }

        CardHolder ICardHolderRepository.GetCardHolderByPhoneNumber(string PhoneNumber)
        {
            InitializeConnection();
            String CleanPhoneNumber = extractPhoneNumber(PhoneNumber);
            String EncryptedPhone = GiftEncryption.Encrypt(CleanPhoneNumber);
            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.EncryptedPhone == EncryptedPhone
                                       select c).FirstOrDefault();
            return DBCardHolder;

        }

        CardHolder ICardHolderRepository.GetCardHolderByCardGUID(Guid CardGUID)
        {
            InitializeConnection();


            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.Card1 == CardGUID
                                       select c).FirstOrDefault();
            return DBCardHolder;

        }

        Int32 ICardHolderRepository.GetCardHolderID(String CardHolderName)
        {
            if (CardHolderName == null) return 0;
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);

            using (var GiftEntity = new GiftEntities())
            {
                CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                           where c.EncryptedCardHolderName == EncryptedCardHolderName
                                           select c).FirstOrDefault();
                return DBCardHolder.ID;
            }
        }

        String ICardHolderRepository.GetCardHolderName(String email)
        {
            if (email == null) return "";
            String EncryptedEmail = GiftEncryption.Encrypt(email);

            using (var GiftEntity = new GiftEntities())
            {
                CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                           where c.EncryptedEmail == EncryptedEmail
                                           select c).FirstOrDefault();
                if (DBCardHolder == null) return "";
                return GiftEncryption.Decrypt (DBCardHolder.EncryptedCardHolderName);
            }
        }

        void ICardHolderRepository.DeactivateCardHolder(int ID)
        {
            throw new Exception("CardHolder does not have an active/deactive state");
//            using (var GiftEntity = new GiftEntities())
//            {
            //                CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
//                                 where c.ID == ID
//                                 select c).FirstOrDefault();
//
            //                if (DBCardHolder == null)
//                {
            //                    throw new Exception("Trying to update a CardHolder that does not exist");
//                }
//                GiftEntity.SaveChanges();
//            }
        }

        List<CardHolder> ICardHolderRepository.ListCardHolders()
        {
            using (var GiftEntity = new GiftEntities())
            {
                List<CardHolder> ToReturn = new List<CardHolder>();
                foreach (CardHolder ch in GiftEntity.CardHolders)
                {
                    ToReturn.Add(ch);
                }
                return ToReturn;
            }
        }

        // This is impossible to do with encrypted data

//        List<CardHolder> ICardHolderRepository.GetCardHolderPage(int pageNumber, int pageSize)
//        {
//            List<CardHolder> CardHolderList = new List<CardHolder>();
//            using (var GiftEntity = new GiftEntities())
//            {
//                var CardHolderPage = (from c in GiftEntity.CardHolders
//                             .OrderBy(o => o.CardHolderName)
//                            .Skip(pageSize * pageNumber).Take(pageSize).Select(o => o)
//                                      select c);
//                foreach (CardHolder ch in CardHolderPage)
//                    CardHolderList.Add(ch);
//            }
//            return CardHolderList;
//        }

// the problem is how to do the "like" clause when the field is encrypted
//        List<CardHolder> ICardHolderRepository.ListCardHoldersLike(String CardHolderName, int pageNumber, int pageSize)
//        {
//            using (var GiftEntity = new GiftEntities())
//            {
//                List<CardHolder> ToReturn = new List<CardHolder>();
//                foreach (CardHolder ch in (from c in GiftEntity.CardHolders
//                                           .OrderBy(o => o.CardHolderName)
//                                           .Skip(pageSize * pageNumber).Take(pageSize).Select(o => o)
//                                           where c.CardHolderName.StartsWith(CardHolderName)
//                                           select c))
//                {
//                    ToReturn.Add(ch);
//                }
//                return ToReturn;
//            }
//        }
//        List<CardHolder> ICardHolderRepository.ListCardHoldersLikeEmail(String CardHolderEmail, int pageNumber, int pageSize)
//        {
//            using (var GiftEntity = new GiftEntities())
//            {
//                List<CardHolder> ToReturn = new List<CardHolder>();
//                foreach (CardHolder ch in (from c in GiftEntity.CardHolders
//                                           .OrderBy(o => o.CardHolderName)
//                                           .Skip(pageSize * pageNumber).Take(pageSize).Select(o => o)
//                                           where c.email.StartsWith(CardHolderEmail)
//                                           select c))
//                {
//                    ToReturn.Add(ch);
//                }
//                return ToReturn;
//            }
//        }


        bool ICardHolderRepository.DeleteCardHolder(string CardHolderName)
        {
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);
            using (var GiftEntity = new GiftEntities())
            {
                CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                           where c.EncryptedCardHolderName == EncryptedCardHolderName
                                           select c).FirstOrDefault();
                if (DBCardHolder == null) return false;
                GiftEntity.CardHolders.Remove(DBCardHolder);
                GiftEntity.SaveChanges();
            }
            return true;
        }


        // returns card number
        // would like this to return the encrypted card number, 
        // but that would require all sorts of other changes
        public bool GetCardForTransaction(String MerchantID, String PhoneNumber, out String CardNumber)
        {
            CardNumber = "";
            // first get the cardholder
            InitializeConnection();
            String CleanPhoneNumber = extractPhoneNumber(PhoneNumber);
            String EncryptedPhone = GiftEncryption.Encrypt(CleanPhoneNumber);
            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.EncryptedPhone == EncryptedPhone
                                       select c).FirstOrDefault();

            // now go down the list of cards to find one that is valid for this merchant

            Card tCard;
            if (DBCardHolder == null) return false;
            if (DBCardHolder.Card1 != null)
            {
                tCard = (from c in GiftEntity.Cards
                                    where c.CardGUID == DBCardHolder.Card1
                                    select c).FirstOrDefault();
                if (tCard != null)
                {
                    Merchant tMerch = (from m in GiftEntity.Merchants
                                   where m.MerchantGUID == tCard.MerchantGUID                                     
                                   select m).FirstOrDefault();
                    if (tMerch != null)
                        if (tMerch.MerchantID == MerchantID.ToUpper())
                        {
                            CardNumber = GiftEncryption.Decrypt(tCard.CardNumber);
                            return true;
                        }
                }
            }
            if (DBCardHolder.Card2 != null)
            {
                tCard = (from c in GiftEntity.Cards
                                    where c.CardGUID == DBCardHolder.Card2
                                    select c).FirstOrDefault();
                if (tCard != null)
                {
                    Merchant tMerch = (from m in GiftEntity.Merchants
                                       where m.MerchantGUID == tCard.MerchantGUID
                                       select m).FirstOrDefault();
                    if (tMerch != null)
                        if (tMerch.MerchantID == MerchantID.ToUpper())
                        {
                            CardNumber = GiftEncryption.Decrypt(tCard.CardNumber);
                            return true;
                        }
                }
            }
            if (DBCardHolder.Card3 != null)
            {
                tCard = (from c in GiftEntity.Cards
                         where c.CardGUID == DBCardHolder.Card3
                         select c).FirstOrDefault();
                if (tCard != null)
                {
                    Merchant tMerch = (from m in GiftEntity.Merchants
                                       where m.MerchantGUID == tCard.MerchantGUID
                                       select m).FirstOrDefault();
                    if (tMerch != null)
                        if (tMerch.MerchantID == MerchantID.ToUpper())
                        {
                            CardNumber = GiftEncryption.Decrypt(tCard.CardNumber);
                            return true;
                        }
                }
            }
            if (DBCardHolder.Card4 != null)
            {
                tCard = (from c in GiftEntity.Cards
                         where c.CardGUID == DBCardHolder.Card4
                         select c).FirstOrDefault();
                if (tCard != null)
                {
                    Merchant tMerch = (from m in GiftEntity.Merchants
                                       where m.MerchantGUID == tCard.MerchantGUID
                                       select m).FirstOrDefault();
                    if (tMerch != null)
                        if (tMerch.MerchantID == MerchantID.ToUpper())
                        {
                            CardNumber = GiftEncryption.Decrypt(tCard.CardNumber);
                            return true;
                        }
                }
            }
            if (DBCardHolder.Card5 != null)
            {
                tCard = (from c in GiftEntity.Cards
                         where c.CardGUID == DBCardHolder.Card5
                         select c).FirstOrDefault();
                if (tCard != null)
                {
                    Merchant tMerch = (from m in GiftEntity.Merchants
                                       where m.MerchantGUID == tCard.MerchantGUID
                                       select m).FirstOrDefault();
                    if (tMerch != null)
                        if (tMerch.MerchantID == MerchantID.ToUpper())
                        {
                            CardNumber = GiftEncryption.Decrypt(tCard.CardNumber);
                            return true;
                        }
                }
            }

            return false;
        }



        int ICardHolderRepository.GetCardHolderCount()
        {
            int count;
            using (var GiftEntity = new GiftEntities())
            {
                count = (from c in GiftEntity.CardHolders select c).Count();
            }
            return count;
        }



        CardHolder ICardHolderRepository.GetCardholderByEmail(String email)
        {
            String EncryptedEmail = GiftEncryption.Encrypt(email);
            InitializeConnection();
            CardHolder DBCardHolder = null;
            DBCardHolder = (from c in GiftEntity.CardHolders
                            where c.EncryptedEmail == EncryptedEmail
                            select c).FirstOrDefault();
            return DBCardHolder;

        }


        int ICardHolderRepository.GetCardCount(String CardHolderName)
        {
            InitializeConnection();
            int count = 0;
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);
            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.EncryptedCardHolderName == EncryptedCardHolderName
                                       select c).FirstOrDefault();
            if (DBCardHolder == null) throw new Exception("Card holder not found");
            if (DBCardHolder.Card1 != null)
                count++;
            if (DBCardHolder.Card2 != null)
                count++;
            if (DBCardHolder.Card3 != null)
                count++;
            if (DBCardHolder.Card4 != null)
                count++;
            if (DBCardHolder.Card5 != null)
                count++;

            return count;
        }


        CardHolder ICardHolderRepository.RegisterThisCard(String CardHolderName, Guid NewCard)
        {
            int cardholderid = 0;
            InitializeConnection();
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);

            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.EncryptedCardHolderName == EncryptedCardHolderName
                                       select c).FirstOrDefault();
            if (DBCardHolder == null) throw new Exception("Card holder not found");
            cardholderid = DBCardHolder.ID;
            if (DBCardHolder.Card1 == null)
                DBCardHolder.Card1 = NewCard;
            else
                if (DBCardHolder.Card2 == null)
                    DBCardHolder.Card2 = NewCard;
                else
                    if (DBCardHolder.Card3 == null)
                        DBCardHolder.Card3 = NewCard;
                    else
                        if (DBCardHolder.Card4 == null)
                            DBCardHolder.Card4 = NewCard;
                        else
                            if (DBCardHolder.Card5 == null)
                                DBCardHolder.Card5 = NewCard;
                            else
                                throw new Exception("Already five cards registered. Can not register more");
            GiftEntity.SaveChanges();

            return DBCardHolder;
        }

        void ICardHolderRepository.UnregisterCard(String CardHolderName, Guid CardToUnregister)
        {
            InitializeConnection();
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);
            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.EncryptedCardHolderName == EncryptedCardHolderName
                                       select c).FirstOrDefault();
            if (DBCardHolder != null)
            {
                if (DBCardHolder.Card1 == CardToUnregister)
                {
                    DBCardHolder.Card1 = DBCardHolder.Card2;
                    DBCardHolder.Card2 = DBCardHolder.Card3;
                    DBCardHolder.Card3 = DBCardHolder.Card4;
                    DBCardHolder.Card4 = DBCardHolder.Card5;
                    DBCardHolder.Card5 = null;
                }
                if (DBCardHolder.Card2 == CardToUnregister)
                {
                    DBCardHolder.Card2 = DBCardHolder.Card3;
                    DBCardHolder.Card3 = DBCardHolder.Card4;
                    DBCardHolder.Card4 = DBCardHolder.Card5;
                    DBCardHolder.Card5 = null;
                }
                if (DBCardHolder.Card3 == CardToUnregister)
                {
                    DBCardHolder.Card3 = DBCardHolder.Card4;
                    DBCardHolder.Card4 = DBCardHolder.Card5;
                    DBCardHolder.Card5 = null;
                }
                if (DBCardHolder.Card4 == CardToUnregister)
                {
                    DBCardHolder.Card4 = DBCardHolder.Card5;
                    DBCardHolder.Card5 = null;
                }
                if (DBCardHolder.Card5 == CardToUnregister)
                {
                    DBCardHolder.Card5 = null;
                }

                GiftEntity.SaveChanges();

            }
        }


        void ICardHolderRepository.UpdateCardHolder(EditCardHolderModel WebData, String CardHolderName)
        {
            String EncryptedCardHolderName = GiftEncryption.Encrypt(CardHolderName);
            InitializeConnection();
            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.EncryptedCardHolderName == EncryptedCardHolderName
                                       select c).FirstOrDefault();
            if (DBCardHolder != null)
            {
                DBCardHolder.EncryptedFirstName = GiftEncryption.Encrypt(WebData.FirstName);
                DBCardHolder.LastName = WebData.LastName;
                DBCardHolder.EncryptedLastName = GiftEncryption.Encrypt(WebData.LastName);
                DBCardHolder.EncryptedAddress1 = GiftEncryption.Encrypt(WebData.Address1);
                DBCardHolder.EncryptedAddress2 = GiftEncryption.Encrypt(WebData.Address2);
                DBCardHolder.EncryptedCity = GiftEncryption.Encrypt(WebData.City);
                DBCardHolder.State = WebData.State;
                DBCardHolder.EncryptedPostalCode = GiftEncryption.Encrypt(WebData.PostalCode);
                DBCardHolder.Country = WebData.Country;
                String CleanPhoneNumber = extractPhoneNumber(WebData.CellPhoneNumber);
                DBCardHolder.EncryptedPhone = GiftEncryption.Encrypt(CleanPhoneNumber);
                DBCardHolder.EncryptedEmail = GiftEncryption.Encrypt(WebData.Email);

                GiftEntity.SaveChanges();
            }
        }

        
        bool ICardHolderRepository.UpdateEmail(String CardHolderName, String email, String comment, bool isapproved)
        {
            InitializeConnection();
            String EncryptedUserName = GiftEncryption.Encrypt(CardHolderName);
            CardHolder DBCardHolder = (from c in GiftEntity.CardHolders
                                       where c.EncryptedCardHolderName == EncryptedUserName
                                       select c).FirstOrDefault();
            if (DBCardHolder == null) return false;
            DBCardHolder.EncryptedEmail = GiftEncryption.Encrypt(email);
            GiftEntity.SaveChanges();
            return true;
        }

        String ICardHolderRepository.GetCleanPhone(String UglyPhone)
        {
            return extractPhoneNumber(UglyPhone);
        }
        String ICardHolderRepository.EncryptPhone(String Phone)
        {
            return GiftEncryption.Encrypt(extractPhoneNumber(Phone));
        }


//        List<String> ICardHolderRepository.CleanExistingPhoneList()
//        {
//            using (var GiftEntity = new GiftEntities())
//            {
//                List<String> ToReturn = new List<String>();
//                foreach (CardHolder ch in GiftEntity.CardHolders)
//                {
//                    if (ch.EncryptedPhone != null)
//                    {
//                        String Phone = DecryptValue(ch.EncryptedPhone);
//                        String Line = "<td>" + Phone + "</td><td>"  + extractPhoneNumber(Phone) + "</td>";
//                        ch.EncryptedPhone = EncryptValue(extractPhoneNumber(Phone));
//                        ToReturn.Add(Line);
//                    }
//                }
//                GiftEntity.SaveChanges();
//                return ToReturn;
//            }

        List<String> ICardHolderRepository.GetPhoneList()
        {
            using (var GiftEntity = new GiftEntities())
            {
                List<String> ToReturn = new List<String>();
                foreach (CardHolder ch in GiftEntity.CardHolders)
                {
                    if (ch.EncryptedPhone != null)
                    {
                        String Phone = GiftEncryption.Decrypt(ch.EncryptedPhone);
                        String Line = "<td>" + Phone + "</td><td>"  + extractPhoneNumber(Phone) + "</td>";
                        ToReturn.Add(Line);
                    }
                }
                return ToReturn;
            }
        }

//        bool ICardHolderRepository.EncryptExistingFile()
//        {
//            InitializeConnection();
//            var CardHolders = (from c in GiftEntity.CardHolders
//                               select c);
//            foreach (CardHolder CH in CardHolders)
//            {
//                CH.EncryptedFirstName = EncryptValue(CH.FirstName);
//                CH.EncryptedLastName = EncryptValue(CH.LastName);
//                CH.EncryptedCardHolderName = EncryptValue(CH.CardHolderName);
//                CH.EncryptedAddress1 = EncryptValue(CH.Address1);
//                CH.EncryptedAddress2 = EncryptValue(CH.Address2);
//                CH.EncryptedPhone = EncryptValue(CH.Phone);
//                CH.EncryptedEmail = EncryptValue(CH.email);
//            }
//            GiftEntity.SaveChanges();
//            return true;
//        }
//
//        bool ICardHolderRepository.VerifyEncryptExistingFile()
//        {
//            InitializeConnection();
//            var CardHolders = (from c in GiftEntity.CardHolders
//                               select c);
//            foreach (CardHolder CH in CardHolders)
//            {
//                if (CH.FirstName != null)
//                    if (DecryptValue(CH.EncryptedFirstName) != CH.FirstName)
//                    throw new Exception("First name not matching for " + CH.CardHolderName);
//                if (CH.LastName != null)
//                    if (DecryptValue(CH.EncryptedLastName) != CH.LastName)
//                    throw new Exception("Last name not matching for " + CH.CardHolderName);
//                if (CH.CardHolderName != null)
//                    if (DecryptValue(CH.EncryptedCardHolderName) != CH.CardHolderName)
//                    throw new Exception("CardHolderName not matching for " + CH.CardHolderName);
//                if (CH.Address2 != null)
//                    if (DecryptValue(CH.EncryptedAddress1) != CH.Address1)
//                    throw new Exception("Address1 not matching for " + CH.CardHolderName);
//                if (CH.Address2 != null)
//                if (DecryptValue(CH.EncryptedAddress2) != CH.Address2)
//                    throw new Exception("Address2 not matching for " + CH.CardHolderName);
//                if (CH.Phone != null)
//                    if (DecryptValue(CH.EncryptedPhone) != CH.Phone)
//                    throw new Exception("Phone not matching for " + CH.CardHolderName);
//                if (CH.email != null)
//                    if (DecryptValue(CH.EncryptedEmail) != CH.email)
//                    throw new Exception("Email not matching for " + CH.CardHolderName);
//            }
//            return true;
//        }



        String ICardHolderRepository.DecryptCardHolderValue(String Value)
        {
            return GiftEncryption.Decrypt(Value);
        }
        #endregion

        #region SupportMethods


        // To put phone numbers into a standard format
        // we allow the leading + sign,
        // remove any parans, dots, dashes, and
        // leave in an X
        private String extractPhoneNumber(String Phone)
        {
            int pos = 0;
            StringBuilder NewPhone = new StringBuilder();

            if (Phone == null) return null;
            if (Phone.Length == 0) return NewPhone.ToString();

            Phone = Phone.Trim();

            // allow leading + sign

            if (pos < Phone.Length)
            {
                if (Phone[pos] == '+')
                    NewPhone.Append('+');
            }

            // allow just digits & x

            while (pos < Phone.Length)
            {
                if (Char.IsDigit(Phone[pos]))
                    NewPhone.Append(Phone[pos]);
                if ((Phone[pos] == 'X') || (Phone[pos] == 'x'))
                    NewPhone.Append('x');
                pos++;
            }
            return NewPhone.ToString();
        }





        public void MoveToWebFormat(EditCardHolderModel WebData, CardHolder DBData)
        {
            WebData.FirstName = GiftEncryption.Decrypt(DBData.EncryptedFirstName);
            WebData.LastName = DBData.LastName;
            WebData.Address1 = GiftEncryption.Decrypt(DBData.EncryptedAddress1);
            WebData.Address2 = GiftEncryption.Decrypt(DBData.EncryptedAddress2);
            WebData.City = GiftEncryption.Decrypt(DBData.EncryptedCity);
            WebData.State = DBData.State;
            WebData.PostalCode = GiftEncryption.Decrypt(DBData.EncryptedPostalCode);
            WebData.Country = DBData.Country;
            WebData.CellPhoneNumber = GiftEncryption.Decrypt(DBData.EncryptedPhone);
            WebData.Email = GiftEncryption.Decrypt(DBData.EncryptedEmail);

        }












        #endregion SupportMethods
    }
}