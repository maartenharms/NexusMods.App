using DynamicData;
using NexusMods.CLI.DataOutputs;
using NexusMods.DataModel.Loadouts;
using NexusMods.DataModel.Loadouts.ApplySteps;
using NexusMods.DataModel.Loadouts.LoadoutSynchronizerDTOs.PlanStates;
using NexusMods.DataModel.Loadouts.Markers;
using NexusMods.Paths;

namespace NexusMods.CLI.Verbs;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Apply a Loadout to a game folder
/// </summary>
public class Apply : AVerb<LoadoutMarker, bool, bool>
{
    private readonly IRenderer _renderer;
    private readonly LoadoutSynchronizer _loadoutSyncronizer;

    /// <summary>
    /// DI constructor
    /// </summary>
    /// <param name="configurator"></param>
    /// <param name="loadoutSynchronizer"></param>
    public Apply(Configurator configurator, LoadoutSynchronizer loadoutSynchronizer)
    {
        _renderer = configurator.Renderer;
        _loadoutSyncronizer = loadoutSynchronizer;
    }

    /// <inheritdoc />
    public static VerbDefinition Definition => new("apply", "Apply a Loadout to a game folder", new OptionDefinition[]
    {
        new OptionDefinition<LoadoutMarker>("l", "loadout", "Loadout to apply"),
        new OptionDefinition<bool>("r", "run", "Run the application? (defaults to just printing the steps)"),
        new OptionDefinition<bool>("s", "summary", "Print the summary, not the detailed step list")
    });

    /// <inheritdoc />
    public async Task<int> Run(LoadoutMarker loadout, bool run, bool summary, CancellationToken token)
    {

        var plan = await _loadoutSyncronizer.Validate(loadout.Value, token);
        if (plan is IFailedValidation failedValidation)
        {
            await _renderer.Render($"Failed to validate loadout: {failedValidation.Message}");
            return -1;
        }

        var validation = (SuccessfulValidationResult)plan;
        if (summary)
        {
            var rows = new List<IEnumerable<object>>();
            rows.Add(new object[] {"Delete", validation.ToDelete.Count});
            rows.Add(new object[] {"Extract", validation.ToExtract.Count});
            rows.Add(new object[] {"Generate", validation.ToGenerate.Count});
            await _renderer.Render(new Table(new[] { "Action", "Count" }, rows));
        }
        else
        {
            var rows = new List<IEnumerable<object>>();
            foreach (var path in validation.ToDelete)
            {
                rows.Add(new object[] {"Delete", path});
            }

            foreach (var (path, hash) in validation.ToExtract)
            {
                rows.Add(new object[] {"Extract", path, hash});
            }

            foreach (var (path, generatedFileState) in validation.ToGenerate)
            {
                rows.Add(new object[] {"Generate", path, generatedFileState.Fingerprint});
            }
            await _renderer.Render(new Table(new[] { "Action", "Path", "Hash / Fingerprint" }, rows));
        }

        if (run)
        {
            await _renderer.WithProgress(token, async () =>
            {
                await _loadoutSyncronizer.Apply(validation, token);
                return 0;
            });
        }
        return 0;
    }
}
