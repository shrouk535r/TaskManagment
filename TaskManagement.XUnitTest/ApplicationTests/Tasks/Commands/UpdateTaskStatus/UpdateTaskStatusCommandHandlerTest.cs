using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Features.Tasks.Commands.UpdateTaskStatus;
using TaskManagement.Domain.Entities.Projects;
using TaskManagement.Domain.Entities.Tasks;
using TaskManagement.Domain.Interfaces.Repositories;
using TaskManagement.Domain.Interfaces.UOW;
using Task = System.Threading.Tasks.Task;
namespace TaskManagement.XUnitTest.ApplicationTests.Tasks.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandHandlerTest
    {
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly Mock<IUnitOfWork> _unifOfWorkMock;



        public UpdateTaskStatusCommandHandlerTest()
        {
            _taskRepoMock = new();
            _unifOfWorkMock = new();
        }

        [Fact]
        public async Task Transition_Status_From_Todo_To_InProgress_Should_Succeed()
        {
            //Arrange
            var OldStatus = TaskStatusEnum.Todo;
            Guid userId = Guid.NewGuid();
            var task = new Domain.Entities.Tasks.Task() 
            {
                Status =OldStatus,
                Project =new Project()
                { UserId =userId }
            };
            var newStatus = TaskStatusEnum.InProgress;
            var command = new UpdateTaskStatusCommand(task.Id,newStatus,userId.ToString());
            _taskRepoMock.Setup(x => x.GetById(task.Id)).ReturnsAsync(task);
            var handler = new UpdateTaskStatusCommandHandler(_taskRepoMock.Object,_unifOfWorkMock.Object);

            //Act
            var result = await handler.Handle(command, default);
            
            //Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }
        public async Task Transition_Status_From_Completed_To_InProgress_Should_Failed()
        {
            //Arrange
            var OldStatus = TaskStatusEnum.Completed;
            Guid userId = Guid.NewGuid();
            var task = new Domain.Entities.Tasks.Task()
            {
                Status = OldStatus,
                Project = new Project()
                { UserId = userId }
            };
            var newStatus = TaskStatusEnum.InProgress;
            var command = new UpdateTaskStatusCommand(task.Id, newStatus, userId.ToString());
            _taskRepoMock.Setup(x => x.GetById(task.Id)).ReturnsAsync(task);
            var handler = new UpdateTaskStatusCommandHandler(_taskRepoMock.Object, _unifOfWorkMock.Object);

            //Act
            var result = await handler.Handle(command, default);
            //Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

    }
}
