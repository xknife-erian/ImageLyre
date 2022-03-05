using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLaka.ViewModels
{
    public class ImageWindowViewModel : ObservableRecipient
    {
        public string ImageSource { get; set; }
    }

}
