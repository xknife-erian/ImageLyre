using ImageLyre.Bootstrap.Startup;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>可配置结果的最小 Step 替身，用来构造 Plan 与模拟执行结果。</summary>
internal sealed class RecordingStep : IBootstrapStep<string>
{
    private readonly StepResult _result;

    public RecordingStep(
        string id,
        string group,
        StepOutcome outcome = StepOutcome.Completed,
        Exception? error = null,
        bool canRollback = false,
        StepPhase phase = StepPhase.Host)
    {
        Id = id;
        Group = group;
        CanRollback = canRollback;
        Phase = phase;
        _result = new StepResult(outcome, error);
    }

    public string Id { get; }

    public string Group { get; }

    public StepPhase Phase { get; }

    public bool CanRollback { get; }

    public ValueTask<StepResult> ExecuteAsync(string context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(_result);

    public ValueTask RollbackAsync(string context, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;
}
