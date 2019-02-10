using MediatR;
using blg.Domain;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;

namespace blg.Application
{
    internal class GetConfigurationCommand : IRequest<IBlogConfiguration>, ILoggedRequest
    {
        public GetConfigurationCommand(string sourceFolder)
        {
            SourceFolder = sourceFolder;
        }

        public string SourceFolder { get; }

        public string Trace() => $"{nameof(GetConfigurationCommand)}: {SourceFolder}";
    }

    internal class GetConfigurationCommandHandler : IRequestHandler<GetConfigurationCommand, IBlogConfiguration>
    {
        private IValidator<IBlogConfiguration> _validator;

        public GetConfigurationCommandHandler(IValidator<IBlogConfiguration> validator)
        {
            _validator = validator;
        }
        public async Task<IBlogConfiguration> Handle(GetConfigurationCommand request, CancellationToken cancellationToken)
        {
            var configuration = new BlogConfiguration {
                SourceFolder = request.SourceFolder
            };

            await _validator.ValidateAndThrowAsync(configuration, cancellationToken: cancellationToken);

            return configuration;
        }
    }
}