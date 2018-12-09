using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class CreateBlogCommand : IRequest, ILoggedRequest
    {
        public CreateBlogCommand(string sourceFolder)
        {
            SourceFolder = sourceFolder;
        }

        public string SourceFolder { get; }

        public string Trace() => $"{nameof(CreateBlogCommand)}: {SourceFolder}";
    }
    internal class CreateBlogCommandHandler : IRequestHandler<CreateBlogCommand>
    {
        private readonly IMediator _mediator;

        public CreateBlogCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<Unit> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));

            await _mediator.Send(new CleanFolderCommand(configuration.TargetFolder));

            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.FolderImagePath, configuration.TargetFolder));
            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.PrismCSS, configuration.TargetFolder));
            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.PrismJS, configuration.TargetFolder));
            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.Favicon, configuration.TargetFolder));

            await _mediator.Send(new CopyFolderCommand(string.Empty, request.SourceFolder));

            return Unit.Value;
        }
    }
}