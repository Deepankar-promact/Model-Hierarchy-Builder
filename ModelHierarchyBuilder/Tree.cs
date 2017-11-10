using System.Collections.Generic;

namespace ModelHierarchyBuilder
{
    public class Tree
    {
        public string Node { get; set; }

        public List<Tree> Leaves { get; set; }
    }
}
