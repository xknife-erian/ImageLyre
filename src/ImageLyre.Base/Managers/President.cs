namespace ImageLyre.Managers
{
    /// <summary>
    /// 总裁(president)。管理所有的经理(manager)。
    /// </summary>
    public class President
    {
        public President(OptionManager optionManager, ConsoleManager consoleManager)
        {
            OptionManager = optionManager;
            ConsoleManager = consoleManager;
        }

        public OptionManager OptionManager { get; set; }

        public ConsoleManager ConsoleManager { get; set; }
    }
}
