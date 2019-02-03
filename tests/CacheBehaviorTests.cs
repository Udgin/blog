using System;
using System.Threading;
using System.Threading.Tasks;
using blg.Application;
using FakeItEasy;
using MediatR;
using Xunit;

namespace tests
{
    public class CacheBehaviorTests
    {
        [Fact]
        public async Task Handle()
        {
            var token = new CancellationToken();
            var console = A.Fake<Action<string>>();

            var del = A.Fake<RequestHandlerDelegate<string>>();
            var cache = new CacheBehaviour(console);

            A.CallTo(() => del()).Returns(nameof(Handle));

            var res = await cache.Handle(new GetTemplateCommand(nameof(Handle)), token, del);

            Assert.Equal(nameof(Handle), res);
            A.CallTo(() => console(A<string>._)).MustHaveHappened();
        }
    }
}
