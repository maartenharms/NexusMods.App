﻿using FluentAssertions;
using NexusMods.Common;
using NexusMods.FileExtractor.FileSignatures;
using NexusMods.Games.RedEngine.ModInstallers;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;

namespace NexusMods.Games.RedEngine.Tests.ModInstallers;

public class AppearancePresetTests : AModInstallerTest<Cyberpunk2077, AppearancePreset>
{
    public AppearancePresetTests(IServiceProvider serviceProvider) : base(serviceProvider) { }


    [Fact]
    public async Task PresetFilesAreInstalledCorrectly()
    {
        var hash = NextHash();
        var files = await BuildAndInstall(Priority.Normal,
            (hash, "cool_choom.preset", FileType.Cyberpunk2077AppearancePreset));

        files.Should()
            .BeEquivalentTo(new[]
            {
                (hash, GameFolderType.Game, "bin/x64/plugins/cyber_engine_tweaks/mods/AppearanceChangeUnlocker/character-preset/female/cool_choom.preset"), 
                (hash, GameFolderType.Game, "bin/x64/plugins/cyber_engine_tweaks/mods/AppearanceChangeUnlocker/character-preset/male/cool_choom.preset")
            });
    }
    
    [Fact]
    public async Task DocumentationFilesAreIgnored()
    {
        var hash = NextHash();
        var files = await BuildAndInstall(Priority.Normal,
            (hash, "cool_choom.preset", FileType.Cyberpunk2077AppearancePreset),
            (NextHash(), "README.md", FileType.TXT),
            (NextHash(), "README.txt", FileType.TXT),
            (NextHash(), "README.md", FileType.TXT),
            (NextHash(), "README.pdf", FileType.TXT));

        files.Should()
            .BeEquivalentTo(new[]
            {
                (hash, GameFolderType.Game, "bin/x64/plugins/cyber_engine_tweaks/mods/AppearanceChangeUnlocker/character-preset/female/cool_choom.preset"), 
                (hash, GameFolderType.Game, "bin/x64/plugins/cyber_engine_tweaks/mods/AppearanceChangeUnlocker/character-preset/male/cool_choom.preset")
            });
    }

}

