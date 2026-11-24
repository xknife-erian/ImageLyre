namespace ImageLyre.Bootstrap.Startup;

/// <summary>一次启动的内存报告：按加入顺序保存每条步骤记录，并支持按组取子集。</summary>
/// <remarks>
/// <para>
/// 本类型只做聚合，不决定结果策略；Pipeline 在每步结束后追加记录，失败与回滚轨迹也由它填写。
/// 实例不是线程安全的，按 Plan 顺序串行写入；<see cref="Records"/> 返回实时视图，<see cref="ForGroup"/> 每次返回独立快照。
/// </para>
/// </remarks>
public sealed class StartupReport
{
    private readonly List<StepRecord> _records = [];

    /// <summary>按加入顺序排列的全部步骤记录。</summary>
    public IReadOnlyList<StepRecord> Records => _records;

    /// <summary>追加一条步骤记录。</summary>
    /// <param name="record">步骤结果快照，调用方负责填入耗时、异常类型与回滚状态。</param>
    /// <exception cref="ArgumentNullException"><paramref name="record"/> 为 <see langword="null"/>。</exception>
    public void Add(StepRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _records.Add(record);
    }

    /// <summary>取出指定组内的步骤记录，保持加入顺序。</summary>
    /// <param name="group">组名，与 <see cref="StepRecord.Group"/> 精确匹配。</param>
    /// <returns>该组的记录快照；组不存在时返回空集合。</returns>
    /// <exception cref="ArgumentException"><paramref name="group"/> 为 <see langword="null"/>、空串或空白。</exception>
    public IReadOnlyList<StepRecord> ForGroup(string group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        return _records
            .Where(record => string.Equals(record.Group, group, StringComparison.Ordinal))
            .ToArray();
    }
}
