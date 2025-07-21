using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using user_panel.Context;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;

namespace user_panel.Services.Entity.LogServices
{
    public class LogService(ApplicationDbContext context) : EntityService<LogEntry, int>(context), ILogService
    {
        public async Task<List<LogEntry>> GetLogsAsync()
        {
            return await _context.Logs
                .OrderByDescending(log => log.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<LogEntry>> GetFilteredLogsAsync(string filter)
        {
            return await _context.Logs
                .Where(log => log.Message != null &&
                              EF.Functions.Like(log.Message, $"%{filter}%"))
                .OrderByDescending(log => log.TimeStamp)
                .ToListAsync();
        }
    }
}
