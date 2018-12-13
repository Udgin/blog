using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using blg.Domain;
using MediatR;

namespace blg.Application
{
    internal class ParseTitleCommand : IRequest<ArticleTitle>
    {
        public ParseTitleCommand(string[] lines)
        {
            Lines = lines;
        }
        public string [] Lines { get; }
    }

    internal class ParseTitleCommandHandler : IRequestHandler<ParseTitleCommand, ArticleTitle>
    {
        private const string MetaDataStart = "---";

        public Task<ArticleTitle> Handle(ParseTitleCommand request, CancellationToken cancellationToken)
        {
            var result = new ArticleTitle();
            if (request.Lines[0] != MetaDataStart) return Task.FromResult(result);
            string line;
            var i = 1;
            while ((line = request.Lines[i++]) != MetaDataStart)
            {
                var splitted = line.Split(":");
                switch (splitted[0])
                {
                    case "Title":
                        result.Title = splitted[1].Trim();
                        break;
                    case "Date":
                        result.Date = DateTime.Parse(splitted[1].Trim());
                        break;
                    case "Tags":
                        result.Tags =
                            splitted[1]
                            .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToArray();
                        break;
                    case "Publish":
                        result.Publish = Convert.ToBoolean(splitted[1].Trim());
                        break;
                    case "Script":
                        result.Script = splitted[1].Trim();
                        break;
                    default:
                        break;
                }
            }
            result.Size = i;
            return Task.FromResult(result);
        }
    }
}