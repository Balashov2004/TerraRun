using TerraRun.Models;

namespace TerraRun.Services;

public interface IRunService
{
    Task<int?> StartRun(int userId);
    Task SavePoint(int runId, double lat, double lon);
    Task StopRun(int runId);
    Task<CaptureResponse?> CaptureCell(int runId, double lat, double lon);
    Task<List<CapturedCellDto>> GetAllCapturedCells();
    Task<UserStatsDto?> GetStats(int userId);
}