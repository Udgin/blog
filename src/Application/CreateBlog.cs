using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class CreateBlogCommand : IRequest
    {
        public CreateBlogCommand(string sourceFolder, string targetFolder)
        {
            TargetFolder = targetFolder;
            SourceFolder = sourceFolder;
        }

        public string TargetFolder { get; }
        public string SourceFolder { get; }
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

            await _mediator.Send(new CleanFolderCommand(request.TargetFolder));

            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.FolderImagePath, request.TargetFolder));
            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.PrismCSS, request.TargetFolder));
            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.PrismJS, request.TargetFolder));
            await _mediator.Send(new CopyStaticCommonResourcesCommand(configuration.Favicon, request.TargetFolder));

            await _mediator.Send(new CopyFolderCommand(string.Empty, request.SourceFolder, request.TargetFolder));

            return Unit.Value;
        }
    }
}