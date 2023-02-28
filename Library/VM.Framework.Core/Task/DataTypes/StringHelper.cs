#region Usings
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Holds helper functions for strings
    /// </summary>
    public static class StringHelper
    {
        private static readonly Regex rgViettel = new Regex(ConfigurationManager.AppSettings["ViettelPrefix"]);
        private static readonly Regex rgVinaphone = new Regex(ConfigurationManager.AppSettings["VinaphonePrefix"]);
        private static readonly Regex rgMobiFone = new Regex(ConfigurationManager.AppSettings["MobifonePrefix"]);
        private static readonly Regex rgVietnamobile = new Regex(ConfigurationManager.AppSettings["VietnamobilePrefix"]);
        private static readonly Regex rgGtel = new Regex(ConfigurationManager.AppSettings["GtelPrefix"]);

        #region Static Public Functions

        /// <summary>
        /// Calls the object's ToString function passing in the formatting
        /// </summary>
        /// <param name="Input">Input object</param>
        /// <param name="Format">Format of the output string</param>
        /// <returns>The formatted string</returns>
        public static string FormatToString(object Input, string Format)
        {
            try
            {
                if (Input == null)
                    return "";
                if (!string.IsNullOrEmpty(Format))
                {
                    return (string)StringHelper.CallMethod("ToString", Input, Format);
                }
                return Input.ToString();
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets the first x number of characters from the left hand side
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <param name="Length">x number of characters to return</param>
        /// <returns>The resulting string</returns>
        public static string Left(string Input, int Length)
        {
            try
            {
                return Input.Substring(0, Length);
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets the last x number of characters from the right hand side
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <param name="Length">x number of characters to return</param>
        /// <returns>The resulting string</returns>
        public static string Right(string Input, int Length)
        {
            try
            {
                return Input.Substring(Input.Length - Length, Length);
            }
            catch { throw; }
        }

        /// <summary>
        /// Takes the first character of an input string and makes it uppercase
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>String with the first character capitalized</returns>
        public static string ToFirstCharacterUpperCase(string Input)
        {
            try
            {
                if (string.IsNullOrEmpty(Input))
                    return null;
                char[] InputChars = Input.ToCharArray();
                for (int x = 0; x < InputChars.Length; ++x)
                {
                    if (InputChars[x] != ' ' && InputChars[x] != '\t')
                    {
                        InputChars[x] = char.ToUpper(InputChars[x]);
                        break;
                    }
                }
                return new string(InputChars);
            }
            catch { throw; }
        }

        /// <summary>
        /// Capitalizes each sentence within the string
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>String with each sentence capitalized</returns>
        public static string ToSentenceCapitalize(string Input)
        {
            try
            {
                string[] Seperator = { ".", "?", "!" };
                string[] InputStrings = Input.Split(Seperator, StringSplitOptions.None);
                for (int x = 0; x < InputStrings.Length; ++x)
                {
                    if (!string.IsNullOrEmpty(InputStrings[x]))
                    {
                        Regex TempRegex = new Regex(InputStrings[x]);
                        InputStrings[x] = ToFirstCharacterUpperCase(InputStrings[x]);
                        Input = TempRegex.Replace(Input, InputStrings[x]);
                    }
                }
                return Input;
            }
            catch { throw; }
        }

        /// <summary>
        /// Capitalizes the first character of each word
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>String with each word capitalized</returns>
        public static string ToTitleCase(string Input)
        {
            try
            {
                string[] Seperator = { " ", ".", "\t", System.Environment.NewLine, "!", "?" };
                string[] InputStrings = Input.Split(Seperator, StringSplitOptions.None);
                for (int x = 0; x < InputStrings.Length; ++x)
                {
                    if (!string.IsNullOrEmpty(InputStrings[x]))
                    {
                        Regex TempRegex = new Regex(InputStrings[x]);
                        InputStrings[x] = ToFirstCharacterUpperCase(InputStrings[x]);
                        Input = TempRegex.Replace(Input, InputStrings[x]);
                    }
                }
                return Input;
            }
            catch { throw; }
        }

        /// <summary>
        /// Checks to see if a string is a valid email address
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>True if it is a valid email address, false otherwise</returns>
        public static bool IsValidEmail(string Input)
        {
            try
            {
                if (string.IsNullOrEmpty(Input))
                    return false;
                Regex TempReg = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                      @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
                return TempReg.IsMatch(Input);
            }
            catch { throw; }
        }

        /// <summary>
        /// Checks to see if a string is a valid domain
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>True if it is a valid domain, false otherwise</returns>
        public static bool IsValidDomain(string Input)
        {
            try
            {
                if (string.IsNullOrEmpty(Input))
                    return false;
                Regex TempReg = new Regex(@"^(http|https|ftp)://([a-zA-Z0-9_-]*(?:\.[a-zA-Z0-9_-]*)+):?([0-9]+)?/?");
                return TempReg.IsMatch(Input);
            }
            catch { throw; }
        }

        /// <summary>
        /// returns the number of times a string occurs within the text
        /// </summary>
        /// <param name="Input">input text</param>
        /// <param name="Match">The string to match (can be regex)</param>
        /// <returns>The number of times the string occurs</returns>
        public static int NumberTimesOccurs(string Input, string Match)
        {
            try
            {
                Regex TempRegex = new Regex(Match);
                return TempRegex.Matches(Input).Count;
            }
            catch { throw; }
        }

        /// <summary>
        /// Reverses a string
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>The reverse of the input string</returns>
        public static string ReverseString(string Input)
        {
            try
            {
                char[] ArrayValues = Input.ToCharArray();
                Array.Reverse(ArrayValues);
                return new string(ArrayValues);
            }
            catch { throw; }
        }

        /// <summary>
        /// Removes the filter text from the input.
        /// </summary>
        /// <param name="Input">Input text</param>
        /// <param name="Filter">Regex expression of text to filter out</param>
        /// <returns>The input text minus the filter text.</returns>
        public static string FilterOutText(string Input, string Filter)
        {
            try
            {
                Regex TempRegex = new Regex(Filter);
                return TempRegex.Replace(Input, "");
            }
            catch { throw; }
        }

        /// <summary>
        /// Removes everything that is not in the filter text from the input.
        /// </summary>
        /// <param name="Input">Input text</param>
        /// <param name="Filter">Regex expression of text to keep</param>
        /// <returns>The input text minus everything not in the filter text.</returns>
        public static string KeepFilterText(string Input, string Filter)
        {
            try
            {
                Regex TempRegex = new Regex(Filter);
                MatchCollection Collection = TempRegex.Matches(Input);
                StringBuilder Builder = new StringBuilder();
                foreach (Match Match in Collection)
                {
                    Builder.Append(Match.Value);
                }
                return Builder.ToString();
            }
            catch { throw; }
        }

        /// <summary>
        /// Keeps only alphanumeric characters
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>the string only containing alphanumeric characters</returns>
        public static string AlphaNumericOnly(string Input)
        {
            try
            {
                return KeepFilterText(Input, "[a-zA-Z0-9]");
            }
            catch { throw; }
        }

        /// <summary>
        /// Keeps only alpha characters
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <returns>the string only containing alpha characters</returns>
        public static string AlphaCharactersOnly(string Input)
        {
            try
            {
                return KeepFilterText(Input, "[a-zA-Z]");
            }
            catch { throw; }
        }

        /// <summary>
        /// Keeps only numeric characters
        /// </summary>
        /// <param name="Input">Input string</param>
        /// <param name="KeepNumericPunctuation">Determines if decimal places should be kept</param>
        /// <returns>the string only containing numeric characters</returns>
        public static string NumericOnly(string Input, bool KeepNumericPunctuation)
        {
            try
            {
                if (KeepNumericPunctuation)
                {
                    return KeepFilterText(Input, @"[0-9\.]");
                }
                return KeepFilterText(Input, "[0-9]");
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a list of words that are equivalents in terms of anagrams
        /// </summary>
        /// <param name="InputArray">Array of words</param>
        /// <returns>A dictionary containing words that are equivalent anagrams,
        /// the key of the dictionary is the alphabetically sorted value for the items in the list
        /// </returns>
        public static Dictionary<string, System.Collections.Generic.List<string>> GetAnagramEquivalents(System.Collections.Generic.List<string> InputArray)
        {
            try
            {
                Dictionary<string, System.Collections.Generic.List<string>> ReturnList = new Dictionary<string, System.Collections.Generic.List<string>>();
                for (int x = 0; x < InputArray.Count; ++x)
                {
                    char[] InputCharArray = InputArray[x].ToCharArray();
                    Array.Sort(InputCharArray);
                    string InputString = new string(InputCharArray);
                    if (ReturnList.ContainsKey(InputString))
                    {
                        ReturnList[InputString].Add(InputArray[x]);
                    }
                    else
                    {
                        ReturnList.Add(InputString, new System.Collections.Generic.List<string>());
                        ReturnList[InputString].Add(InputArray[x]);
                    }
                }
                return ReturnList;
            }
            catch { throw; }
        }

        #endregion

        #region Private Static Functions

        /// <summary>
        /// Calls a method on an object
        /// </summary>
        /// <param name="MethodName">Method name</param>
        /// <param name="Object">Object to call the method on</param>
        /// <param name="InputVariables">(Optional)input variables for the method</param>
        /// <returns>The returned value of the method</returns>
        private static object CallMethod(string MethodName, object Object, params object[] InputVariables)
        {
            try
            {
                if (string.IsNullOrEmpty(MethodName) || Object == null)
                    return null;
                Type ObjectType = Object.GetType();
                MethodInfo Method = null;
                if (InputVariables != null)
                {
                    Type[] MethodInputTypes = new Type[InputVariables.Length];
                    for (int x = 0; x < InputVariables.Length; ++x)
                    {
                        MethodInputTypes[x] = InputVariables[x].GetType();
                    }
                    Method = ObjectType.GetMethod(MethodName, MethodInputTypes);
                    if (Method != null)
                    {
                        return Method.Invoke(Object, InputVariables);
                    }
                }
                Method = ObjectType.GetMethod(MethodName);
                if (Method != null)
                {
                    return Method.Invoke(Object, null);
                }
                return null;
            }
            catch { throw; }
        }

        #endregion

        #region Number mobile


        public static string GetStandardMobileNumber(string mobile)
        {
            if (mobile.StartsWith("0"))
            {
                mobile = "84" + mobile.Substring(1);
            }
            if (!mobile.StartsWith("84"))
            {
                mobile = "84" + mobile;
            }
            return mobile.Trim();
        }

        public static bool CheckFormatMobileNumber(string strMobile, string strPattern)
        {
            if (strMobile.Length > 12 || strMobile.Length < 9) return false;
            if (strMobile.StartsWith("849") && strMobile.Length != 11)
                return false;
            if (strMobile.StartsWith("841") && strMobile.Length != 12)
                return false;
            if (strMobile.StartsWith("9") && strMobile.Length > 9)
                return false;
            if (strMobile.StartsWith("1") && strMobile.Length > 10)
                return false;
            if (!CheckDigitMobile(strMobile)) return false;
            Regex rg = new Regex(strPattern);
            Match m = rg.Match(strMobile);
            return m.Success;
        }
        /// <summary>
        /// Check Vinaphone no
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public static bool CheckVinaPhoneNo(string phoneno)
        {
            return rgVinaphone.IsMatch(phoneno);
        }
        /// <summary>
        /// Check Viettel no
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public static bool CheckViettelNo(string phoneno)
        {
            return rgViettel.IsMatch(phoneno);
        }
        /// <summary>
        /// Check Viettel no
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public static bool CheckMobiFoneNo(string phoneno)
        {
            return rgMobiFone.IsMatch(phoneno);
        }
        /// <summary>
        /// Check Vietnamobile no
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public static bool CheckVietnamobileNo(string phoneno)
        {
            return rgVietnamobile.IsMatch(phoneno);
        }
        /// <summary>
        /// Check Vietnamobile no
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public static bool CheckGtelNo(string phoneno)
        {
            return rgGtel.IsMatch(phoneno);
        }
        public static bool CheckDigitMobile(string strMobile)
        {
            bool bCheck = true;
            char[] listNum = strMobile.ToCharArray();
            foreach (char c in listNum)
            {
                int num;
                if (!int.TryParse(c.ToString(), out num))
                {
                    bCheck = false;
                    break;
                }
            }
            return bCheck;
        }
        public static int CountParameter(string strTemp)
        {
            //string str = "Ngay , chung toi thong bao toi Bui Quang Phu den du bua com vo mat";
            MatchCollection wordColl =
            Regex.Matches(strTemp, @"[{][P]([1-9]|[1-9][0-9])[}]");
            //string[] arr = Regex.Split(strTemp, @"[{][P][1-9][}]", RegexOptions.None);
            //string strPattern = string.Empty;
            //#region
            ////if (arr != null && arr.Length > 0)
            ////{
            ////    for (int i = 0; i < arr.Length; i++)
            ////    {
            ////        if (i == 0)
            ////            strPattern += "[" + arr[i] + "]";
            ////        else
            ////            strPattern += "|[" + arr[i] + "]";
            ////    }
            ////}
            //#endregion
            ////if (strPattern != string.Empty)
            ////{
            //    //string[] stringSeparators = new string[] { " chung toi thong bao toi " };
            //    //string[] arrRequest = str.Split(stringSeparators, StringSplitOptions.None);
            //    string[] arrValueParam = str.Split(arr, StringSplitOptions.RemoveEmptyEntries);
            //    Regex regex = new Regex(@"\Bui Quang Phu");
            //    //arrRequest = regex.Split(str);
            ////}
            return wordColl.Count;
        }
        #endregion

    }
}
