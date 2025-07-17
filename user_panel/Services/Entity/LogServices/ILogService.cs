using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.LogServices
{
    public interface ILogService : IEntityService<LogEntry, int>
    {
        Task<List<LogEntry>> GetLogsAsync();
    }
}
