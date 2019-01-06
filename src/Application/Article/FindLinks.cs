using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class FindLinksCommand : IRequest<IEnumerable<string>>
    {
        public FindLinksCommand(IEnumerable<string> lines)
        {
            Lines = lines;
        }
        public IEnumerable<string> Lines { get; }
    }

    internal class FindLinksCommandHandler : IRequestHandler<FindLinksCommand, IEnumerable<string>>
    {
        private readonly Regex linkRegex
            = new Regex(@"\[.+\]\((?<url>.+)\)", RegexOptions.Compiled);

        public Task<IEnumerable<string>> Handle(FindLinksCommand request, CancellationToken cancellationToken)
        {
            var result = new List<string>();
            foreach (var line in request.Lines)
            {
                foreach (Match match in linkRegex.Matches(line))
                {
                    for (int i = 0; i < match.Groups.Count; i++)
                    {
                        Group group = match.Groups[i];
                        if (group.Success && group.Name == "url")
                            result.Add(group.Value);
                    }
                }
            }
            return Task.FromResult((IEnumerable<string>)result);
        }
    }
}