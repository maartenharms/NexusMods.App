using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.JsonConverters;
using NexusMods.Paths;

namespace NexusMods.DataModel.ArchiveContents;

/// <summary>
/// Information about what archives a file is contained in, so this is a back-index of the <see cref="AnalyzedArchive"/> entity.
/// </summary>
[JsonTypeId<ArchivedFiles>("761192F0-F1BE-4DB8-85C9-2DDAF4B54742")]
public record ArchivedFiles : Entity
{
    /// <inheritdoc />
    public override EntityCategory Category => EntityCategory.ArchivedFiles;
    
    /// <summary>
    /// Name of the archive this file is contained in.
    /// </summary>
    public required RelativePath File { get; init; }
    
    /// <summary>
    /// The file entry data for the NX block offset
    /// </summary>
    public required byte[] FileEntryData { get; init; }
}
