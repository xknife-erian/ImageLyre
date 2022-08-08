using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WPFSample.Panes;

namespace WPFSample
{
    public class WorkbenchViewModel: ObservableRecipient
    {

        public WorkbenchViewModel(HistogramSampleViewModel histogramSampleVm, 
            MatSampleViewModel matSampleVm,
            CamSampleViewModel camSampleVm)
        {
            HistogramSampleVm = histogramSampleVm;
            MatSampleVm = matSampleVm;
            CamSampleVm = camSampleVm;
        }

        public HistogramSampleViewModel HistogramSampleVm { get; set; }

        public MatSampleViewModel MatSampleVm { get; set; }

        public CamSampleViewModel CamSampleVm { get; set; }
    }
}
