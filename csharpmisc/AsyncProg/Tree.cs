using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.AsyncProg
{
    public class Tree<T>
    {
        public T Data;
        public Tree<T> Left, Right;

        // Fork/Join pattern: Walk the tree with Tasks 
        static void Walk(Tree<T> root, Action<T> action)
        {
            if (root == null)
                return;
            var t1 = Task.Factory.StartNew(() => action(root.Data),
            TaskCreationOptions.AttachedToParent);
            var t2 = Task.Factory.StartNew(() => Walk(root.Left, action),
            TaskCreationOptions.AttachedToParent);
            var t3 = Task.Factory.StartNew(() => Walk(root.Right, action),
            TaskCreationOptions.AttachedToParent);
            Task.WaitAll(t1, t2, t3);
        }


    }
}
