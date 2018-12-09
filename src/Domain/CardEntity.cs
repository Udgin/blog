using System;
using System.Collections.Generic;
using System.Linq;

namespace blg.Domain
{
    internal class CardEntity
    {
        public ArticleTitle ArticleTitle { get; set; }
        public string RelativePath { get; set; }

        public string SortDate {
            get {
                return ArticleTitle.Date == DateTime.MinValue ? string.Empty : ArticleTitle.Date.ToString("D");
            }
        }
    }
}