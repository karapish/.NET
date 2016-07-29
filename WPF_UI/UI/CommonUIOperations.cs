
namespace TellusResourceAllocatorManagement.UI
{
    using System.Windows;
    using System.Windows.Media;

    public static class CommonUIOperations
    {
        /// <summary>
        /// Finds a parent control of type <ref>T</ref> in a visual WPF hierarchy
        /// </summary>
        /// <typeparam name="T">Type to find (e.g. ScrollViewer)</typeparam>
        /// <param name="child">Object to use as a start</param>
        /// <returns>Null if type <ref>T</ref> is not found or <ref>T</ref></returns>
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;

            return parent ?? FindVisualParent<T>(parentObject);
        }

        /// <summary>
        /// Finds a first instance of a child parent control of type <ref>T</ref> in a visual WPF hierarchy
        /// </summary>
        /// <typeparam name="T">Type to find (e.g. ScrollViewer)</typeparam>
        /// <param name="parent">Object to use as a start</param>
        /// <returns>Null if type <ref>T</ref> is not found or <ref>T</ref></returns>
        public  static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);

            if (count == 0)
                return null;

            for (int i = 0; i < count; ++i)
            {
                DependencyObject childObject = VisualTreeHelper.GetChild(parent, i);

                if (childObject == null)
                    continue;

                T child = childObject as T;
                if (child != null)
                {
                    // found!
                    return child;
                }
            }

            // Reached here? Didn't find child of type T            
            // Invoke for all children recursively 
            for (int i = 0; i < count; ++i)
            {
                DependencyObject childObject = VisualTreeHelper.GetChild(parent, i);

                if (childObject == null)
                    continue;

                T childOfChild = FindVisualChild<T>(childObject);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }
    }
}
