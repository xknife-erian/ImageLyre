using NKnife;

namespace ImageLad.Managers;

/// <summary>
/// 选项与用户习惯服务
/// </summary>
public class OptionManager
{
    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        HabitData = new NKnife.HabitData();
    }

    /// <summary>
    /// 用户使用习惯
    /// </summary>
    public HabitData? HabitData { get; set; }
}