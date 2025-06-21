namespace MicFx.Web.Admin.Services
{
    /// <summary>
    /// Result of admin module scanning operation
    /// </summary>
    public class AdminScanResult
    {
        public int ScannedAssemblies { get; set; }
        public List<string> AssemblyNames { get; set; } = new();
        public List<ContributorInfo> Contributors { get; set; } = new();
    }

    /// <summary>
    /// Information about a discovered admin navigation contributor
    /// </summary>
    public class ContributorInfo
    {
        public string TypeName { get; set; } = string.Empty;
        public string AssemblyName { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
    }
} 