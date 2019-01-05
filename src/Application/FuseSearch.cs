using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using blg.Domain;
using MediatR;

namespace blg.Application
{
    internal class FuseSearchCommand : IRequest
    {
        public FuseSearchCommand(IEnumerable<ArticleTitle> cards, string outputPath)
        {
            Cards = cards;
            OutputPath = outputPath;
        }
        public IEnumerable<ArticleTitle> Cards { get; }
        public string OutputPath { get; }
    }

    internal class FuseSearchCommandHandler : IRequestHandler<FuseSearchCommand>
    {
        private IFileSystem _fileSystem;
        private IMediator _mediator;

        public FuseSearchCommandHandler(IFileSystem fileSystem, IMediator mediator)
        {
            _fileSystem = fileSystem;
            _mediator = mediator;
        }
        public async Task<Unit> Handle(FuseSearchCommand request, CancellationToken cancellationToken)
        {
            var fuseFilePath = Path.Combine(request.OutputPath, "fuse.js");

            var js = new StringBuilder();

            js.AppendLine("var articles = [");
            foreach (var card in request.Cards)
            {
                js.AppendLine("{");
                js.AppendLine($"  'Title': \"{card.Title}\",");
                js.AppendLine($"  'Description': \"{card.Description}\",");
                js.AppendLine($"  'Tags': [{string.Join(',', card.Tags.Select(x => $"\"{x}\""))}]");
                js.AppendLine("},");
            }
            js.AppendLine("];");

            var uglified = await _mediator.Send(new UglifyHtmlCommand(js.ToString()));

            await _fileSystem.WriteAllTextAsync(fuseFilePath, uglified);

            return Unit.Value;
        }
    }
}