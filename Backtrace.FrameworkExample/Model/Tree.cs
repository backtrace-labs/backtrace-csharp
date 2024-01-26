﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Framework45Example.Model
{
    public class Tree : IEnumerable<String>
    {
        private Node _root { get; set; }

        public Tree()
        {
            _root = new Node(' ', false);
        }

        /// <summary>
        /// Add word to tree
        /// </summary>
        public void Add(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                throw new ArgumentException("Word is empty or null.");
            }

            var current = _root;
            for (int i = 0; i < word.Length; ++i)
            {
                if (!current.Children.ContainsKey(word[i]))
                {
                    var newTreeNode = new Node(word[i])
                    {
                        Parent = current
                    };
                    current.Children.Add(word[i], newTreeNode);
                }

                current = current.Children[word[i]];
            }

            if (current.IsTerminal)
            {
                throw new InvalidOperationException("Word already exists in Tree.");
            }
            current.IsTerminal = true;
        }

        /// <summary>
        /// Removes a word from the tree.
        /// </summary>
        public void Remove(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                throw new ArgumentException("Word is empty or null.");
            }

            var current = _root;

            for (int i = 0; i < word.Length; ++i)
            {
                if (!current.Children.ContainsKey(word[i]))
                {
                    var exception = new KeyNotFoundException("Word doesn't belong to tree.");
                    throw exception;
                }
                current = current.Children[word[i]];
            }

            if (!current.IsTerminal)
            {
                var exception = new KeyNotFoundException("Word doesn't belong to tree.");
                throw exception;
            }
            current.Remove();
        }

        #region IEnumerable<String> Implementation
        /// <summary>
        /// IEnumerable\<String\>.IEnumerator implementation.
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            return _root.GetTerminalChildren().Select(node => node.Word).GetEnumerator();
        }

        /// <summary>
        /// IEnumerable\<String\>.IEnumerator implementation.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion IEnumerable<String> Implementation

    }
}
