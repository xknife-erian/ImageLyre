namespace ImageLyre.Bootstrap.Startup;

/// <summary>单步执行的四种结局，决定后续步骤是否继续以及已完成步骤是否回滚。</summary>
public enum StepOutcome
{
    /// <summary>正常完成，后续步骤继续，已持有资源的步骤参与失败回滚。</summary>
    Completed,

    /// <summary>被取消或主动中止，后续步骤不再执行；它本身不视为错误，也不进入异常类型统计。</summary>
    Aborted,

    /// <summary>硬失败，后续步骤不再执行，已完成且可回滚的步骤按逆序回滚，失败归属到本步骤 Id。</summary>
    Failed,

    /// <summary>软失败，后续步骤继续，不触发回滚；插件级失败与可降级能力用它表达，失败模块与原因写入诊断。</summary>
    Degraded,
}
