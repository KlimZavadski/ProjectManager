using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace ProjectManager.WebUI.Models
{
    public class Filter
    {
        public List<FilterObject> FilterList { get; set; }

        public Filter()
        {
            FilterList = new List<FilterObject>();
        }

        public Filter(String filter)
        {
            int i;
            Regex regEx = new Regex(@"(%[^%]*%\s*\S+\s*[^\|&]+[\|&$]{0,2})");
            MatchCollection collection = regEx.Matches(filter);
            this.FilterList = new List<FilterObject>();
            foreach (Match col in collection)
            {
                FilterObject filterObject = new FilterObject(col.Value);
                if (filterObject.Name != null)
                {
                    this.FilterList.Add(filterObject);
                }
            }
            this.FilterList.Last().Operator = "";
        }

        public String[] GetNamesArray()
        {
            String[] namesArray = new string[FilterList.Count()];
            for (int i = 0; i < FilterList.Count(); i++)
            {
                namesArray[i] += FilterList[i].Name;
            }
            return namesArray;
        }

        public static bool IsFilterStringValid(String filterString)
        {
            //return Regex.IsMatch(filterString,
            //    @"((([\|&]{2})|$)?\s*(%[0-9a-zA-Z_]+%)\s*" +
            //    @"((\<=)|(\>=)|(!=)|(==)|(\<)|(\>))\s*" +
            //    @"(([0-9]+)|('[a-zA-Z0-9_\.\s]+')|([0-9]{1,2}\/[0-9]{1,2}\/[0-9]{1,4}))\s*)*");
            return Regex.IsMatch(filterString, @"(([\|&$]{0,2})?%[^%]*%\s*\S+\s*[^\|&]+)");
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
            if (Value.Length > 0 && Value[0] == '\'')
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
            if ((this.Name.Length == 0) || (Sign.Length == 0) || (Value.Length == 0))
            {
                this.Name = this.Sign = this.Value = this.Operator = null;
            }
        }

        private String GetValue(String input, String pattern, int start)
        {
            Regex regEx = new Regex(pattern);
            return regEx.Match(input, start).Value;
        }
    }
}