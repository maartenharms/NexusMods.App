using NexusMods.DataModel.Loadouts.Markers;

namespace NexusMods.CLI.Types;

/// <summary>
/// Defines a protocol handler used for downloading items.
/// </summary>
public interface IDownloadProtocolHandler
{
    /// <summary>
    /// The protocol to handle, e.g. 'nxm'
    /// </summary>
    public string Protocol { get; }

    /// <summary>
    /// Handles downloads from the given URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="modName">Name of the mod to install.</param>
    /// <param name="token">Allows to cancel the operation.</param>
    /// <param name="loadout">Load to install the mod to.</param>
    public Task Handle(string url, LoadoutMarker loadout, string modName, CancellationToken token);
}
