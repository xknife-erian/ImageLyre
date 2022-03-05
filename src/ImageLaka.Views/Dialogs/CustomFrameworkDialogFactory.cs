
using MvvmDialogs;
using MvvmDialogs.DialogFactories;
using MvvmDialogs.DialogTypeLocators;
using MvvmDialogs.FrameworkDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

namespace ImageLaka.Views.Dialogs
{
    public class CustomFrameworkDialogFactory : DefaultFrameworkDialogFactory
    {
        public override IFrameworkDialog CreateOpenFileDialog(OpenFileDialogSettings settings)
        {
            return new CustomOpenFileDialog(settings);
        }
    }
}
