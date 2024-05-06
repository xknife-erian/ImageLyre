using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ImageLyre.UI.Controls.Win
{
    public static class VisualStates
    {
        public static VisualStateGroup TryGetVisualStateGroup(DependencyObject dependencyObject, string groupName)
        {
            var root = GetImplementationRoot(dependencyObject);
            if (root == null) return null;

            return VisualStateManager
                .GetVisualStateGroups(root)?
                .OfType<VisualStateGroup>()
                .FirstOrDefault(group => string.CompareOrdinal(groupName, group.Name) == 0);
        }

        public static FrameworkElement GetImplementationRoot(DependencyObject dependencyObject)
        {
            Debug.Assert(dependencyObject != null, "DependencyObject should not be null.");
            return 1 == VisualTreeHelper.GetChildrenCount(dependencyObject)
                ? VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement
                : null;
        }
    }
}