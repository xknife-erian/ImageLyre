using LyreSample.Panes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LyreSample
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
