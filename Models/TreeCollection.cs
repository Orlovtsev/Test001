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
using System.Collections;
#endregion

namespace Orlovtsev.Models
{

    public sealed partial class TreeCollection : ICollection<object>
    {
        #region Properties
        private TreeCollection parant = null;
        public TreeCollection Parant
        {
            get => parant;
            set
            {
                if(value is TreeCollection)
                    parant = value;
            }
        }

        public string GroupName { get; private set; }
        public string Name { get; private set; }
        public int Depth { get; private set; }
        public List<string> Parameters { get; }

        List<object> innerCollections;

        #endregion

        #region Constructor
        public TreeCollection(string groupName ,string collectionName, int depth, List<string> parameters)
        {
            GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
            Name = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            Depth = depth;
            innerCollections = new List<object>();
            Parameters = parameters;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Возвращает первую ветку, которая включает искомую группу
        /// </summary>
        /// <param name="groupName">Имя параметра</param>
        /// <returns></returns>
        public TreeCollection GetGroup(string groupName)
        {
            if (Parant != null && Parant.Name.Equals(groupName))
                return this;

            foreach(var obj in this)
            {
                if(obj is TreeCollection tree)
                {
                    if (tree.Name.Equals(groupName))
                        return tree;
                    else
                        return tree.GetGroup(groupName);
                }
            }
            return null;
        }

        public List<TreeCollection> GetGroups(string groupName)
        {
            if (this.GroupName == groupName)
                return new List<TreeCollection>() { this };
            else
            {

                List<TreeCollection> collections = new List<TreeCollection>();
                foreach (object obj in innerCollections)
                {
                    if(obj is TreeCollection tree)
                    {
                        if (tree.GroupName == groupName)
                            collections.Add(tree);
                        else
                        {
                            var list = tree.GetGroups(groupName);
                            if (list.Count != 0)
                                collections.AddRange(list);
                        }
                            
                    }
                }
                return collections;
            }
        }

        /// <summary>
        /// Возвращает элементы определенной коллекции
        /// </summary>
        /// <param name="collectionName">Имя коллекции</param>
        /// <returns></returns>
        public List<object> GetElements(string collectionName)
        {
            if (this.Name.Equals(collectionName))
                return this.innerCollections;
            else
            {
                var list = new List<object>();
                foreach(var obj in this)
                {
                    if (obj is TreeCollection tree)
                    {
                        if (tree.Name.Equals(collectionName))
                            list.AddRange(tree.ToList());
                        else
                            list.AddRange(tree.GetElements(collectionName));
                    }
                }
                if (list!= null && list.Any())
                    return list;
            }
            return null;
        }

        /// <summary>
        /// Возвращает все вложенные элементы, не являющиеся ветками
        /// </summary>
        /// <returns></returns>
        public List<object> GetElements()
        {
            List<object> elements = new List<object>();
            List<TreeCollection> collections = new List<TreeCollection>();
            foreach(var elem in this)
            {
                if (elem is TreeCollection collection)
                    collections.Add(collection);
                else
                    elements.Add(elem);
            }

            if(collections.Count != 0)
            {
                foreach (var collection in collections)
                    elements.AddRange(collection.GetElements());
            }

            return elements;
                
        }

        #endregion

        #region ICollection

        public int Count => innerCollections.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public object this[int index]
        {
            get => (object)innerCollections[index];
            set { innerCollections[index] = value; }
        }

        public void Add(object item)
        {
            this.innerCollections.Add(item);
        }

        public void Clear()
        {
            this.innerCollections.Clear();
        }

        public bool Contains(object item)
        {
            if(item is TreeCollection collection)
                foreach (var innerCollection in innerCollections)
                    if (collection.Equals(innerCollection))
                        return true;
            return false;
        }

        public bool ContainsCollectionName(string collectionName)
        {
            if (Depth < Parameters.Count - 1)
                foreach (TreeCollection tree in innerCollections)
                    return tree.Name == collectionName;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is TreeCollection collection)
                return collection.Name == this.Name;
            return false;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new CollectionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CollectionEnumerator(this);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < innerCollections.Count; i++)
            {
                array[i + arrayIndex] = innerCollections[i];
            }
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        #endregion
    }
}
