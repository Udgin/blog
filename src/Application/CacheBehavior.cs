using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class CacheBehaviour : IPipelineBehavior<GetTemplateCommand, string>
    {
        private Dictionary<string, string> _cache = new Dictionary<string, string>();
        private readonly Action<string> _console;
        public CacheBehaviour(Action<string> console)
        {
            _console = console;
        }
        public async Task<string> Handle(GetTemplateCommand request, CancellationToken cancellationToken, RequestHandlerDelegate<string> next)
        {
            if (_cache.ContainsKey(request.Path))
                return _cache[request.Path];

            var response = await next();

            _console("Missed cache");

            _cache[request.Path] = response;
            return response;
        }
    }
}