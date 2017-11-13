using System;

namespace ModelHierarchyBuilder
{
    class Program
    {
        static void Main(string[] args)
        {           
            var hierarchyBuilder = new HierarchyBuilder(@"\\server12\Deepankar\Visualogyx.DomainModel.dll");

            hierarchyBuilder.Build("DomainModel.Models.");

            //Returns a linked list
            var list = hierarchyBuilder.CreateList();

            //Prints the list
            hierarchyBuilder.DisplayLinkedList();

            Console.ReadKey();
        }
    }
}
