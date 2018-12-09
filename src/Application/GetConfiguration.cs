using MediatR;
using blg.Domain;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;

namespace blg.Application
{
    internal class GetConfigurationCommand : IRequest<BlogConfiguration>, ILoggedRequest
    {
        public GetConfigurationCommand(string sourceFolder)
        {
            SourceFolder = sourceFolder;
        }

        public string SourceFolder { get; }

        public string Trace() => $"{nameof(GetConfigurationCommand)}: {SourceFolder}";
    }

    internal class GetConfigurationCommandHandler : IRequestHandler<GetConfigurationCommand, BlogConfiguration>
    {
        private IValidator<BlogConfiguration> _validator;

        public GetConfigurationCommandHandler(IValidator<BlogConfiguration> validator)
        {
            _validator = validator;
        }
        public async Task<BlogConfiguration> Handle(GetConfigurationCommand request, CancellationToken cancellationToken)
        {
            var configuration = new BlogConfiguration {
                SourceFolder = request.SourceFolder
            };

            await _validator.ValidateAndThrowAsync(configuration, cancellationToken: cancellationToken);

            return configuration;
        }
    }
}