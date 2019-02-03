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
            var markdown = A.Fake<MarkdownPipeline>();
            var validator = A.Fake<IValidator<CardEntity>>();
            var command = A.Fake<CreateArticlePageCommand>();

            var handler = new CreateArticlePageCommandHandler(fileSystem, mediator, markdown, validator);

            var entity = await handler.Handle(command, token);

            Assert.NotNull(entity);
        }
    }
}
