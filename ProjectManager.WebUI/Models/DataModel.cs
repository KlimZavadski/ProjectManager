using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ProjectManager.WebUI.Models
{
    public class NewPropertyTransfer
    {
        public String PropertyName { get; set; }
        public bool PropertyIsPublic { get; set; }
        public string PropertyType { get; set; }
        public String PropertyValues { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class ValueTransfer
    {
        public String Value { get; set; }
        public bool IsAssigned { get; set; }
    }

    public class WrapperMyString
    {
        public List<MyString> list { get; set; }

        public WrapperMyString()
        {
            list = new List<MyString>();
        }
    }

    public class MyString
    {
        public bool IsCorrect { get; set; }
        public String Value { get; set; }
        public String ErrorMessage { get; set; }
        public MyString(String msg)
        {
            if (!(new Regex(@"([a-zA-Z0-9_\.\s]+)")).IsMatch(msg))
            {
                ErrorMessage = "Input format is incorrect";
                IsCorrect = false;
            }
            else
            {
                IsCorrect = true;
            }
            Value = msg;
        }
    }
}