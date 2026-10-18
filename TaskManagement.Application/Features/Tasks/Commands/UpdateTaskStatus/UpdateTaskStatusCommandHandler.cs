using TaskManagement.Application.Excepitions;
using TaskManagement.Domain.Entities.Projects;
using TaskManagement.Domain.Entities.Tasks;
using TaskManagement.Domain.Interfaces.Repositories;
using TaskManagement.Domain.Interfaces.UOW;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Results;

namespace TaskManagement.Application.Features.Tasks.Commands.UpdateTaskStatus
{
    public sealed class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Result<string>>
    {
        private ITaskRepository _taskRepository;
        private IUnitOfWork _unitOfWork;
        public UpdateTaskStatusCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
        {
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var task = await _taskRepository.GetById(request.TaskId);
            if (task == null)
            {
                return new Error("Id Not Found", $"There is Not Task With This Id: {request.TaskId}");
            }
            var OldStautus = task.Status;
            if (!Guid.TryParse(request.userId, out var userId))
                return new Error("Convert Conflict", "id cannot convert to Guid");

            if (task.Project.UserId != userId)
                return new Error("Forbidden Error", "This User Not Allowed to Update this Task, Owner only can update it");


            if (!validTransition(task.Status, request.NewStatus))
                return new Error("Task Status Conflict", $"cannot convert from {task.Status} to {request.NewStatus}");

            task.Status = request.NewStatus;
            _taskRepository.Update(task);
            await _unitOfWork.Save();
            return $"Task With Id : {request.TaskId} \n Convert Successfully From {OldStautus} To {task.Status}";
        }
        public bool validTransition(TaskStatusEnum oldstatus,TaskStatusEnum newstaus )
        {
            return oldstatus switch
            {
                TaskStatusEnum.Todo => true,
                TaskStatusEnum.InProgress => newstaus == TaskStatusEnum.InProgress || newstaus == TaskStatusEnum.Completed || newstaus == TaskStatusEnum.Cancelled,
                TaskStatusEnum.Completed => false,
                TaskStatusEnum.Cancelled => false,
            };
        }
    }
}
