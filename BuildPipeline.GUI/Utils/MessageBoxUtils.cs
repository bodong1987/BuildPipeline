using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace BuildPipeline.GUI.Utils
{
    /// <summary>
    /// Class MessageBoxUtils.
    /// </summary>
    public static class MessageBoxUtils
    {
        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        public static async Task<ButtonResult> ShowErrorAsync(Window parent, string title, string text)
        {
            return await ShowMessageBoxAsync(parent, title, text, ButtonEnum.Ok, Icon.Error);
        }

        /// <summary>
        /// Shows the warning.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        public static async Task<ButtonResult> ShowWarningAsync(Window parent, string title, string text)
        {
            return await ShowMessageBoxAsync(parent, title, text, ButtonEnum.Ok, Icon.Warning);
        }

        /// <summary>
        /// Show tips as an asynchronous operation.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        /// <returns>A Task&lt;ButtonResult&gt; representing the asynchronous operation.</returns>
        public static async Task<ButtonResult> ShowTipsAsync(Window parent, string title, string text)
        {
            return await ShowMessageBoxAsync(parent, title, text, ButtonEnum.Ok, Icon.Info);
        }

        /// <summary>
        /// Show message box as an asynchronous operation.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        /// <param name="enum">The enum.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="windowStartupLocation">The window startup location.</param>
        /// <returns>A Task&lt;ButtonResult&gt; representing the asynchronous operation.</returns>
        public static async Task<ButtonResult> ShowMessageBoxAsync(
            Window parent,
            string title,
            string text,
            ButtonEnum @enum = ButtonEnum.Ok,
            Icon icon = Icon.None,
            WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterOwner
            )
        {    
            var window = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                GetStandardParams(
                        parent,
                        title,
                        text,
                        @enum,
                        icon,
                        windowStartupLocation
                    )
                );

            return await window.ShowWindowDialogAsync(parent);
        }

        private static MsBox.Avalonia.Dto.MessageBoxStandardParams GetStandardParams(
            Window parent,
            string title,
            string text,
            ButtonEnum @enum = ButtonEnum.Ok,
            Icon icon = Icon.None,
            WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterOwner
            )
        {
            var @params = new MsBox.Avalonia.Dto.MessageBoxStandardParams
            {
                ContentTitle = title,
                ContentMessage = text,
                ButtonDefinitions = @enum,
                Icon = icon,
                WindowStartupLocation = windowStartupLocation,
                WindowIcon = parent.Icon,
                FontFamily = FontUtils.ChineseFontFamily
            };


            return @params;
        }
    }
}
