using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using blg.Common;
using blg.Domain;
using MediatR;

namespace blg.Application
{
    internal class CreateIndexPageCommand : IRequest<CardEntity>
    {
        public CreateIndexPageCommand(
            string sourceFolder,
            IList<CardEntity> cardEntities,
            string pathToIndexFolder)
        {
            SourceFolder = sourceFolder;
            CardEntities = cardEntities;
            PathToIndexFolder = pathToIndexFolder;
        }

        public string SourceFolder { get; }
        public IList<CardEntity> CardEntities { get; }
        public string PathToIndexFolder { get; }
    }
    internal class CreateIndexPageCommandHandler : IRequestHandler<CreateIndexPageCommand, CardEntity>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IMediator _mediator;

        public CreateIndexPageCommandHandler(IFileSystem fileSystem,
            IMediator mediator)
        {
            _fileSystem = fileSystem;
            _mediator = mediator;
        }
        public async Task<CardEntity> Handle(CreateIndexPageCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));
            var cardTemplate = await _mediator.Send(new GetTemplateCommand(configuration.CardTemplatePath));
            var tagTemplate = await _mediator.Send(new GetTemplateCommand(configuration.TagTemplatePath));
            var indexTemplate = await _mediator.Send(new GetTemplateCommand(configuration.IndexTemplatePath));

            var fullPathForIndexPage = request.PathToIndexFolder;
            var htmlFilePath = Path.Combine(fullPathForIndexPage, "index.html");

            var htmlCards = new List<string>();
            var tags = new Dictionary<string, int>();
            foreach (var card in request.CardEntities.OrderByDescending(x => x.SortDate))
            {
                var htmlCard = cardTemplate;
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
                htmlCards.Add(htmlCard);
            }

            var tagsHtml = string.Empty;

            foreach (var key in tags.Keys)
            {
                tagsHtml += tagTemplate.Replace("{{COUNT}}", tags[key].ToString()).Replace("{{NAME}}", key);
            }

            var contentOfIndex = indexTemplate.Replace("{{BODY}}", string.Join(string.Empty, htmlCards));
            var folderName = Path.GetFileNameWithoutExtension(fullPathForIndexPage);
            contentOfIndex = contentOfIndex.Replace("{{TITLE}}", string.IsNullOrEmpty(folderName) ? "Main" : folderName);
            contentOfIndex = contentOfIndex.Replace("{{TAGS}}", tagsHtml);

            await _fileSystem.WriteAllTextAsync(htmlFilePath, contentOfIndex);

            return new CardEntity {
                Tags = new Dictionary<string, string> {
                    ["Title"] = folderName
                },
                RelativePath = folderName + "/index.html"
            };
        }
    }
}