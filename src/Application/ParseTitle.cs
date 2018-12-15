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
                var index = line.IndexOf(":");
                if (index == -1) continue;
                var key = line.Substring(0, index);
                var value = line.Substring(index + 1).Trim();
                switch (key)
                {
                    case "Title":
                        result.Title = value;
                        break;
                    case "Date":
                        result.Date = DateTime.Parse(value);
                        break;
                    case "Tags":
                        result.Tags =
                            value
                            .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToArray();
                        break;
                    case "Publish":
                        result.Publish = Convert.ToBoolean(value);
                        break;
                    case "Script":
                        result.Script = value;
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