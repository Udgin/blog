using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using blg.Application;
using blg.Infrastructure;
using DryIoc;
using Markdig;
using MediatR;
using MediatR.Pipeline;

namespace blg
{
    class Program
    {
        static async Task Main(string[] args) =>
            await SetUpMediator().Send(
                new CreateBlogCommand(args.Length > 0 ? args[0] :
                    Directory.GetCurrentDirectory()));

        private static IMediator SetUpMediator()
        {
            var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());

            container.RegisterDelegate<ServiceFactory>(r => r.Resolve);
            container.RegisterDelegate(r => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
            container.Register<IFileSystem, FileSystem>();
            container.Register(typeof(IPipelineBehavior<GetTemplateCommand, string>), typeof(CacheBehaviour), reuse: Reuse.Singleton);
            container.RegisterMany(new []{ typeof(Mediator).Assembly }, t => t.IsInterface);
            container.RegisterMany(new []{ typeof(Program).Assembly }, t =>
            {
                return
                t.Name == typeof(IRequest).Name ||
                t.Name == typeof(IRequestPreProcessor<>).Name ||
                t.Name == typeof(IRequest<>).Name ||
                t.Name == typeof(IRequestHandler<>).Name ||
                t.Name == typeof(IRequestHandler<,>).Name;
            });

            //container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), typeof(Program).GetAssembly() }, Registrator.Interfaces);

            return container.Resolve<IMediator>();
        }
    }
}
