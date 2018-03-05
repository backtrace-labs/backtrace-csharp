using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backtrace.NetFrameworkExample.Model
{
    public class Node
    {
        public virtual char Key { get; set; }
        public virtual bool IsTerminal { get; set; }
        public virtual Node Parent { get; set; }
        public virtual Dictionary<char, Node> Children { get; set; }

        public Node(char key) : this(key, false) { }

        public Node(char key, bool isTerminal)
        {
            Key = key;
            IsTerminal = isTerminal;
            Children = new Dictionary<char, Node>();
        }

        /// <summary>
        /// Return the word at this node if the node is terminal; otherwise, return null
        /// </summary>
        public virtual string Word
        {
            get
            {
                if (!IsTerminal)
                    return null;

                var curr = this;
                var stack = new Stack<char>();

                while (curr.Parent != null)
                {
                    stack.Push(curr.Key);
                    curr = curr.Parent;
                }

                return new String(stack.ToArray());
            }

        }

        ///// <summary>
        ///// Returns an enumerable collection of terminal child nodes.
        ///// </summary>
        public virtual IEnumerable<Node> GetTerminalChildren()
        {
            foreach (var child in Children.Values)
            {
                if (child.IsTerminal)
                    yield return child;

                foreach (var grandChild in child.GetTerminalChildren())
                    if (grandChild.IsTerminal)
                        yield return grandChild;
            }
        }

        /// <summary>
        /// Remove this element upto its parent.
        /// </summary>
        public virtual void Remove()
        {
            IsTerminal = false;

            if (Children.Count == 0 && Parent != null)
            {
                Parent.Children.Remove(Key);

                if (!Parent.IsTerminal)
                    Parent.Remove();
            }
        }
    }
}
