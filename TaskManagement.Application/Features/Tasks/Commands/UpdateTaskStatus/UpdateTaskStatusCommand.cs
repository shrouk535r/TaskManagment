using TaskManagement.Domain.Entities.Tasks;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Results;

namespace TaskManagement.Application.Features.Tasks.Commands.UpdateTaskStatus
{
    public sealed record UpdateTaskStatusCommand(Guid TaskId, TaskStatusEnum NewStatus, string userId):IRequest<Result<string>>;
}
