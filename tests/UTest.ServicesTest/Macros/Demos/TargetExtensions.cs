namespace UTest.ServicesTest.Macros.Demos;

public static class TargetExtensions
{
    public const string PLAY = "Play\r\n";

    public static void Play(this TextTarget it)
    {
        it.Text += PLAY;
    }

    public static void UnPlay(this TextTarget it)
    {
        it.Text += it.Text.Replace(PLAY, string.Empty);
    }
}