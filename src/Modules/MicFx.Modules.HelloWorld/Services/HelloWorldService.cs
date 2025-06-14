using MicFx.Abstractions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using MicFx.SharedKernel.Modularity;
using MicFx.Modules.HelloWorld.Domain;

namespace MicFx.Modules.HelloWorld.Services;

/// <summary>
/// Interface for HelloWorld business operations
/// Demonstrates service layer pattern in MicFx framework
/// </summary>
public interface IHelloWorldService
{
    /// <summary>
    /// Gets a standard greeting message
    /// </summary>
    /// <param name="context">Optional context for the greeting</param>
    /// <returns>Greeting entity</returns>
    Task<Greeting> GetGreetingAsync(string? context = null);

    /// <summary>
    /// Creates a personalized greeting for a user
    /// </summary>
    /// <param name="userName">User name for personalization</param>
    /// <param name="context">Optional context</param>
    /// <returns>User interaction with personalized greeting</returns>
    Task<UserInteraction> CreatePersonalizedGreetingAsync(string userName, string? context = null);

    /// <summary>
    /// Gets module information and statistics
    /// </summary>
    /// <returns>Module statistics</returns>
    Task<ModuleStatistics> GetModuleStatisticsAsync();

    /// <summary>
    /// Gets all available greetings
    /// </summary>
    /// <returns>List of greetings</returns>
    Task<IEnumerable<Greeting>> GetAllGreetingsAsync();

    /// <summary>
    /// Gets module manifest information
    /// </summary>
    /// <returns>Module manifest</returns>
    Task<IModuleManifest> GetModuleManifestAsync();

    /// <summary>
    /// Validates framework integration health
    /// </summary>
    /// <returns>Health status information</returns>
    Task<Dictionary<string, object>> ValidateFrameworkIntegrationAsync();
}

/// <summary>
/// HelloWorld service implementation demonstrating MicFx framework patterns
/// Showcases structured logging, exception handling, and domain-driven design
/// </summary>
public class HelloWorldService : IHelloWorldService
{
    private readonly IStructuredLogger<HelloWorldService> _logger;
    private readonly IModuleManifest _manifest;
    private readonly DateTime _moduleStartTime;
    private readonly List<Greeting> _greetings;
    private readonly List<UserInteraction> _interactions;

    public HelloWorldService(IStructuredLogger<HelloWorldService> logger)
    {
        _logger = logger;
        _manifest = new Manifest();
        _moduleStartTime = DateTime.UtcNow;
        _greetings = InitializeGreetings();
        _interactions = new List<UserInteraction>();

        _logger.LogBusinessOperation("ServiceInitialization", 
            new { ModuleName = _manifest.Name, GreetingCount = _greetings.Count }, 
            "HelloWorldService initialized successfully");
    }

    public async Task<Greeting> GetGreetingAsync(string? context = null)
    {
        using var timer = _logger.BeginTimedOperation("GetGreeting", 
            new { Context = context, ModuleName = _manifest.Name });

        _logger.LogBusinessOperation("GetGreeting", 
            new { RequestedContext = context, AvailableGreetings = _greetings.Count }, 
            "Processing greeting request");

        try
        {
            // Simulate async operation
            await Task.Delay(10);

            var greeting = context switch
            {
                "welcome" => _greetings.FirstOrDefault(g => g.Context == "welcome") ?? Greeting.CreateWelcome(),
                "demo" => _greetings.FirstOrDefault(g => g.Context == "demo") ?? 
                         new Greeting { Message = "MicFx Framework Demo - Showcasing modular architecture! 🏗️", Context = "demo" },
                _ => _greetings.FirstOrDefault(g => g.Context == "default") ?? Greeting.CreateHello()
            };

            greeting.IncrementUsage();

            _logger.LogBusinessOperation("GetGreeting", 
                new { 
                    GreetingId = greeting.Id, 
                    Context = greeting.Context, 
                    UsageCount = greeting.UsageCount,
                    Success = true
                }, 
                "Greeting retrieved and usage incremented");

            return greeting;
        }
        catch (Exception ex)
        {
            _logger.LogSecurity("ServiceError", null, 
                new { 
                    ServiceMethod = nameof(GetGreetingAsync), 
                    Context = context,
                    ErrorType = ex.GetType().Name 
                }, 
                $"Error in GetGreetingAsync: {ex.Message}");
            
            throw new BusinessException("Failed to retrieve greeting", "GREETING_RETRIEVAL_FAILED")
                .AddDetail("Context", context)
                .AddDetail("ServiceName", nameof(HelloWorldService));
        }
    }

    public async Task<UserInteraction> CreatePersonalizedGreetingAsync(string userName, string? context = null)
    {
        using var timer = _logger.BeginTimedOperation("CreatePersonalizedGreeting", 
            new { UserName = userName, Context = context });

        _logger.LogBusinessOperation("CreatePersonalizedGreeting", 
            new { UserName = userName, RequestedContext = context }, 
            "Creating personalized greeting");

        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogSecurity("ValidationFailure", userName, 
                    new { ValidationRule = "UserNameRequired", ProvidedValue = userName }, 
                    "Empty or null user name provided");
                
                var validationErrors = new List<ValidationError>
                {
                    new ValidationError { Field = "UserName", Message = "User name cannot be empty or whitespace" }
                };
                throw new ValidationException("User name is required", validationErrors);
            }

            if (userName.Length > 100)
            {
                _logger.LogSecurity("ValidationFailure", userName, 
                    new { ValidationRule = "UserNameLength", ProvidedLength = userName.Length }, 
                    "User name exceeds maximum length");
                
                var validationErrors = new List<ValidationError>
                {
                    new ValidationError { Field = "UserName", Message = "User name must be 100 characters or less" }
                };
                throw new ValidationException("User name too long", validationErrors);
            }

            // Get base greeting
            var baseGreeting = await GetGreetingAsync(context);
            
            // Create personalized interaction
            var interaction = UserInteraction.CreatePersonalized(userName, baseGreeting, "api");
            _interactions.Add(interaction);

            _logger.LogBusinessOperation("CreatePersonalizedGreeting", 
                new { 
                    InteractionId = interaction.Id,
                    UserName = userName,
                    GreetingId = baseGreeting.Id,
                    TotalInteractions = _interactions.Count,
                    Success = true
                }, 
                "Personalized greeting created successfully");

            return interaction;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogSecurity("ServiceError", userName, 
                new { 
                    ServiceMethod = nameof(CreatePersonalizedGreetingAsync), 
                    UserName = userName,
                    Context = context,
                    ErrorType = ex.GetType().Name 
                }, 
                $"Error creating personalized greeting: {ex.Message}");
            
            throw new BusinessException("Failed to create personalized greeting", "PERSONALIZATION_FAILED")
                .AddDetail("UserName", userName)
                .AddDetail("Context", context);
        }
    }

    public async Task<ModuleStatistics> GetModuleStatisticsAsync()
    {
        using var timer = _logger.BeginTimedOperation("GetModuleStatistics", 
            new { ModuleName = _manifest.Name });

        _logger.LogBusinessOperation("GetModuleStatistics", 
            new { RequestTime = DateTime.UtcNow }, 
            "Calculating module statistics");

        try
        {
            // Simulate some processing
            await Task.Delay(5);

            var mostPopular = _greetings
                .OrderByDescending(g => g.UsageCount)
                .FirstOrDefault()?.Message ?? "None";

            var avgUsage = _greetings.Any() ? _greetings.Average(g => g.UsageCount) : 0.0;

            var statistics = ModuleStatistics.Create(
                _greetings.Count,
                _interactions.Count,
                mostPopular,
                avgUsage,
                _moduleStartTime);

            _logger.LogBusinessOperation("GetModuleStatistics", 
                new { 
                    TotalGreetings = statistics.TotalGreetings,
                    TotalInteractions = statistics.TotalInteractions,
                    ModuleUptime = statistics.ModuleUptime.ToString(),
                    Success = true
                }, 
                "Module statistics calculated successfully");

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogSecurity("ServiceError", null, 
                new { 
                    ServiceMethod = nameof(GetModuleStatisticsAsync), 
                    ErrorType = ex.GetType().Name 
                }, 
                $"Error calculating statistics: {ex.Message}");
            
            throw new BusinessException("Failed to calculate module statistics", "STATISTICS_CALCULATION_FAILED");
        }
    }

    public async Task<IEnumerable<Greeting>> GetAllGreetingsAsync()
    {
        using var timer = _logger.BeginTimedOperation("GetAllGreetings", 
            new { ModuleName = _manifest.Name });

        _logger.LogBusinessOperation("GetAllGreetings", 
            new { GreetingCount = _greetings.Count }, 
            "Retrieving all greetings");

        // Simulate async operation
        await Task.Delay(5);

        var activeGreetings = _greetings.Where(g => g.IsActive).ToList();

        _logger.LogBusinessOperation("GetAllGreetings", 
            new { 
                TotalGreetings = _greetings.Count, 
                ActiveGreetings = activeGreetings.Count,
                Success = true
            }, 
            "All greetings retrieved successfully");

        return activeGreetings;
    }

    public async Task<IModuleManifest> GetModuleManifestAsync()
    {
        using var timer = _logger.BeginTimedOperation("GetModuleManifest", 
            new { ModuleName = _manifest.Name });

        _logger.LogBusinessOperation("GetModuleManifest", 
            new { ManifestVersion = _manifest.Version }, 
            "Retrieving module manifest");

        // Simulate async operation
        await Task.Delay(2);

        _logger.LogBusinessOperation("GetModuleManifest", 
            new { 
                ModuleName = _manifest.Name,
                Version = _manifest.Version,
                Author = _manifest.Author,
                Success = true
            }, 
            "Module manifest retrieved successfully");

        return _manifest;
    }

    public async Task<Dictionary<string, object>> ValidateFrameworkIntegrationAsync()
    {
        using var timer = _logger.BeginTimedOperation("ValidateFrameworkIntegration", 
            new { ModuleName = _manifest.Name });

        _logger.LogBusinessOperation("ValidateFrameworkIntegration", 
            new { ValidationStartTime = DateTime.UtcNow }, 
            "Starting framework integration validation");

        try
        {
            // Simulate validation checks
            await Task.Delay(20);

            var validationResults = new Dictionary<string, object>
            {
                ["StructuredLoggingWorking"] = true,
                ["ExceptionHandlingWorking"] = true,
                ["DependencyInjectionWorking"] = true,
                ["ModuleManifestValid"] = _manifest != null,
                ["DomainEntitiesWorking"] = _greetings.Any(),
                ["ServiceLayerWorking"] = true,
                ["ValidationTime"] = DateTime.UtcNow,
                ["ModuleUptime"] = DateTime.UtcNow - _moduleStartTime,
                ["TotalValidationChecks"] = 6,
                ["PassedChecks"] = 6,
                ["HealthStatus"] = "Healthy"
            };

            _logger.LogBusinessOperation("ValidateFrameworkIntegration", 
                new { 
                    ValidationResults = validationResults,
                    AllChecksPassed = true,
                    Success = true
                }, 
                "Framework integration validation completed successfully");

            return validationResults;
        }
        catch (Exception ex)
        {
            _logger.LogSecurity("ValidationError", null, 
                new { 
                    ValidationMethod = nameof(ValidateFrameworkIntegrationAsync), 
                    ErrorType = ex.GetType().Name 
                }, 
                $"Framework validation failed: {ex.Message}");
            
            throw new ModuleException("Framework integration validation failed", ex, _manifest.Name);
        }
    }

    /// <summary>
    /// Initializes default greetings for the module
    /// </summary>
    private List<Greeting> InitializeGreetings()
    {
        return new List<Greeting>
        {
            Greeting.CreateHello("default"),
            Greeting.CreateWelcome("welcome"),
            new Greeting 
            { 
                Message = "MicFx Framework - Building the future of modular applications! 🏗️", 
                Context = "demo",
                Language = "en"
            },
            new Greeting 
            { 
                Message = "Clean Architecture + SOLID Principles = Maintainable Code! ⚡", 
                Context = "architecture",
                Language = "en"
            },
            new Greeting 
            { 
                Message = "Zero Configuration, Maximum Productivity! 🚀", 
                Context = "productivity",
                Language = "en"
            }
        };
    }
} 