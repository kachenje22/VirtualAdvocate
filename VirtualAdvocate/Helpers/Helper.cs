#region NameSpaces
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
#endregion
#region VirtualAdvocate.Helpers
namespace VirtualAdvocate.Helpers
{
    #region UserRoles
    public enum UserRoles
    {
        SuperAdmin = 1,
        AccountAdmin = 2,
        IndividualUser = 3,
        CompanyUser = 5,
        DepartmentAdmin = 6,
        DueDiligenceUser = 7

    }
    #endregion

    #region ConversionHelper
    public static class ConversionHelper
    {
        #region ConvertToDecimal
        public static decimal ToDecimal(this string decimalString)
        {
            decimal dValue = 0;
            decimal.TryParse(decimalString, out dValue);
            return dValue;
        }
        #endregion

        #region ToBoolean
        public static bool ToBoolean(this string boolString)
        {
            if (new string[] { "1", "TRUE", "T", "Y", "YES" }.Contains(boolString.ToUpper()))
                boolString = "true";
            bool bValue = false;
            bool.TryParse(boolString, out bValue);
            return bValue;
        }
        #endregion

        #region ConvertToInt
        public static int ToInteger(this string intString)
        {
            int iValue = 0;
            int.TryParse(intString, out iValue);
            return iValue;
        }
        #endregion

        #region ConvertToInt64
        public static Int64 ToInt64(this string intString)
        {
            Int64 iValue = 0;
            Int64.TryParse(intString, out iValue);
            return iValue;
        }
        #endregion

        #region ConvertToDecimal2
        public static decimal ToDecimal2(this string decimalString)
        {
            decimal dValue = -1;
            decimal.TryParse(decimalString, out dValue);
            return dValue;
        }
        #endregion

        #region ConvertToInt
        public static int ToInt2(this string intString)
        {
            int iValue = -1;
            int.TryParse(intString, out iValue);
            return iValue;
        }
        #endregion

        #region ToDate
        public static DateTime ToDate(this string dateString)
        {
            DateTime dateTime = DateTime.MinValue;
            DateTime.TryParse(dateString, out dateTime);
            if (DateTime.MinValue.Equals(dateTime))
            {
                string[] formats = new string[2] { "dd/MM/yyyy", "MM/dd/yyyy" };
                try
                {
                    dateTime = DateTime.ParseExact(dateString, formats, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal);
                }
                catch (Exception ex)
                {

                }
            }
            return dateTime;
        }
        #endregion
    }
    #endregion
}
#endregion