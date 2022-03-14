using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLaka.ViewModels
{
    public class LoggerWindowViewModel : ObservableRecipient
    {
        private int _viewCount;

        public int ViewCount
        {
            get => _viewCount++;
            set => _viewCount = value;
        }
    }
}
