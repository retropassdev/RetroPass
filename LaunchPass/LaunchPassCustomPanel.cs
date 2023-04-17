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
            int imagesPerRow = 6;
            double x = 0;
            double y = 0;
            double[] rowHeights = new double[(Children.Count + imagesPerRow - 1) / imagesPerRow];
            int currentRow = 0;
            int imagesInCurrentRow = 0;

            foreach (var child in Children)
            {
                var desiredSize = child.DesiredSize;

                if (imagesInCurrentRow >= imagesPerRow || (x + desiredSize.Width) > finalSize.Width)
                {
                    currentRow++;
                    x = 0;
                    imagesInCurrentRow = 0;
                }

                rowHeights[currentRow] = Math.Max(rowHeights[currentRow], desiredSize.Height);
                imagesInCurrentRow++;
                x += desiredSize.Width + 4;
            }

            double totalHeight = rowHeights.Sum() + 4 * (rowHeights.Length - 1);
            double remainingSpace = finalSize.Height - totalHeight;
            double verticalMargin = remainingSpace / rowHeights.Length;

            x = 0;
            y = 0;
            currentRow = 0;
            imagesInCurrentRow = 0;

            foreach (var child in Children)
            {
                var desiredSize = child.DesiredSize;

                if (imagesInCurrentRow >= imagesPerRow || (x + desiredSize.Width) > finalSize.Width)
                {
                    currentRow++;
                    x = 0;
                    y += rowHeights[currentRow - 1] + verticalMargin;
                    imagesInCurrentRow = 0;
                }

                double yAdjustment = (rowHeights[currentRow] - desiredSize.Height) / 2;
                child.Arrange(new Rect(x, y + yAdjustment, desiredSize.Width, desiredSize.Height));

                x += desiredSize.Width + 4;
                imagesInCurrentRow++;
            }

            return finalSize;
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