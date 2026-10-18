using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.Features.Auth.Commands.Login;

namespace TaskManagement.IntegrationTest.APITests.DTOs
{
    public sealed record CreateUpdateResponseDto(string Message, Guid Data);
    public sealed record FinalLoginResponseDto(string Message, LoginResponseDTO Data);

}
