using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageLaka.ImageEngine;
using ImageLaka.Services.Macros;

namespace UTest.ServicesTest.Macros.Demos
{
    public class DemoViewModel
    {
        private TextTarget _textTarget;
        private Macro _macro;

        public string Text { get; set; }

        public void Read()
        {
            _macro = new Macro();
            _textTarget = new TextTarget();
            var beat = new PlayBeat(_textTarget);
            _macro.DoCurrent(beat);
            Text = _textTarget.Text;
        }

        public void Play()
        {
            var beat = new PlayBeat(_textTarget);
            _macro.DoCurrent(beat);
            Text = _textTarget.Text;
        }
    }
}
