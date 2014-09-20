using System;
using System.Collections.Generic;

namespace WorldEditor.Classes.History
{
    class HistoryStack
    {
        int maxSize;
        List<HistoryItem> items;

        public int Count
        {
            get { return items.Count; }
        }

        public HistoryStack(int max)
        {
            maxSize = max;
            items = new List<HistoryItem>();
        }

        public void Push(HistoryItem item)
        {
            items.Add(item);
            while (items.Count > maxSize)
            {
                items.RemoveAt(0);
            }
        }

        public HistoryItem Last()
        {
            return items[items.Count - 1];
        }

        public HistoryItem Pop()
        {
            if (items.Count == 0)
                throw new Exception("Empty Stack");

            HistoryItem temp = items[items.Count - 1];

            items.RemoveAt(items.Count - 1);

            return temp;
        }

        public void Remove(int position)
        {
            items.RemoveAt(position);
        }

        public void Clear()
        {
            items.Clear();
        }
    }
}
