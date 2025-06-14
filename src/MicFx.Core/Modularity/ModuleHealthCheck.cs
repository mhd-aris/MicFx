using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Health check untuk semua modules dalam sistem
    /// </summary>
    public class ModuleHealthCheck : IHealthCheck
    {
        private readonly ModuleLifecycleManager _lifecycleManager;
        private readonly ILogger<ModuleHealthCheck> _logger;

        public ModuleHealthCheck(ModuleLifecycleManager lifecycleManager, ILogger<ModuleHealthCheck> logger)
        {
            _lifecycleManager = lifecycleManager;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var moduleStates = _lifecycleManager.GetAllModuleStates();
                var healthData = new Dictionary<string, object>();
                var unhealthyModules = new List<string>();
                var degradedModules = new List<string>();

                foreach (var kvp in moduleStates)
                {
                    var moduleName = kvp.Key;
                    var moduleState = kvp.Value;

                    try
                    {
                        var healthDetails = await _lifecycleManager.CheckModuleHealthAsync(moduleName, cancellationToken);

                        healthData[moduleName] = new
                        {
                            State = moduleState.State.ToString(),
                            Health = healthDetails.Status.ToString(),
                            Description = healthDetails.Description,
                            LastStateChange = moduleState.LastStateChange,
                            ErrorCount = moduleState.ErrorCount,
                            RegisteredAt = moduleState.RegisteredAt,
                            CheckDuration = healthDetails.Duration,
                            CheckedAt = healthDetails.CheckedAt
                        };

                        switch (healthDetails.Status)
                        {
                            case ModuleHealthStatus.Unhealthy:
                                unhealthyModules.Add(moduleName);
                                break;
                            case ModuleHealthStatus.Degraded:
                                degradedModules.Add(moduleName);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error checking health for module {ModuleName}", moduleName);
                        unhealthyModules.Add(moduleName);

                        healthData[moduleName] = new
                        {
                            State = moduleState.State.ToString(),
                            Health = "Unknown",
                            Description = $"Health check failed: {ex.Message}",
                            LastStateChange = moduleState.LastStateChange,
                            ErrorCount = moduleState.ErrorCount + 1
                        };
                    }
                }

                // Determine overall health status
                var overallStatus = HealthStatus.Healthy;
                var description = "All modules are healthy";

                if (unhealthyModules.Any())
                {
                    overallStatus = HealthStatus.Unhealthy;
                    description = $"Unhealthy modules: {string.Join(", ", unhealthyModules)}";
                }
                else if (degradedModules.Any())
                {
                    overallStatus = HealthStatus.Degraded;
                    description = $"Degraded modules: {string.Join(", ", degradedModules)}";
                }

                healthData["Summary"] = new
                {
                    TotalModules = moduleStates.Count,
                    HealthyModules = moduleStates.Count - unhealthyModules.Count - degradedModules.Count,
                    DegradedModules = degradedModules.Count,
                    UnhealthyModules = unhealthyModules.Count,
                    CheckedAt = DateTime.UtcNow
                };

                return new HealthCheckResult(overallStatus, description, data: healthData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during module health check");
                return HealthCheckResult.Unhealthy("Failed to check module health", ex);
            }
        }
    }
}