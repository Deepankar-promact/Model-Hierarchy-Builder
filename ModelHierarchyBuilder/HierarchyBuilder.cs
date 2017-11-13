using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace ModelHierarchyBuilder
{
    public class HierarchyBuilder
    {
        private Assembly assembly;
        private Dictionary<Type, List<Type>> modelDictionary;
        private IList<string> modelList;
        private Tree FullTree;
        private int TreeHeight = 0;

        /// <summary>
        /// Setup hierarchy builder
        /// </summary>
        /// <param name="assemblyPath">Path to dll file</param>
        public HierarchyBuilder(string assemblyPath)
        {
            assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            modelList = new List<string>();
            modelDictionary = new Dictionary<Type, List<Type>>();
            FullTree = new Tree();
        }

        /// <summary>
        /// Build the model hierarchy
        /// </summary>
        /// <param name="namespaceToSearch">Partial or Full namespace to search</param>
        public void Build(string namespaceToSearch)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            //Traverse member classes
            foreach (var type in types.Where(x => x != null && x.FullName.Contains(namespaceToSearch)))
            {
                try
                {
                    var properties = type.GetRuntimeProperties();
                                        
                    List<Type> dependentModel = new List<Type>();
                    
                    foreach (var property in properties)
                    {
                        var customAttrs = property.GetCustomAttributes(true).Where(y => y.GetType().FullName.Contains("ForeignKey")).ToList();
                        if ( customAttrs.Count() > 0 && property.PropertyType.FullName.Contains("Int32"))
                        {
                            var customAttr = property
                                .GetCustomAttributes(true)
                                .FirstOrDefault(y => y.GetType().FullName.Contains("ForeignKey"));

                            var virtualPropertyName = customAttr.GetType().GetProperty("Name").GetValue(customAttr);
                            var virtualProperty = properties.SingleOrDefault(x => x.GetMethod.IsVirtual && x.Name.Equals(virtualPropertyName));

                            dependentModel.Add(virtualProperty.PropertyType);
                        }else if(customAttrs.Count() > 0)
                        {
                            dependentModel.Add(property.PropertyType);
                        }
                    }

                    if (dependentModel.Count() > 0)
                    {
                        modelDictionary.Add(type, dependentModel);
                    }
                }
                catch (Exception) { }
            }

            BuildTree();
        }
        
        /// <summary>
        /// Prints the model in a tree structure
        /// </summary>
        public void DisplayTreeList()
        {
            if(FullTree == null)
            {
                Console.WriteLine("No Data");
            }

            foreach (var tree in FullTree.Leaves)
            {
                Console.WriteLine(tree.Node);
                DisplaySubTree(tree, 0);
            }

            Console.WriteLine("\n Height: " + (TreeHeight + 1));
        }

        /// <summary>
        /// Creates and returns a list with models 
        /// arranged according to their dependency
        /// </summary>
        /// <returns>List<string></returns>
        public IList<string> CreateList()
        {
            for (int i = TreeHeight + 1; i > 0; i--)
            {
                CreateList(FullTree, i);
            }
            modelList.Remove("root");

            return modelList;
        }

        /// <summary>
        /// Prints the linked list
        /// </summary>
        public void DisplayLinkedList()
        {
            foreach (var model in modelList)
            {
                Console.WriteLine(model);
            }
        }

        private void BuildTree()
        {
            FullTree.Node = "root";
            FullTree.Leaves = new List<Tree>();
            foreach(var tree in modelDictionary)
            {
                Tree t = new Tree();
                t.Node = tree.Key.FullName;
                BuildTreeFor(tree.Key.FullName, tree.Key.FullName, t, 0);

                FullTree.Leaves.Add(t);                
            }
        }


        private void DisplaySubTree(Tree tree, int level)
        {
            if (tree.Leaves != null)
            {
                foreach (var leaf in tree.Leaves)
                {
                    for (int i = 0; i <= level; i++)
                        Console.Write("|\t");
                    Console.WriteLine("\u2514" + leaf.Node);
                    DisplaySubTree(leaf, level + 1);
                }
            }
        }       

        private void CreateList(Tree tree, int level)
        {
            if(level == 1 && !modelList.Contains(tree.Node))
            {
                modelList.Add(tree.Node);
            }
            else if(tree.Leaves != null)
            {
                foreach(var leaf in tree.Leaves)
                {
                    CreateList(leaf, level - 1);
                }
            }
        }
        
        private void BuildTreeFor(string root, string name, Tree t, int level)
        {
            var tree = modelDictionary.FirstOrDefault(x => x.Key.FullName.Equals(name));

            if(level > TreeHeight)
            {
                TreeHeight = level;
            }

            if (modelDictionary.Any(x => x.Key.FullName.Equals(name)))
            {
                foreach (var type in tree.Value)
                {
                    var typeName = (type.IsGenericType ? type.GetGenericArguments()[0].FullName : type.FullName);
                    Tree t1 = new Tree();
                    t1.Node = typeName;

                    if (t.Leaves == null)
                        t.Leaves = new List<Tree>();

                    t.Leaves.Add(t1);

                    BuildTreeFor(name, typeName, t1, level+1);
                }
            }
        }
    }
}
