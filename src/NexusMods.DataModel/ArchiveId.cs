﻿using Vogen;

namespace NexusMods.DataModel;

[ValueObject<Guid>]
public partial struct ArchiveId
{

    /// <summary>
    /// Create a new download id, randomly generated
    /// </summary>
    /// <returns></returns>
    public static ArchiveId New() => From(Guid.NewGuid());

}
