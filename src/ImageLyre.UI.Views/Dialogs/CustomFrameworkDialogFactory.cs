using MvvmDialogs.FrameworkDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

namespace ImageLyre.UI.Views.Dialogs
{
    public class CustomFrameworkDialogFactory : DefaultFrameworkDialogFactory
    {
        public override IFrameworkDialog CreateOpenFileDialog(OpenFileDialogSettings settings)
        {
            return new CustomOpenFileDialog(settings);
        }
    }
}
