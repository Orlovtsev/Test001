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
#endregion

namespace Orlovtsev.Models
{

    public sealed partial class ParameterUtil
    {


        public static Dictionary<string, object> ParametersDefinition =
            new Dictionary<string, object>()
            {
                {"BS_Блок", new Guid("aae263eb-7d0d-4e32-93e6-c4afca2de2b6") },
                {"Уровень", BuiltInParameter.ROOM_LEVEL_ID },
                {"ROM_Зона", new Guid("ac5c4e67-e8bc-4411-a4a4-c5941ceedc01") },
                {"ROM_Подзона", new Guid("a3937d51-58e4-4971-adc1-9882299a7974") },
                { "ROM_Расчетная_подзона_ID", new Guid("80ea3554-5a78-4f4c-ad23-ebbf92ef5320")},
                { "ROM_Подзона_Index", new Guid("1acad438-db9d-498d-9ac2-c786f562d880") },
            };

        public static string GetParameterValue(Element element, string name)
        {
            string valueAsString = "Invalid";
            if (!ParametersDefinition.ContainsKey(name))
                return valueAsString;
            object definition = ParametersDefinition[name];
            Parameter parameter = null;
            if (definition is Guid guid)
                parameter = element.get_Parameter(guid);
            else if (definition is BuiltInParameter builtInParameter)
                parameter = element.get_Parameter(builtInParameter);

            if (parameter == null) return valueAsString;

            switch (parameter.StorageType)
            {
                case StorageType.String:
                    valueAsString = parameter.AsString(); break;
                case StorageType.ElementId:
                    valueAsString = parameter.AsValueString(); break;
                case StorageType.Integer:
                    valueAsString = parameter.AsInteger().ToString(); break;
                case StorageType.Double:
                    valueAsString = parameter.AsDouble().ToString(); break;
                default:
                    valueAsString = parameter.AsString();break;
            }
            return valueAsString;
        }

        public static void SetValue(IEnumerable<Element> elements, string name, object value)
        {
            if (!elements.Any())
                return;
            
            foreach(var element in elements)
            {
                var parameter = GetParameter(element, name);
                if (parameter == null)
                    throw new NullReferenceException($"Параметр {name} у элементов не найден");
                SetParameterValue(parameter, value);
            }
        }

        private static void SetParameterValue(Parameter parameter, object value)
        {
            if (value is int iVal)
                parameter.Set(iVal);
            else if (value is double dVal)
                parameter.Set(dVal);
            else if (value is string sVal)
                parameter.Set(sVal);
            else if (value is ElementId id)
                parameter.Set(id);
        }

        private static Parameter GetParameter(Element element, string name)
        {
            if (!ParametersDefinition.ContainsKey(name))
                throw new ArgumentException($"Параметр {name} в коллекции {nameof(ParametersDefinition)} класса {nameof(ParameterUtil)} не обозначен");
            object definition = ParametersDefinition[name];
            Parameter parameter = null;
            if (definition is Guid guid)
                parameter = element.get_Parameter(guid);
            else if (definition is BuiltInParameter builtInParameter)
                parameter = element.get_Parameter(builtInParameter);
            return parameter;
        }
    }
}
