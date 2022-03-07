using ImageLaka;

namespace ImageLaka.Services.Macros
{
    public class Macro
    {
        private LinkedList<BaseCommand> Commands;
        public Macro()
        {
            Commands = new LinkedList<BaseCommand>();
        }
        public void Add(BaseCommand dc)
        {
            Commands.AddLast(dc);
        }
        public void Remove(BaseCommand dc)
        {
            Commands.Remove(dc);
        }
        public void Do()
        {
            foreach (var command in Commands)
            {
                command.Do();
            }
        }
    }
}