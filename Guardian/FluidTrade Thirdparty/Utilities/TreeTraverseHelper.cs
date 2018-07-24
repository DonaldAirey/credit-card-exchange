namespace FluidTrade.Thirdparty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>t
    /// Extension methods to traverse a hierachal data collection. />
    /// </summary>
    public static class TreeTraverseHelper 
    {
        /// <summary>
        /// Class to hold the results
        /// </summary>
        /// <typeparam name="T">The type of the item in the node.</typeparam>
        public class Node<T>
        {
            /// <summary>
            /// Create an empty node.
            /// </summary>
            internal Node()
            {
            }

            /// <summary>
            /// The tree level this node resides in.
            /// </summary>
            public int Level;
            /// <summary>
            /// The direct parent of this node.
            /// </summary>
            public Node<T> Parent;
            /// <summary>
            /// The item contained in this node.
            /// </summary>
            public T Item;
        }

        
        /// <summary>
        /// Search the tree and create a path to the node.
        /// Usage:
        ///  Class Tree : IEnumarable&lt;Tree&gt;
        ///  {
        ///     String Name;
        ///     IEnumerable&lt;Tree&gt; Children ( get;}
        ///  }
        ///  
        /// Tree t;
        /// t.FindLeafNode(x => x.Name == nameToFind, (parent) => parent.Children); 
        ///     
        /// </summary>
        /// <typeparam name="T">Type this method operates on.  ('Tree' in above example)</typeparam>
        /// <param name="source">Source object. ('t')</param>
        /// <param name="predicate">Condition to test. ('x => x.Name == nameToFind') (</param>
        /// <param name="connectBy">Parent to child relationship. ('(parent) => parent.Children')</param>
        /// <returns>The indicated node, or null if it is not found.</returns>
        public static Node<T> FindLeafNode<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate,
            Func<T, IEnumerable<T>> connectBy)
        {
            return source.FindLeafNode<T>(predicate, connectBy, null);
        }


        /// <summary>
        /// Recursively search the tree to determine path to the node.
        /// </summary>
        /// <typeparam name="T">Type this method works on</typeparam>
        /// <param name="source">Type this method operates on.</param>
        /// <param name="predicate">Source object.</param>
        /// <param name="connectBy">Parent to child relationship.</param>
        /// <param name="parent">The root to search in.</param>
        /// <returns>The indicated node, or null if it is not found.</returns>
        private static Node<T> FindLeafNode<T>(
           this IEnumerable<T> source,
           Func<T, bool> predicate,
           Func<T, IEnumerable<T>> connectBy,
           Node<T> parent)
        {
          
            //Sanity checks.
            if (source == null)
                throw new ArgumentNullException("source");

            if (predicate == null)
                throw new ArgumentNullException("predicate");

            if (connectBy == null)
                throw new ArgumentNullException("connectBy");

            int level = (parent == null ? 0 : parent.Level + 1);
            IEnumerable<T> foundNodes = source.Where(predicate);

            if (foundNodes != null &&  foundNodes.Count() > 0)
            {
                Node<T> newNode = new Node<T> { Level = level, Parent = parent, Item = foundNodes.First() };
                return newNode;
            }

            //Not found, so recursively check the tree
            foreach (T subNode in source)
            {
                Node<T> newNode = new Node<T> { Level = level, Parent = parent, Item = subNode };
                Node<T> foundNode = connectBy(subNode).FindLeafNode<T>(predicate, connectBy, newNode);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;

        }

    }

}
