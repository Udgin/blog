using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace blg.Application
{
    internal class RequestLogger : IRequestPreProcessor<ILoggedRequest>
    {
        public Task Process(ILoggedRequest request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Trace());

            return Task.CompletedTask;
        }
    }
}