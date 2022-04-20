﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace VirtualAdvocate.Models
{
    public class PreviewKeyValue
    {
        public int RowId { get; set; }
        public string TemplateKey { get; set; }
        public string ParentKey { get; set; }
    }
    public class PreviewRowID
    {
        public int RowId { get; set; }
        //  public string TemplateKey { get; set; }
    }
    public class Roman
        {
        public string ToRomanNumber(int number)
        {
            StringBuilder result = new StringBuilder();
            int[] digitsValues = { 1, 4, 5, 9, 10, 40, 50, 90, 100, 400, 500, 900, 1000 };
            string[] romanDigits = { "I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD", "D", "CM", "M" };
            while (number > 0)
            {
                for (int i = digitsValues.Count() - 1; i >= 0; i--)
                    if (number / digitsValues[i] >= 1)
                    {
                        number -= digitsValues[i];
                        result.Append(romanDigits[i]);
                        break;
                    }
            }
            return result.ToString();
        }

    }
}