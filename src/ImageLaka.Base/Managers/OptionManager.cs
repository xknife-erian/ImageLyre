using NKnife;

namespace ImageLaka.Managers;

public class OptionManager
{
    public void Initianize()
    {
        HabitData = new NKnife.HabitData();
    }

    public HabitData? HabitData { get; private set; }
}