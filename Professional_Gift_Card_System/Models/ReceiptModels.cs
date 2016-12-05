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

namespace Professional_Gift_Card_System.Models
{
    public class ReceiptModels
    {
    }

    public class Receipt
    {
        public List<String> Lines;
        int PageWidth;

        public Receipt()
        {
            Lines = new List<string>();
            PageWidth = 40;
        }
        public Receipt(int width)
        {
            Lines = new List<string>();
            PageWidth = width;
        }
        public void Add(String LineContents)
        {
            if (LineContents == null) return;
            Lines.Add(LineContents);
        }
        public void AddCentered(String LineContents)
        {
            int len;

            if (LineContents == null) return;

            // figure out how many spaces to add

            if (LineContents.Length < 1)
            {
                Lines.Add(LineContents);
            }
            else
            {
                len = (PageWidth - LineContents.Length) / 2;
                Lines.Add(spaces(len) + LineContents);
            }
        }
        public String spaces(int i)
        {
            StringBuilder res;

            res = new StringBuilder();
            if (i > 0)
                while (i > 0)
                {
                    res.Append(' ');
                    i--;
                }
            return (res.ToString());
        }
        public void JustifyBoth(String value, String value2)
        {
            int len;

            // figure out how many spaces to add

            if (value2.Length < 1)
            {
                Lines.Add(value);
            }
            else
            {
                len = PageWidth - value.Length - value2.Length;
                Lines.Add(value + spaces(len) + value2);
            }
        }
        public String GetPrintableCardNumber(String CardNumber)
        {
            int len;
            StringBuilder pCard = new StringBuilder();

            CardNumber = CardRepository.extractCardNumber(CardNumber);

            // the printable card number has X's for all but the last 4
            // digits

            len = CardNumber.Length;
            if (len > 4)
            {
                while (len - 4 > 0)
                {
                    pCard = pCard.Append("X");
                    len--;
                }
                len = CardNumber.Length;
                pCard = pCard.Append(CardNumber.Substring(len - 4));
            }
            else
                pCard.Append(CardNumber);
            return (pCard.ToString());
        }

    }
       
}