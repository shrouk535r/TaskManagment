using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Excepitions;
using TaskManagement.Application.Features.Comments.Commands.CreateComment;
using TaskManagement.Application.Features.Tasks.Commands.CreateTask;
using TaskManagement.Domain.Entities.Comments;
using TaskManagement.Domain.Entities.Projects;
using TaskManagement.Domain.Entities.Tasks;
using TaskManagement.Domain.Interfaces.Repositories;
using TaskManagement.Domain.Interfaces.UOW;
using Task = System.Threading.Tasks.Task;
using TaskEntity = TaskManagement.Domain.Entities.Tasks.Task;
namespace TaskManagement.XUnitTest.ApplicationTests.Comments.Commands.CreateComment
{
    public class CreateCommentCommandHandlerTest
    {
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly Mock<ICommentRepository> _commentRepoMock;
        private readonly Mock<IUnitOfWork> _unifOfWorkMock;

        public CreateCommentCommandHandlerTest()
        {
            _taskRepoMock = new();
            _commentRepoMock = new();
            _unifOfWorkMock = new();
        }

        [Fact]
        public async Task Create_Comment_with_Invalid_Task_should_Throw_Excepition()
        {
            // Arrange
            var InvalidTaskId = Guid.NewGuid();
            var validUserId = Guid.NewGuid();

            var task = new TaskEntity();
            _taskRepoMock.Setup(x => x.GetById(task.Id)).ReturnsAsync(task);
            var command = new CreateCommentCommand
                (
                "Good Task",
                InvalidTaskId,
                validUserId.ToString()
                );
            var handler = new CreateCommentCommandHandler(_commentRepoMock.Object, _unifOfWorkMock.Object, _taskRepoMock.Object);

            // Act
            Func<Task> result = async () => await handler.Handle(command, default);

            // Assert
            _commentRepoMock.Verify(x => x.Add(It.IsAny<Comment>()), Times.Never, "Not");
            await result.Should().ThrowAsync<NotFoundException>();
        }
        [Fact]
        public async Task Create_Comment_with_valid_Task_should_Created_Successfully_and_get_Guid()
        {
            // Arrange
            var validUserId = Guid.NewGuid();

            var task = new TaskEntity();
            _taskRepoMock.Setup(x => x.GetById(task.Id)).ReturnsAsync(task);
            var command = new CreateCommentCommand
                (
                "Good Task",
                task.Id,
                validUserId.ToString()
                );
            var handler = new CreateCommentCommandHandler(_commentRepoMock.Object, _unifOfWorkMock.Object, _taskRepoMock.Object);

            // Act
            var result = handler.Handle(command, default);

            // Assert
            _commentRepoMock.Verify(x => x.Add(It.IsAny<Comment>()), Times.Once, "Not");
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<Guid>>();


        }

    }
}
