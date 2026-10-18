using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Api.Requests.Auth;
using TaskManagement.Api.Requests.Projects;
using TaskManagement.Api.Requests.Tasks;
using TaskManagement.Application.Features.Auth.Commands.Login;
using TaskManagement.Application.Features.Projects.Commands.CreateProject;
using TaskManagement.Application.Features.Projects.Queries.DTOS;
using TaskManagement.Application.Features.Tasks.Queries.DTOS;
using TaskManagement.Application.Results;
using TaskManagement.Domain.Entities.Projects;
using TaskManagement.Domain.Entities.Tasks;
using TaskManagement.Domain.Entities.Users;
using TaskManagement.IntegrationTest.APITests.DTOs;
using TaskManagement.IntegrationTest.Fixtures;
using TaskManagement.IntegrationTest.Helper;

namespace TaskManagement.IntegrationTest.APITests.CriticalWorkflowIntegrationTests
{
    public class RegisterToCompleteTaskWorkflowIntegrationTests:IClassFixture<WebApplicationFactoryFixture>
    {
        private WebApplicationFactoryFixture _factory;

        public RegisterToCompleteTaskWorkflowIntegrationTests(WebApplicationFactoryFixture factory)
        {
            _factory = factory;
        }
        private async Task<HttpResponseMessage?> RegisterAsync(string Name,string email, string password,string city, UserRole role) 
        {
            var request = new RegisterRequest()
            {
                Name=Name,
                Email= email,
                Password= password,
                City =city,
                Role =role
            };
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            return await _factory.client.PostAsync(UserHttpHelper.Regiser, httpContent);

        }
        private async Task<HttpResponseMessage?> LoginAsync(string email, string password)
        {
            var request = new LoginRequest()
            {
                Email = email,
                Password = password,
             };
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            return await  _factory.client.PostAsync(UserHttpHelper.Login, httpContent);
        }
        private void SetBearerToken(string token)
        {
            _factory.client.DefaultRequestHeaders.Authorization= new AuthenticationHeaderValue("Bearer",token);
        }

        private async Task<HttpResponseMessage?>CreateProjectAsync(string name, string description)
        {
            var request = new CreateProjectRequest() 
            {
                Name= name,
                Description= description,
            };
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            return await _factory.client.PostAsync(ProjectHttpHelper.CreateProject, httpContent);
        }
        private async Task<HttpResponseMessage?> GetProjectByIdAsync(Guid projectId)=>
            await _factory.client.GetAsync(ProjectHttpHelper.GetProjectById(projectId));

        private async Task<HttpResponseMessage?> CreateTaskAsync(string title, string description, TaskPriority priority,DateOnly date,Guid projectId)
        {
            var request = new CreateTaskRequest()
            {
                Title = title,
                Description = description,
                Priority = priority,
                DueDate = date,
                ProjectId = projectId
            };
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");


            return await _factory.client.PostAsync(TaskHttpHelper.CreateTask, httpContent);
        }
        private async Task<HttpResponseMessage?> GetTaskByIdAsync(Guid taskId) =>
            await _factory.client.GetAsync(TaskHttpHelper.GetTaskById(taskId));

        private async Task<HttpResponseMessage?> UpdateTaskStatus(Guid taskId, TaskStatusEnum newStatus)
        {
            return await _factory.client.PutAsync(TaskHttpHelper.UpdateTaskStatus(taskId,newStatus),null);
        }
        [Fact]
        public async Task Task_Workflow_From_Register_To_Complete_Task_Should_Succeed()
        {
            //Register
            //Arrange
            var validUserName = "User1";
            var validUserEmail = "User@mail.com";//register & login
            var validUserPassword = "User@123";//register &login
            var validUserCity = "cairo";
            var validUserRole = UserRole.Member;

            //Act
            var registerResponse = await RegisterAsync(validUserName, validUserEmail, validUserPassword, validUserCity, validUserRole);
            registerResponse!.StatusCode.Should().Be(HttpStatusCode.OK);

            var registerResult = await registerResponse!.Content.ReadFromJsonAsync<CreateUpdateResponseDto>();//userId

            //Assert
            registerResult.Should().NotBeNull();
            registerResult.Message.Should().Be("Registerd Successfully");
            registerResult.Data.Should().NotBeEmpty();
            registerResult.Data.GetType().Should().Be(typeof(Guid));


            //Login
            //Act
            var loginResponse = await LoginAsync(validUserEmail, validUserPassword);

            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<FinalLoginResponseDto>();//userId,token,userRole
           
            //Assert
            loginResult.Should().NotBeNull();
            loginResult.Message.Should().Be("Logined Successfully");
            loginResult.Data.Should().NotBeNull();
            loginResult!.Data.UserId.Should().NotBeEmpty();
            loginResult!.Data.UserId.Should().Be(registerResult.Data);
            loginResult!.Data.Role.Should().NotBeNullOrEmpty();
            loginResult!.Data.Role.Should().Be(validUserRole.ToString());

            //Authenticate
            //Act
            SetBearerToken(loginResult!.Data.Token);

            //createProject
            //Arrange
            var projectName = "Project1";
            var projectDesc = "new Project1";

            //act
            var CreateProjectResponse = await CreateProjectAsync(projectName, projectDesc);
            CreateProjectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var CreateProjectResult = await CreateProjectResponse.Content.ReadFromJsonAsync<CreateUpdateResponseDto>();//userId,token,userRole
            
            var ProjectResponse = await GetProjectByIdAsync(CreateProjectResult.Data);
            ProjectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var ProjectResult = await ProjectResponse.Content.ReadFromJsonAsync<ProjectDetailsDto>();

            //Assert
            CreateProjectResult.Should().NotBeNull();
            CreateProjectResult.Message.Should().NotBeNullOrEmpty();
            CreateProjectResult.Message.Should().Be("Project Added Successfully!");
            CreateProjectResult.Data.Should().NotBeEmpty();
            CreateProjectResult.Data.GetType().Should().Be(typeof(Guid));
            ProjectResult.Should().NotBeNull();
            ProjectResult.Name.Should().Be(projectName);
            ProjectResult.Description.Should().Be(projectDesc);
            ProjectResult.Completed.Should().BeFalse();
            ProjectResult.projectId.Should().Be(CreateProjectResult.Data);
            ProjectResult.projectId.Should().Be(CreateProjectResult.Data);
            ProjectResult.UserName.Should().Be(validUserName);

            //createTask
            //Arrange
            var taskTitle = "Task1";
            var taskDesc = "new Task for Project1";
            var taskPriority = TaskPriority.Normal;
            var taskDueDate = DateTime.Now.AddDays(3);
            //act
            var createTaskResponse = await CreateTaskAsync(taskTitle, taskDesc,taskPriority,DateOnly.FromDateTime(taskDueDate),CreateProjectResult.Data);
            createTaskResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var CreateTaskResult = await createTaskResponse.Content.ReadFromJsonAsync<CreateUpdateResponseDto>();//userId,token,userRole

            var TaskResponse = await GetTaskByIdAsync(CreateTaskResult.Data);
            TaskResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var TaskResult = await TaskResponse.Content.ReadFromJsonAsync<TaskDetailsDto>();

            //Assert
            CreateTaskResult.Should().NotBeNull();
            CreateTaskResult.Message.Should().NotBeNullOrEmpty();
            CreateTaskResult.Message.Should().Be("Task Added Successfully!");
            CreateTaskResult.Data.Should().NotBeEmpty();
            CreateTaskResult.Data.GetType().Should().Be(typeof(Guid));
            TaskResult.Should().NotBeNull();
            TaskResult.TaskId.Should().Be(CreateTaskResult.Data);
            TaskResult.Title.Should().Be(taskTitle);
            TaskResult.Description.Should().Be(taskDesc);
            TaskResult.Priority.Should().Be(taskPriority);
            TaskResult.ProjectName.Should().Be(ProjectResult.Name);
            TaskResult.DueDate.Should().Be(DateOnly.FromDateTime(taskDueDate));
            TaskResult.Status.Should().Be(TaskStatusEnum.Todo);



            //assignTask
            //Arrange
            var AssignTaskStatus = TaskStatusEnum.InProgress;
            //act
            var InprogressTaskResponse = await UpdateTaskStatus(CreateTaskResult.Data, AssignTaskStatus);
                InprogressTaskResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
            var InProgressTaskResult = await InprogressTaskResponse.Content.ReadAsStringAsync();

            var TaskResponseAfterAssigning = await GetTaskByIdAsync(CreateTaskResult.Data);
            TaskResponseAfterAssigning.StatusCode.Should().Be(HttpStatusCode.OK);
            var TaskResultAfterAssigning = await TaskResponseAfterAssigning.Content.ReadFromJsonAsync<TaskDetailsDto>();

            //Assert
            InProgressTaskResult.Should().NotBeNull();
            InProgressTaskResult.Should().Be($"Task With Id : {CreateTaskResult.Data} \n Convert Successfully From {TaskStatusEnum.Todo} To {AssignTaskStatus}");
            TaskResultAfterAssigning.Should().NotBeNull();
            TaskResultAfterAssigning.TaskId.Should().Be(CreateTaskResult.Data);
            TaskResultAfterAssigning.Status.Should().Be(TaskStatusEnum.InProgress);


            //completeTask
            //Arrange
            var CompleteTaskStatus = TaskStatusEnum.Completed;
            //act
            var CompleteTaskResponse = await UpdateTaskStatus(CreateTaskResult.Data, CompleteTaskStatus);
            CompleteTaskResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
            var CompleteTaskResult = await CompleteTaskResponse.Content.ReadAsStringAsync();

            var TaskResponseAfterCompleting = await GetTaskByIdAsync(CreateTaskResult.Data);
            TaskResponseAfterCompleting.StatusCode.Should().Be(HttpStatusCode.OK);
            var TaskResultAfterCompleting = await TaskResponseAfterCompleting.Content.ReadFromJsonAsync<TaskDetailsDto>();

            //Assert
            CompleteTaskResult.Should().NotBeNull();
            CompleteTaskResult.Should().Be($"Task With Id : {CreateTaskResult.Data} \n Convert Successfully From {AssignTaskStatus} To {CompleteTaskStatus}");
            TaskResultAfterCompleting.Should().NotBeNull();
            TaskResultAfterCompleting.TaskId.Should().Be(CreateTaskResult.Data);
            TaskResultAfterCompleting.Status.Should().Be(TaskStatusEnum.Completed);




        }
    }
}
