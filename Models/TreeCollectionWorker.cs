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
#endregion

namespace Orlovtsev.Models
{

    public sealed partial class TreeCollectionWorker
    {
        #region Methods
        public static void AddElement(Element element, string collectionName, TreeCollection tree)
        {
            if (tree.Depth == tree.Parameters.Count)
                tree.Add(element);
            else
            {
                if (tree.Count == 0)
                {
                    var branch = CreateBranch(tree, element);
                    tree.Add(branch);
                }
                else
                {
                    bool input = true;
                    foreach (object obj in tree)
                    {
                        if(obj is TreeCollection innerCollection)
                        {
                            if (innerCollection.Name == collectionName)
                            {
                                var index = innerCollection.Depth;
                                if (++index < tree.Parameters.Count)
                                {
                                    var parameterName = tree.Parameters[index];
                                    var innerCollectionName = ParameterUtil.GetParameterValue(element, parameterName);
                                    AddElement(element, innerCollectionName, innerCollection);
                                    input = false;
                                }
                                else
                                {
                                    tree.Add(element);
                                    input = false;
                                    return;
                                }
                                
                            }
                        }
                        else
                        {
                            tree.Add(element);
                            input = false;
                            return;
                        }
                        
                    }
                    if (input)
                    {
                        var brunch = CreateBranch(tree, element);
                        tree.Add(brunch);
                    }
                }

            }

        }

        public static TreeCollection CreateBranch(TreeCollection tree, Element element)
        {
            TreeCollection branch = null;
            var depth = tree.Depth;
            var index = ++depth;
            if (index == tree.Parameters.Count)
            {
                tree.Add(element);
                return branch;
            }
            else
            {
                var groupName = tree.Parameters[index];
                var collectionName = ParameterUtil.GetParameterValue(element, groupName);
                branch = new TreeCollection(groupName, collectionName, index, tree.Parameters);
                branch.Parant = tree;
                var nextBranch = CreateBranch(branch, element);
                if (nextBranch != null)
                    branch.Add(nextBranch);
                return branch;
            }
        }

        #endregion
    }
}
