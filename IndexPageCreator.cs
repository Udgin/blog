using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace blg
{
    internal interface IIndexPageCreator
    {
        Task Create(string path, List<CardEntity> cards);
    }
    internal class IndexPageCreator : IIndexPageCreator
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBlogConfiguration _configuration;
        private readonly Lazy<string> _cardTemplate;
        private readonly Lazy<string> _indexTemplate;
        private readonly Lazy<string> _tagTemplate;

        public IndexPageCreator(
            IFileSystem fileSystem,
            IBlogConfiguration configuration
        )
        {
            _fileSystem = fileSystem;
            _configuration = configuration;
            _cardTemplate = new Lazy<string>(() => _fileSystem.ReadAllText(_configuration.CardTemplatePath));
            _indexTemplate = new Lazy<string>(() => _fileSystem.ReadAllText(_configuration.IndexTemplatePath));
            _tagTemplate = new Lazy<string>(() => _fileSystem.ReadAllText(_configuration.TagTemplatePath));
        }
        public async Task Create(string path, List<CardEntity> cards)
        {
            var fullPathForIndexPage = Path.Combine(_configuration.TargetFolder, path);
            var htmlFilePath = Path.Combine(fullPathForIndexPage, "index.html");

            var htmlCards = new List<string>();
            var tags = new Dictionary<string, int>();
            foreach (var card in cards.OrderByDescending(x => x.SortDate))
            {
                var htmlCard = _cardTemplate.Value;
                if (!card.Tags.ContainsKey("Title"))
                {
                    throw new Exception($"{card.RelativePath} doesn't have title");
                }
                htmlCard = htmlCard.Replace("{{TITLE}}", card.Tags["Title"]);
                var tagsValue = string.Empty;
                if (card.Tags.ContainsKey("Tags"))
                {
                    tagsValue = card.Tags["Tags"];
                    var cardTags = card.Tags["Tags"].Split(',').Select(x => x.Trim());
                    foreach(var cardTag in cardTags)
                    {
                        if (tags.ContainsKey(cardTag))
                        {
                            tags[cardTag]++;
                        }
                        else
                        {
                            tags[cardTag] = 1;
                        }
                    }
                }
                var date = string.Empty;
                if (card.Tags.ContainsKey("Date"))
                {
                    date = card.Tags["Date"];
                }
                htmlCard = htmlCard.Replace("{{DATE}}", date);
                htmlCard = htmlCard.Replace("{{SUBTITLE}}", tagsValue);
                htmlCard = htmlCard.Replace("{{LINK}}", card.RelativePath);
                htmlCard = htmlCard.Replace("{{IMAGE_LINK}}", card.ImageRelativePath);
                htmlCards.Add(htmlCard);
            }

            var tagsHtml = string.Empty;

            foreach (var key in tags.Keys)
            {
                var tagTemplate = _tagTemplate.Value;
                tagsHtml += tagTemplate.Replace("{{COUNT}}", tags[key].ToString()).Replace("{{NAME}}", key);
            }

            var contentOfIndex = _indexTemplate.Value.Replace("{{BODY}}", string.Join(string.Empty, htmlCards));
            var folderName = Path.GetFileNameWithoutExtension(path);
            contentOfIndex = contentOfIndex.Replace("{{TITLE}}", string.IsNullOrEmpty(folderName) ? "Main" : folderName);
            contentOfIndex = contentOfIndex.Replace("{{TAGS}}", tagsHtml);
            await _fileSystem.WriteAllTextAsync(htmlFilePath, contentOfIndex);
        }
    }
}