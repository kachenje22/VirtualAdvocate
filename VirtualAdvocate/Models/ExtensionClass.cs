using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Models
{
    public static class ExtensionClass
    {
        public static string ReplacewithIndex(this string source, string oldValue,string newValue)
        {
            //string AllOldValue = "<" + oldValue + ">";
          
            int i = source.IndexOf(oldValue);
            if (i > 0)
            {
                i = i - 2;
                int LengthOldValue = oldValue.Length + 3;
                //string str = oldValue;

                //count yhe string length of old value including <> and then remove it by index n count parametre of remove and then insert
                source = source.Remove(i, LengthOldValue);
                source = source.Insert(i, newValue);
                source = source.Replace("&l" + newValue + "gt;", "<i>" + newValue + "</i>");
            }
            else
            {
                source = source.Replace(oldValue , "<i>" + string.Empty + "</i>");
                // source = source;
            }
            return source;
        }
        public static string ReplacewithIndexforTabel(this string source, string oldValue, string newValue)
        {
            //string AllOldValue = "<" + oldValue + ">";

            int i = source.IndexOf(oldValue);
            if (i > 0)
            {
                i = i - 2;
                int LengthOldValue = oldValue.Length + 3;
                //string str = oldValue;

                //count yhe string length of old value including <> and then remove it by index n count parametre of remove and then insert
                source = source.Remove(i, LengthOldValue);
                source = source.Insert(i, newValue);
                source = source.Replace("&l" + newValue + "gt;", "<i>" + newValue + "</i>");
            }
            else
            {
                source = source.Replace(oldValue, "<i>" + string.Empty + "</i>");
                // source = source;
            }
            return source;
        }
    }
}