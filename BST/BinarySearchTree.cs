using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prep
{
    public class BinarySearchTree<T> where T: IComparable<T>
    {
        public class Node<J>
        {
            public Node<J> Left { get; set; }
            public Node<J> Right { get; set; }
            public T Value { get; set; }

            public bool IsRemoved { get; set; }
        }

        private Node<T> root = null;

        public BinarySearchTree<T> Add(T value)
        {
            var newNode = new Node<T>();
            newNode.IsRemoved = false;
         
            if(this.root == null)
            {
                // no nodes
                //
                newNode.Value = value;
                this.root = newNode;
            }
            else {
                // 1+ nodes already added
                //
                var node = this.root;
                Node<T> parent = null;
                bool wentLeft = false;

                while (node != null)
                {
                    parent = node;

                    if (value.CompareTo(node.Value) < 0)
                    {
                        // go to the left subtree
                        //
                        node = node.Left;
                        wentLeft = true;
                    }
                    else
                    {
                        node = node.Right;
                        wentLeft = false;
                    }
                }

                // add node
                //
                if(wentLeft)
                {
                    parent.Left = newNode;
                }
                else 
                {
                    parent.Right = newNode;
                }

                newNode.Value = value;
            }

            return this;
        }

        public void Remove(T value)
        {
            Node<T> current = this.root;
            while(current != null)
            {
                if (value.Equals(current.Value) &&
                    !current.IsRemoved)
                {
                    current.IsRemoved = true;
                    return;
                }
                else
                {
                    if (value.CompareTo(current.Value) < 0)
                    {
                        // go to the left subtree
                        //
                        current = current.Left;
                    }
                    else
                    {
                        current = current.Right;
                    }
                }
            }
        }

    }
}
