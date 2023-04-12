using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace LaunchPass
{
    public class LaunchPassCustomPanel : Panel
    {
        protected override Size ArrangeOverride(Size finalSize)
        {
            double y = 0;
            double rowHeight = 0;
            double x = 0;
            foreach (var child in Children)
            {
                var desiredSize = child.DesiredSize;

                if ((x + desiredSize.Width) > finalSize.Width)
                {
                    y += rowHeight + 4;
                    x = 0;
                    rowHeight = 0;
                }

                child.Arrange(new Rect(x, y, desiredSize.Width, desiredSize.Height));
                x += desiredSize.Width + 4;

                rowHeight = Math.Max(rowHeight, desiredSize.Height);
            }

            return new Size(finalSize.Width, y + rowHeight);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double maxWidth = 0;
            double maxHeight = 0;
            double currentWidth = 0;
            double currentRowMaxHeight = 0;

            foreach (var child in Children)
            {
                child.Measure(availableSize);
                var desiredSize = child.DesiredSize;

                if (currentWidth + desiredSize.Width > availableSize.Width)
                {
                    maxHeight += currentRowMaxHeight + 4;
                    maxWidth = Math.Max(maxWidth, currentWidth);
                    currentWidth = 0;
                    currentRowMaxHeight = 0;
                }

                currentWidth += desiredSize.Width + 4;
                currentRowMaxHeight = Math.Max(currentRowMaxHeight, desiredSize.Height);
            }

            maxHeight += currentRowMaxHeight;
            maxWidth = Math.Max(maxWidth, currentWidth > 0 ? currentWidth - 4 : 0);

            return new Size(maxWidth, maxHeight);
        }
    }
}