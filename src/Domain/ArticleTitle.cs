using System;
using System.Collections.Generic;

namespace blg.Domain
{
    internal class ArticleTitle
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = "";
        public string[] Tags { get; set; } = new string[0];
        public string Script { get; set; } = "";
        public int Size { get; set;} = 0;
        public bool Publish { get; set; } = true;
    }
}