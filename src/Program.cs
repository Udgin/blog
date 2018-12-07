using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using blg.Application;
using blg.Infrastructure;
using DryIoc;
using Markdig;
using MediatR;

namespace blg
{
    class Program
    {
        static async Task Main(string[] args) =>
            await SetUpMediator().Send(
                new CreateBlogCommand(Directory.GetCurrentDirectory(), @"C:\Users\ea_pyl\projects\blog"));

        private static IMediator SetUpMediator()
        {
            var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());

            container.RegisterDelegate<ServiceFactory>(r => r.Resolve);
            container.RegisterDelegate(r => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
            container.Register<IFileSystem, FileSystem>();

            container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), typeof(Program).GetAssembly() }, Registrator.Interfaces);

            return container.Resolve<IMediator>();
        }
    }
}
