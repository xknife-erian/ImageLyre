using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using ImageLaka;
using ImageLaka.Services.Macros;
using UTest.ServicesTest.Macros.Demos;
using Xunit;

namespace UTest.ServicesTest.Macros.Commands
{
    public class PlayCommandTest
    {
        private const string TEST_TXT = TargetExtensions.PLAY;

        [Fact]
        public void Test1()
        {
            var tt = new TextTarget();//实际项目中，将使用Image作为目标；为方便测试，此处采用Text作为目标（不保存）
            //实例一个宏
            var macro = new Macro();
            var cs = new List<IMacroBeat>();

            //连续执行10次宏，也就是定义了10个命令
            var count = 10;
            for (int i = 0; i < 10; i++)
            {
                var c = new PlayBeat(tt);
                cs.Add(c);
            }
            macro.DoCurrent(cs.ToArray());

            var actual = Regex.Matches(tt.Text, TEST_TXT).Count;

            //期待10次迭加的字符串是正确的。
            actual.Should().Be(count);
            tt.Text.Length.Should().Be(count * TEST_TXT.Length);
        }
    }
}
