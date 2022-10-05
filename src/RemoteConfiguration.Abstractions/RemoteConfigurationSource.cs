using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration.Abstractions;

public abstract class RemoteConfigurationSource : IConfigurationSource
{
    /// <summary>Determines if loading the file is optional.</summary>
    public bool Optional { get; set; }

    /// <summary>
    /// Determines whether the source will be loaded if the underlying file changes.
    /// </summary>
    public bool ReloadOnChange { get; set; }

    /// <summary>
    /// Number of milliseconds that reload will wait before calling Load.  This helps
    /// avoid triggering reload before a file is completely written. Default is 250.
    /// </summary>
    public int ReloadDelay { get; set; } = 250;

    /// <summary>
    /// Builds the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" />.</param>
    /// <returns>A <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /></returns>
    public abstract IConfigurationProvider Build(IConfigurationBuilder builder);
}