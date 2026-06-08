using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Ribbon
{
    /// <summary>
    /// Helper class for creating Revit ribbon UI elements.
    /// Centralizes button creation logic to keep App.cs clean.
    /// </summary>
    public static class RibbonHelper
    {
        /// <summary>
        /// Creates a standard PushButtonData with tooltip.
        /// </summary>
        public static PushButtonData CreatePushButtonData(
            string internalName,
            string displayText,
            string fullClassName,
            string tooltip)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                internalName,
                displayText,
                assemblyPath,
                fullClassName)
            {
                ToolTip = tooltip
            };

            return buttonData;
        }

        /// <summary>
        /// Creates a PushButtonData with large and small icons from embedded resources.
        /// </summary>
        public static PushButtonData CreatePushButtonData(
            string internalName,
            string displayText,
            string fullClassName,
            string tooltip,
            Bitmap largeIcon,
            Bitmap smallIcon)
        {
            var buttonData = CreatePushButtonData(
                internalName, displayText, fullClassName, tooltip);

            if (largeIcon != null)
                buttonData.LargeImage = BitmapToBitmapSource(largeIcon);

            if (smallIcon != null)
                buttonData.Image = BitmapToBitmapSource(smallIcon);

            return buttonData;
        }

        /// <summary>
        /// Adds a PushButton to an existing RibbonPanel.
        /// </summary>
        public static PushButton AddPushButton(
            RibbonPanel panel,
            string internalName,
            string displayText,
            string fullClassName,
            string tooltip)
        {
            var buttonData = CreatePushButtonData(
                internalName, displayText, fullClassName, tooltip);

            return panel.AddItem(buttonData) as PushButton;
        }

        /// <summary>
        /// Adds a PushButton with large (32x32) and small (16x16) icons from embedded resources.
        /// </summary>
        public static PushButton AddPushButton(
            RibbonPanel panel,
            string internalName,
            string displayText,
            string fullClassName,
            string tooltip,
            Bitmap largeIcon,
            Bitmap smallIcon)
        {
            var buttonData = CreatePushButtonData(
                internalName, displayText, fullClassName, tooltip,
                largeIcon, smallIcon);

            return panel.AddItem(buttonData) as PushButton;
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap (from Resources) to a WPF BitmapSource
        /// that the Revit ribbon API expects.
        /// </summary>
        internal static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
