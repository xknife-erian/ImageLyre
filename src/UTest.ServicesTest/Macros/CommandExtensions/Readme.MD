### 如何新增Command？如何为命令的接收目标`ITarget`定义命令函数？

```
public static class TargetExtensions
{
    public static void Play(this TextTarget it)
    {
        it.TestTarget += $"{nameof(Play)}/";
    }
    public static void UnPlay(this TextTarget it)
    {
        it.TestTarget += $"{nameof(UnPlay)}/";
    }
}
```
**采用扩展方法可以很好的在未来继续不断增加命令，详细可见`PlayCommand`类中的使用方法**