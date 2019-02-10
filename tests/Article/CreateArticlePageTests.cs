using System;
using System.Threading;
using System.Threading.Tasks;
using blg.Application;
using blg.Domain;
using FakeItEasy;
using FluentValidation;
using Markdig;
using MediatR;
using Xunit;

namespace tests
{
    public class CreateArticlePageTests
    {
        [Fact]
        public async Task Handle()
        {
            var token = new CancellationToken();
            var fileSystem = A.Fake<IFileSystem>();
            var mediator = A.Fake<IMediator>();
            var command = new CreateArticlePageCommand(string.Empty, string.Empty);

            var configuration = A.Fake<IBlogConfiguration>();
            var articleTitle = new ArticleTitle
            {
                Title = nameof(ArticleTitle.Title),
                Date = DateTime.Now,
                Description = nameof(ArticleTitle.Description),
                Publish = true,
                Script = nameof(ArticleTitle.Script),
                Size = 3,
                Tags = new string[0]
            };

            A.CallTo(() => configuration.ArticlesFolder).Returns(string.Empty);
            A.CallTo(() => fileSystem.ReadAllLinesAsync(A<string>._)).Returns(new []{ string.Empty });
            A.CallTo(() => mediator.Send(A<GetConfigurationCommand>._, default(CancellationToken)))
                .Returns(Task.FromResult(configuration));
            A.CallTo(() => mediator.Send(A<GetTemplateCommand>._, default(CancellationToken)))
                .Returns(Task.FromResult(string.Empty));
            A.CallTo(() => mediator.Send(A<ParseTitleCommand>._, default(CancellationToken)))
                .Returns(Task.FromResult(articleTitle));

            var handler = new CreateArticlePageCommandHandler(fileSystem, mediator, new CardEntityValidator());

            var entity = await handler.Handle(command, token);

            Assert.NotNull(entity);
        }
    }
}
