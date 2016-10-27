using DotLiquid;
using System.Collections.Generic;

namespace VirtoCommerce.LiquidThemeEngine.Objects
{
    public class TreeNode : Drop
    {
        public TreeNode()
        {
            Children = new List<TreeNode>();
            Parents = new List<TreeNode>();
        }

        public string Id { get; set; }

        public int Level { get; set; }

        public string Title { get; set; }

        public int? Priority { get; set; }

        public ICollection<TreeNode> Children { get; set; }

        public ICollection<TreeNode> Parents { get; set; }
    }
}