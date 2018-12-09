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
                return ArticleTitle.Date == DateTime.MaxValue ? string.Empty : ArticleTitle.Date.ToString("d MMMM, yyyy");
            }
        }
    }
}