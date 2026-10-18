using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManagement.Api;
using TaskManagement.Domain.Entities.Projects;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Migrations;

namespace TaskManagement.IntegrationTest.Fixtures
{
    public class WebApplicationFactoryFixture:IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory;

        public HttpClient client { get; private set; }

        private const string _connectionString= "Data Source=.;Initial Catalog = TaskDB; Integrated Security = True; TrustServerCertificate=true";

        public WebApplicationFactoryFixture()
        {
            _factory = new WebApplicationFactory<TaskManagement.Api.Program>().WithWebHostBuilder( //create server for us behined the scene
                builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll(typeof(DbContextOptions<TaskDBContext>));
                        services.AddDbContext<TaskDBContext>(options =>
                        {
                            options.UseSqlServer(_connectionString);
                        });
                    });
                });
            //like http client that can send request to server and get response from it
            client = _factory.CreateClient();

        }

        public async Task InitializeAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var scopeServices = scope.ServiceProvider;
                var _context = scopeServices.GetRequiredService<TaskDBContext>();
                await _context.Database.EnsureCreatedAsync();
            }

        }

        public async Task DisposeAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var scopeServices = scope.ServiceProvider;
                var _context = scopeServices.GetRequiredService<TaskDBContext>();
                await _context.Database.EnsureDeletedAsync();
            }
        }
    }
}
