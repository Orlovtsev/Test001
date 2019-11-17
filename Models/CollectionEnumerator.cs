// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* CollectionEnumerator.cs
 * 
 * © Orlovtsev, 2019
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
#endregion

namespace Orlovtsev.Models
{

    public partial class CollectionEnumerator : IEnumerator<object>
    {
        int index = -1;
        TreeCollection _collection;
        object currentObject;
        public object Current => currentObject;

        public CollectionEnumerator(TreeCollection collection)
        {
            _collection = collection;
            currentObject = default(object);
        }

        public void  Dispose()
        {
            Reset();
        }

        public bool MoveNext()
        {
            if (++index >= _collection.Count)
                return false;
            else
                currentObject = _collection[index];
            return true;
        }

        public void Reset()
        {
            index = -1;
        }

    }
}
