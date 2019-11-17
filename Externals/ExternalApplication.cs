// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* ExternalApplication.cs
 * None
 * © DarkTech, 2019
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */
#region Namespaces
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Resources;
using System.Reflection;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using WPF = System.Windows;
using System.Linq;
using System.Text;
using System.Windows.Forms;
#endregion

namespace Orlovtsev.Externals
{

    public sealed partial class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            var result = Result.Succeeded;

            return result;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            var result = Result.Succeeded;

            try
            {
                Initialize(application);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();

                sb.AppendLine(ex.Message);
                sb.AppendLine();
                sb.AppendLine(ex.HelpLink);
                sb.AppendLine();
                sb.AppendLine(ex.GetType().ToString());
                sb.AppendLine();
                sb.AppendLine(ex.StackTrace);
                MessageBox.Show(sb.ToString());
                result = Result.Failed;
            }
            return result;
        }

        private void Initialize(UIControlledApplication application)
        {
            string panelName = "Test";
            string tabName = "test001";

            var assembly = Assembly.GetExecutingAssembly();
            var location = assembly.Location;
            var panels = application.GetRibbonPanels();
            RibbonPanel panel = null;
            if (panels.Count(x => x.Name == panelName) > 0)
            {
                panel = panels.First(x => x.Name == panelName);
            }
            else
            {
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException argException) { }

                panel = application.CreateRibbonPanel(tabName, panelName);
            }

            PushButtonData data;

            data = new PushButtonData("HalfTone", "Полутона", location, "Orlovtsev.Externals.HalfToneCommand");
            var pbTrussCreate = panel.AddItem(data) as PushButton;
            pbTrussCreate.LargeImage = new BitmapImage(new Uri
            (Path.Combine(Path.GetDirectoryName(location) ?? throw new InvalidOperationException(),
                @"Resources/icons", "iconHalfTone.png")));
            pbTrussCreate.ToolTip = "Полутона помещений";

        }

    }
}
