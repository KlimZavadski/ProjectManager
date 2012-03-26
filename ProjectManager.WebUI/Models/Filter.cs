using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace ProjectManager.WebUI.Models
{
    public class Filter
    {
        public FilterObject[] FilterArray { get; set; }

        public Filter()
        {
            FilterArray = new FilterObject[0];
        }

        public Filter(String filter)
        {
            Regex regEx = new Regex(@"(%[^%]*%\s*\S+\s*[^\|&]+[\|&$]{0,2})");
            MatchCollection collection = regEx.Matches(filter);
            this.FilterArray = new FilterObject[collection.Count];
            for (int i = 0; i < collection.Count; i++)
            {
                this.FilterArray[i] = new FilterObject(collection[i].Value);
            }
        }

        public String[] GetNamesArray()
        {
            String[] namesArray = new string[FilterArray.Count()];
            for (int i = 0; i < FilterArray.Count(); i++)
            {
                namesArray[i] += FilterArray[i].Name;
            }
            return namesArray;
        }

        public static bool IsFilterStringValid(String filterString)
        {
            return Regex.IsMatch(filterString,
                @"((([\|&]{2})|$)?\s*(%[0-9a-zA-Z_]+%)\s*" +
                @"((\<=)|(\>=)|(!=)|(==)|(\<)|(\>))\s*" +
                @"(([0-9]+)|('[a-zA-Z0-9_\.\s]+')|([0-9]{1,2}\/[0-9]{1,2}\/[0-9]{1,4}))\s*)*");
        }
    }

    public class FilterObject
    {
        public String Name { get; set; }
        public String Sign { get; set; }
        public String Value { get; set; }
        public String Operator { get; set; }
        private static String[] patterns = new String[] {
                                    @"(%[0-9a-zA-Z_]+%)",		// Value.
                                    @"(\<=)|(\>=)|(!=)|(==)|(\<)|(\>)",      // Sign.
                                    @"([0-9]+)|('[a-zA-Z0-9_\.\s]+')|([0-9]{1,2}\/[0-9]{1,2}\/[0-9]{1,4})",        // Value.
                                    @"([\|&]{2})"};                         // Operator.

        public FilterObject(String input)
        {
            this.Name = GetValue(input, patterns[0], 0);
            if (this.Name.Length > 1) this.Name = Name.Substring(1, Name.Length - 2);
            this.Name = Name.Replace('_', ' ');
            int length = this.Name.Length + 2;
            this.Sign = GetValue(input, patterns[1], length);
            switch (this.Sign)
            {
                case "==": this.Sign = "="; break;
                case "!=": this.Sign = "<>"; break;
            }
            length += this.Sign.Length;
            this.Value = GetValue(input, patterns[2], length);
            if (Value[0] == '\'')
            {
                this.Value = Value.Substring(1, this.Value.Length - 2);
                length += this.Value.Length + 2;
            }
            switch (GetValue(input, patterns[3], length))
            {
                case "&&": this.Operator = "INTERSECT"; break;
                case "||": this.Operator = "UNION"; break;
                case "" : this.Operator = ""; break;
            }
        }

        private String GetValue(String input, String pattern, int start)
        {
            Regex regEx = new Regex(pattern);
            return regEx.Match(input, start).Value;
        }
    }
}