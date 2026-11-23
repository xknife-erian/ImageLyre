namespace ImageLyre.Shell.Contracts;

/// <summary>主窗体标记契约：实现类型必须是 <c>Avalonia.Controls.Window</c>。</summary>
/// <remarks>
/// <para>
/// 本接口是标记而不是工厂，与 Mercury 的 <c>IWorkbench</c> 同构：壳插件在 <c>Build()</c> 里构造窗口并把窗口本身作为服务产物返回，
/// 宿主取到产物后断言其类型为 <c>Window</c> 再挂到桌面，不通过接口方法创建窗口——因此换壳实现只换窗口类型，宿主代码不动。
/// </para>
/// <para>
/// 时序契约：槽位贡献必须发生在窗口构造期间，于是「取到壳体」等价于「槽位已贡献」，
/// 宿主随后可以安全地执行 <c>RegisterFeatureAttachments</c>；cangming v1 的 <c>AttachToSlot</c> 在槽位不存在时抛异常，
/// 这条时序是它成立的前提。
/// </para>
/// <para>
/// 生命周期：窗口关闭即释放，<see cref="IDisposable.Dispose"/> 由窗口自己的关闭流程调用；
/// 宿主不主动释放壳体，也不在关闭后继续访问它的槽位。
/// </para>
/// </remarks>
public interface IWorkbenchShell : IDisposable
{
}
