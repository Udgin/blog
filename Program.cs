using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DryIoc;
using Markdig;

namespace blg
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await SetUp(new Container()).Resolve<IBlogCreator>().Generate();
        }

        private static IContainer SetUp(IContainer container)
        {
            container.Register(made: Made.Of(() => LocationFactory.Get()), reuse: Reuse.Singleton);
            container.Register<IFileSystem, FileSystem>();
            container.Register<IBlogConfiguration, BlogConfiguration>();
            container.Register<IBlogCreator, BlogCreator>();
            container.Register<IArticlePageCreator, ArticlePageCreator>();
            container.Register<IIndexPageCreator, IndexPageCreator>();
            container.RegisterDelegate((r) => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

            return container;
        }

        private static class LocationFactory
        {
            public static string Get() => Directory.GetCurrentDirectory();
        }
    }
}
