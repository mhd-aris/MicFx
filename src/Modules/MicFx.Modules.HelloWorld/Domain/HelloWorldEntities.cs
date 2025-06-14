using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.HelloWorld.Domain;

/// <summary>
/// Core greeting entity representing a greeting message with metadata
/// Demonstrates domain modeling in MicFx framework
/// </summary>
public class Greeting
{
    /// <summary>
    /// Unique identifier for the greeting
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The greeting message content
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The language code for the greeting (e.g., "en", "id", "fr")
    /// </summary>
    [StringLength(5)]
    public string Language { get; set; } = "en";

    /// <summary>
    /// When this greeting was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Context or category for the greeting
    /// </summary>
    [StringLength(50)]
    public string? Context { get; set; }

    /// <summary>
    /// Number of times this greeting has been used
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Whether this greeting is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creates a standard hello greeting
    /// </summary>
    public static Greeting CreateHello(string? context = null) => new()
    {
        Message = "Hello from MicFx Framework! 🚀",
        Context = context ?? "default",
        Language = "en"
    };

    /// <summary>
    /// Creates a welcome greeting
    /// </summary>
    public static Greeting CreateWelcome(string? context = null) => new()
    {
        Message = "Welcome to the MicFx modular framework!",
        Context = context ?? "welcome",
        Language = "en"
    };

    /// <summary>
    /// Increments the usage count
    /// </summary>
    public void IncrementUsage()
    {
        UsageCount++;
    }

    /// <summary>
    /// Validates the greeting entity
    /// </summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Message) && IsActive;
}

/// <summary>
/// User interaction entity for personalized greetings
/// Demonstrates user context in domain modeling
/// </summary>
public class UserInteraction
{
    /// <summary>
    /// Unique identifier for the interaction
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// User name for personalization
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// The greeting that was used
    /// </summary>
    public Greeting Greeting { get; set; } = new();

    /// <summary>
    /// Personalized message generated for the user
    /// </summary>
    public string PersonalizedMessage { get; set; } = string.Empty;

    /// <summary>
    /// When this interaction occurred
    /// </summary>
    public DateTime InteractionTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source of the interaction (web, api, etc.)
    /// </summary>
    [StringLength(20)]
    public string Source { get; set; } = "api";

    /// <summary>
    /// Additional metadata for the interaction
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Creates a personalized interaction
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="greeting">Base greeting to personalize</param>
    /// <param name="source">Source of interaction</param>
    /// <returns>Personalized user interaction</returns>
    public static UserInteraction CreatePersonalized(string userName, Greeting greeting, string source = "api")
    {
        var interaction = new UserInteraction
        {
            UserName = userName.Trim(),
            Greeting = greeting,
            Source = source,
            PersonalizedMessage = $"Hello {userName.Trim()}! {greeting.Message}",
            Metadata = new Dictionary<string, object>
            {
                ["GreetingId"] = greeting.Id,
                ["UserNameLength"] = userName.Trim().Length,
                ["HasSpecialChars"] = userName.Any(c => !char.IsLetterOrDigit(c) && c != ' ')
            }
        };

        // Mark greeting as used
        greeting.IncrementUsage();

        return interaction;
    }

    /// <summary>
    /// Validates the user interaction
    /// </summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(UserName) && 
                            !string.IsNullOrWhiteSpace(PersonalizedMessage) && 
                            Greeting.IsValid();
}

/// <summary>
/// Module statistics value object
/// Demonstrates value objects in domain design
/// </summary>
public record ModuleStatistics
{
    /// <summary>
    /// Total number of greetings available
    /// </summary>
    public int TotalGreetings { get; init; }

    /// <summary>
    /// Total number of user interactions
    /// </summary>
    public int TotalInteractions { get; init; }

    /// <summary>
    /// Most popular greeting
    /// </summary>
    public string MostPopularGreeting { get; init; } = string.Empty;

    /// <summary>
    /// Average greeting usage
    /// </summary>
    public double AverageUsage { get; init; }

    /// <summary>
    /// When statistics were calculated
    /// </summary>
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Module uptime since startup
    /// </summary>
    public TimeSpan ModuleUptime { get; init; }

    /// <summary>
    /// Creates empty statistics
    /// </summary>
    public static ModuleStatistics Empty => new()
    {
        TotalGreetings = 0,
        TotalInteractions = 0,
        MostPopularGreeting = "None",
        AverageUsage = 0.0,
        ModuleUptime = TimeSpan.Zero
    };

    /// <summary>
    /// Creates statistics from data
    /// </summary>
    public static ModuleStatistics Create(
        int totalGreetings, 
        int totalInteractions, 
        string mostPopular, 
        double avgUsage,
        DateTime moduleStartTime) => new()
    {
        TotalGreetings = totalGreetings,
        TotalInteractions = totalInteractions,
        MostPopularGreeting = mostPopular,
        AverageUsage = Math.Round(avgUsage, 2),
        ModuleUptime = DateTime.UtcNow - moduleStartTime
    };
} 