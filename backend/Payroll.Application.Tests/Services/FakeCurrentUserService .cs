using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Tests.Services
{
    public class FakeCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; } = "test-user";
        public string? UserName { get; set; } = "Test User";
        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    }
}
