using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectManager.Domain.Entities
{
    public class Project
    {
        public Guid ProjectId { get; set; }
        public List<Prop> PropertyList;
        //public IEnumerable<Prop> list;
        public Project()
        {            
            PropertyList = new List<Prop>();            
        }
        public void doit()
        {
            //list = PropertyList.AsEnumerable();
        }
    }
    public class Prop
    {
        public Guid PropertyId { get; set; }
        public String PropertyValue { get; set; }        
    }
}
