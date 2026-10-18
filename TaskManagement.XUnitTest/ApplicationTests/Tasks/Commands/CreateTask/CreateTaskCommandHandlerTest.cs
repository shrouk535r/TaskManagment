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
using TaskManagement.Application.Features.Tasks.Commands.UpdateTaskStatus;
using TaskManagement.Domain.Entities.Projects;
using TaskManagement.Domain.Entities.Tasks;
using TaskManagement.Domain.Interfaces.Repositories;
using TaskManagement.Domain.Interfaces.UOW;
using Task = System.Threading.Tasks.Task;
using TaskEntity = TaskManagement.Domain.Entities.Tasks.Task;
namespace TaskManagement.XUnitTest.ApplicationTests.Tasks.Commands.CreateTask
{
    public class CreateTaskCommandHandlerTest
    {
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IUnitOfWork> _unifOfWorkMock;

        public CreateTaskCommandHandlerTest()
        {
            _taskRepoMock = new();
            _projectRepoMock = new();
            _unifOfWorkMock = new();
        }

        [Fact]
        public async Task Create_Task_with_Invalid_Project_should_Throw_Excepition()
        {
            // Arrange
            var InvalidProjectId = Guid.NewGuid();
            var validUserId = Guid.NewGuid();

            var project = new Project() { UserId = validUserId };
            _projectRepoMock.Setup(x => x.GetById(project.Id)).ReturnsAsync(project);
            var command = new CreateTaskCommand
                (
                "Task1",
                "None Description",
                TaskPriority.Low,
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                InvalidProjectId,
                validUserId.ToString()
                );
            var handler = new CreateTaskCommandHandler(_taskRepoMock.Object, _unifOfWorkMock.Object, _projectRepoMock.Object);

            // Act
            Func<Task> result = async () => await handler.Handle(command, default);

            // Assert
            _taskRepoMock.Verify(x => x.Update(It.IsAny<TaskEntity>()), Times.Never, "Not");
            await result.Should().ThrowAsync<NotFoundException>();
        }
        [Fact]
        public async Task Create_Task_with_Invalid_User_should_Throw_Excepition()
        {
            // Arrange
            var validUserId = Guid.NewGuid();
            var invalidUserId = Guid.NewGuid();
            var project = new Project() { UserId = validUserId };
            _projectRepoMock.Setup(x => x.GetById(project.Id)).ReturnsAsync(project);
            var command = new CreateTaskCommand
                (
                "Task1",
                "None Description",
                TaskPriority.Low,
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                project.Id,
                invalidUserId.ToString()
                );
            var handler = new CreateTaskCommandHandler(_taskRepoMock.Object, _unifOfWorkMock.Object, _projectRepoMock.Object);

            // Act
            Func<Task> result = async () => await handler.Handle(command, default);

            // Assert
            _taskRepoMock.Verify(x => x.Update(It.IsAny<TaskEntity>()), Times.Never, "Not");
            await result.Should().ThrowAsync<ForbiddenException>();

        }
        [Fact]
        public async Task Create_Task_with_Valid_Project_and_Valid_User_should_Ceated_successfully_and_return_guid()
        {
            // Arrange
            var validUserId = Guid.NewGuid();

            var project = new Project() { UserId = validUserId };
            _projectRepoMock.Setup(x => x.GetById(project.Id)).ReturnsAsync(project);
            var command = new CreateTaskCommand
                (
                "Task1",
                "None Description",
                TaskPriority.Low,
                DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                project.Id,
                validUserId.ToString()
                );
            var handler = new CreateTaskCommandHandler(_taskRepoMock.Object, _unifOfWorkMock.Object, _projectRepoMock.Object);

            // Act
            var result =   handler.Handle(command, default);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Task<Guid>>();
            _taskRepoMock.Verify(x => x.Add(It.IsAny<TaskEntity>()), Times.Once, "Not");
        }
    }
}