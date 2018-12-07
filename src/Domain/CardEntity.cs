using System;
using System.Collections.Generic;

namespace blg.Domain
{
    internal class CardEntity
    {
        public IDictionary<string, string> Tags { get; set; }
        public string RelativePath { get; set; }
        public DateTime SortDate
        {
            get
            {
                if (Tags.ContainsKey("Date"))
                    return DateTime.Parse(Tags["Date"]);
                return DateTime.Now;
            }
        }
    }
}