
#region Namespaces
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
using Orlovtsev.Models;


#endregion

namespace Orlovtsev.Externals
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HalfToneCommand : IExternalCommand
    {
        static string filteredParameterName = "ROM_Зона";
        static string filteredParameterValue = "Квартира";
        static int startIndex = -1;
        static string startTreeName = "root";
        static string startGroupName = "Building";

        static string subzoneId = "ROM_Расчетная_подзона_ID";
        static string subzoneIndex = "ROM_Подзона_Index";

        public static List<string> ParametersLine = new List<string>
        {
            "BS_Блок",
            "Уровень",
            "ROM_Зона",
            "ROM_Подзона",
        };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var result = Result.Succeeded;

            try
            {
                var ui_app = commandData.Application;
                var ui_doc = ui_app.ActiveUIDocument;
                var doc = ui_doc.Document;

                var roomCollection = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Rooms)
                        .WhereElementIsNotElementType()
                        .Cast<Room>()
                        .Where(x =>
                        {
                            if (x.Location != null)
                            {
                                var zoneValue = x.LookupParameter(filteredParameterName)?.AsString();
                                if (!string.IsNullOrEmpty(zoneValue))
                                    return zoneValue.Contains(filteredParameterValue);
                            }
                            return false;
                        })
                        .ToList();

                if (roomCollection.Count() > 0)
                {
                    var tree = new TreeCollection(startGroupName, startTreeName, startIndex, ParametersLine);
                    var parameterName = tree.Parameters[0];
                    foreach (var room in roomCollection)
                    {
                        var collectionName = ParameterUtil.GetParameterValue(room, parameterName);
                        TreeCollectionWorker.AddElement(room, collectionName, tree);
                    }

                    var levelGroupName = tree.Parameters[1];
                    var appartmentType = tree.Parameters[3];
                    var collections = tree.GetGroups(levelGroupName);
                    var sb = new StringBuilder();
                    using (var tr = new Transaction(doc, "Помещения"))
                    {
                        tr.Start();
                        int counter = 1;
                        
                        foreach (TreeCollection appartment in collections)
                        {
                            var sortedAppList = appartment.Cast<TreeCollection>().OrderBy(x => x.Name);
                            TreeCollection temp = null;
                            foreach (var app in sortedAppList)
                            {
                                var type = app.GetGroups(appartmentType).FirstOrDefault();
                                if (temp == null)
                                    temp = type;
                                else
                                {
                                    if (string.Compare(temp.Name, type.Name) == 0)
                                    {
                                        counter++;
                                        if (counter % 2 == 0)
                                        {
                                            var rooms = app.GetElements().Cast<Element>();
                                            var room = rooms.First();
                                            var subValue = ParameterUtil.GetParameterValue(room, subzoneId);
                                            ParameterUtil.SetValue(rooms, subzoneIndex, $"{subValue}.Полутон");
                                        }
                                    }
                                    else
                                        counter = 1;
                                    temp = type;
                                }
                            }
                        }
                        tr.Commit();
                    }

                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                sb.AppendLine();
                sb.AppendLine($"Тип ошибки {ex.GetType()}");
                sb.AppendLine();
                sb.AppendLine(ex.StackTrace);
                WPF.MessageBox.Show(sb.ToString());

                result = Result.Failed;
            }

            return result;
        }


    }
}
