/*
 * 작성자: 윤정도
 * 생성일: 6/12/2023 7:26:16 AM
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapleJaeMul
{
    public class ScrollViewerExt
    {
        // https://stackoverflow.com/questions/2337822/wpf-listbox-scroll-to-end-automatically
        public static ScrollViewer Find(DependencyObject root)
        {
            var queue = new Queue<DependencyObject>(new[] { root });

            do
            {
                var item = queue.Dequeue();

                if (item is ScrollViewer)
                    return (ScrollViewer)item;

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                    queue.Enqueue(VisualTreeHelper.GetChild(item, i));
            } while (queue.Count > 0);

            return null;
        }

        public static void EnableMoveToBottom(ScrollViewer viewer)
        {
            if (viewer == null) 
                return;

            viewer.ScrollChanged += (o, args) =>
            {
                if (args.ExtentHeightChange > 0)
                    viewer.ScrollToBottom();
            };
        }

    }
}
