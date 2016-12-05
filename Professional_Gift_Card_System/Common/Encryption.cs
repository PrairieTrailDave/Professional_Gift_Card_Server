// ************************************************************
//
// Copyright (c) 2016 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Professional_Gift_Card_System.Models
{
    public class GiftEncryption
    {
        static Byte[] MyIV = { 123, 64, 84, 70, 103, 58, 93, 85, 98, 35, 117, 34, 91, 48, 84, 71 };
        static Byte[] MyKey = {82, 36, 74, 159, 53, 63, 85, 96, 55, 64, 57, 97, 59, 78, 35, 96, 
                         35, 63, 137, 35, 156, 239, 120, 229, 10, 154, 104, 227, 229, 168, 243, 59};
        public static String EncryptPassword(String Password)
        {
            if ((Password == null) || (Password.Length == 0))
                return null;

            // neeed to replace with this on next issue:

            Aes AESAlg = AesCryptoServiceProvider.Create();
            AESAlg.IV = BaseIV;
            AESAlg.Key = BaseKey;

            // Declare streams used to encrypt to an in memory array of bytes.
            MemoryStream msEncrypt = null;
            CryptoStream csEncrypt = null;
            StreamWriter swEncrypt = null;

            try
            {
                // Create a encrytor to perform the stream transform.
                ICryptoTransform encryptor =
                                 AESAlg.CreateEncryptor(AESAlg.Key, AESAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                swEncrypt = new StreamWriter(csEncrypt);

                // Write all data to the stream.
                swEncrypt.Write(Password);
            }
            catch
            {
                return "";
            }
            finally
            {
                // Close the streams.
                if (swEncrypt != null) swEncrypt.Close();
                if (csEncrypt != null) csEncrypt.Close();
                if (msEncrypt != null) msEncrypt.Close();
                // Clear the AES Managed object.
                if (AESAlg != null) AESAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.

            StringBuilder st = new StringBuilder();
            foreach (Byte bt in msEncrypt.ToArray())
            {
                st.AppendFormat("{0:x2}", bt);
            }
            //return msEncrypt.ToArray();
            return st.ToString();

        }



        public static String DecryptPassword(String EncryptedPassword)
        {
            //String DecryptedPassword;

                return "";
        }





        // Moving to use the following routines throughout the system

        static Byte[] BaseIV = { 90, 75, 58, 74, 98, 33, 104, 49, 97, 158, 87, 61, 27, 86, 40, 153 };
        static Byte[] BaseKey = {149, 248, 160, 92, 143, 164, 142, 241, 16, 30, 174, 160, 18, 4, 227, 92, 
                         43, 55, 65, 45, 81, 108, 152, 91, 52, 69, 48, 56, 87, 58, 72, 42};
        public static String Encrypt(String ToEncrypt)
        {
            if ((ToEncrypt == null) || (ToEncrypt.Length == 0))
                return null;
            // Create a new Rijndael object 
            Aes AESAlg = AesCryptoServiceProvider.Create();
            AESAlg.IV = BaseIV;
            AESAlg.Key = BaseKey;

            // Declare streams used to encrypt to an in memory array of bytes.
            MemoryStream msEncrypt = null;
            CryptoStream csEncrypt = null;
            StreamWriter swEncrypt = null;

            try
            {
                // Create a encrytor to perform the stream transform.
                ICryptoTransform encryptor =
                                 AESAlg.CreateEncryptor(AESAlg.Key, AESAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                swEncrypt = new StreamWriter(csEncrypt);

                // Write all data to the stream.
                swEncrypt.Write(ToEncrypt);
            }
            catch
            {
                return "";
            }
            finally
            {
                // Close the streams.
                if (swEncrypt != null) swEncrypt.Close();
                if (csEncrypt != null) csEncrypt.Close();
                if (msEncrypt != null) msEncrypt.Close();
                // Clear the RijndaelManaged object.
                if (AESAlg != null) AESAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.

            StringBuilder st = new StringBuilder();
            foreach (Byte bt in msEncrypt.ToArray())
            {
                st.AppendFormat("{0:x2}", bt);
            }
            //return msEncrypt.ToArray();
            return st.ToString();

        }

        public static String Decrypt(String EncryptedValue)
        {
            String DecryptedValue;

            if (EncryptedValue == null)
                return "";
            if (EncryptedValue.Length <= 1)  // need at least two characters
                return "";

            // convert the stream of numbers to byte array
            int NumberChars = EncryptedValue.Length;
            byte[] ToDecrypt = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                ToDecrypt[i / 2] = Convert.ToByte(EncryptedValue.Substring(i, 2), 16);



            // Create a new Rijndael object 
            Aes AESAlg = AesCryptoServiceProvider.Create();
            AESAlg.IV = BaseIV;
            AESAlg.Key = BaseKey;

            // Declare streams used to encrypt to an in memory array of bytes.
            MemoryStream msDecrypt = null;
            CryptoStream csDecrypt = null;
            StreamReader srDecrypt = null;

            try
            {
                // Use a decrytor to perform the stream transform.
                ICryptoTransform decryptor =
                                 AESAlg.CreateDecryptor(AESAlg.Key, AESAlg.IV);

                // Create the streams used for decryption.
                msDecrypt = new MemoryStream(ToDecrypt);
                csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream as string.
                DecryptedValue = srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                String mess = ex.Message;
                return "";
            }
            finally
            {
                // Close the streams.
                if (srDecrypt != null) srDecrypt.Close();
                if (csDecrypt != null) csDecrypt.Close();
                if (msDecrypt != null) msDecrypt.Close();
                // Clear the RijndaelAlg object.
                if (AESAlg != null) AESAlg.Clear();
            }

            // and done
            return DecryptedValue;

        }

    }
}