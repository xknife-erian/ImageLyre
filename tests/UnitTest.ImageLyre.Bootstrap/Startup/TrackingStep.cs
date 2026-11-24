using ImageLyre.Bootstrap.Startup;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>记录执行与回滚顺序的最小 Step 替身，供共享执行器与增长压测构造可断言的调用轨迹。</summary>
internal sealed class TrackingStep : IBootstrapStep<string>
{
    private readonly List<string> _executions;
    private readonly List<string> _rollbacks;
    private readonly StepOutcome _outcome;
    private readonly Exception? _error;
    private readonly IReadOnlyList<StepDiagnostic>? _diagnostics;
    private readonly Exception? _executeException;
    private readonly Action? _onExecute;

    public TrackingStep(
        string id,
        string group,
        StepOutcome outcome,
        List<string> executions,
        List<string> rollbacks,
        bool canRollback = false,
        Exception? error = null,
        IReadOnlyList<StepDiagnostic>? diagnostics = null,
        Exception? executeException = null,
        Action? onExecute = null)
    {
        Id = id;
        Group = group;
        CanRollback = canRollback;
        Phase = StepPhase.Host;
        _outcome = outcome;
        _error = error;
        _diagnostics = diagnostics;
        _executeException = executeException;
        _onExecute = onExecute;
        _executions = executions;
        _rollbacks = rollbacks;
    }

    public string Id { get; }

    public string Group { get; }

    public StepPhase Phase { get; }

    public bool CanRollback { get; }

    public ValueTask<StepResult> ExecuteAsync(string context, CancellationToken cancellationToken)
    {
        _executions.Add(Id);
        _onExecute?.Invoke();

        if (_executeException is not null)
        {
            throw _executeException;
        }

        return ValueTask.FromResult(new StepResult(_outcome, _error, _diagnostics));
    }

    public ValueTask RollbackAsync(string context, CancellationToken cancellationToken)
    {
        _rollbacks.Add(Id);
        return ValueTask.CompletedTask;
    }
}
