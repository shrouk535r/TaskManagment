using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities.Tasks;
using TaskManagement.Domain.Entities.Users;

namespace TaskManagement.IntegrationTest.Helper
{
    public static class UserHttpHelper
    {
        public static readonly string Regiser= "/api/Auth/Register";
        public static readonly string Login = "/api/Auth/Login";
    }
    public static class CommentHttpHelper
    {
        public static string GetCommentsByTask(Guid taskId) => $"/api/Comment/CommentsByTask/{taskId}";
        public static readonly string CreateComment = "/api/Comment";
    }
    public static class ProjectHttpHelper
    {
        public static readonly string GetAllProjects = "/api/Project";
        public static string GetProjectById(Guid id) => $"/api/Project/GetProjects/{id}";
        public static string GetProjectsByUser(Guid Userid) => $"/api/Project/GetProjectsByUser/{Userid}";
        public static readonly string CreateProject = "/api/Project/CreateProject";
        public static string UpdateProject(Guid id) => $"/api/Project/UpdateProject/{id}";
        public static string DeleteProject(Guid id) => $"/api/Project/DeleteProject/{id}";

    }
    public static class TaskHttpHelper
    {
        public static readonly string GetAllTasks = "/api/Task";
        public static string GetTaskById(Guid id) => $"/api/Task/TasksById/{id}";
        public static string GetTasksByUser(Guid userId) => $"/api/Task/TasksByUser/{userId}";
        public static string GetTasksByProject(Guid projectId) => $"/api/Task/TasksByProject/{projectId}";

        public static readonly string CreateTask = "/api/Task/CreateTask";
        public static string UpdateTask(Guid id) => $"/api/Task/Update/{id}";
        public static string UpdateTaskStatus(Guid id, TaskStatusEnum newstatus) => $"/api/Task/UpdateStatus/{id}?newStatus={newstatus}";
        public static string DeleteTask(Guid id) => $"/api/Task/Delete/{id}";

    }
}
