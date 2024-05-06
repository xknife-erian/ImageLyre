namespace ImageLyre;

/// <summary>
/// 描述一个被操作的目标
/// </summary>
public interface ITarget
{
    /// <summary>
    /// 打开目标，以保证目标可操作。与<see cref="Open"/>相对应。
    /// </summary>
    public void Open();

    /// <summary>
    /// 关闭目标，以保证目标在下次打开<see cref="Open"/>前不再会被操作。
    /// </summary>
    public void Close();
}