using MediatR;
using blg.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace blg.Application
{
    internal class GetConfigurationCommand : IRequest<BlogConfiguration>
    {
        public GetConfigurationCommand(string sourceFolder)
        {
            SourceFolder = sourceFolder;
        }

        public string SourceFolder { get; }
    }

    internal class GetConfigurationCommandHandler : IRequestHandler<GetConfigurationCommand, BlogConfiguration>
    {
        public Task<BlogConfiguration> Handle(GetConfigurationCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new BlogConfiguration(request.SourceFolder));
        }
    }
}