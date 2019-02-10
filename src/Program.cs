using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using blg.Application;
using blg.Domain;
using blg.Infrastructure;
using DryIoc;
using FluentValidation;
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

            container.Register<IFileSystem, FileSystem>();
            container.Register<IBlogConfiguration, BlogConfiguration>();
            container.RegisterDelegate<Action<string>>((r) => Console.WriteLine);

            container.RegisterMany(new []{ typeof(Mediator).Assembly }, t => t.IsInterface);
            container.RegisterDelegate<ServiceFactory>(r => r.Resolve);

            container.RegisterMany(new []{ typeof(Program).Assembly }, t => t.IsInterface, reuse: Reuse.Singleton);

            container.Register(typeof(IPipelineBehavior<GetTemplateCommand, string>), typeof(CacheBehaviour),
                reuse: Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            return container.Resolve<IMediator>();
        }
    }
}
