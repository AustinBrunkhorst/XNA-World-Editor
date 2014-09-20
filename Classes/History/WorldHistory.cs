
namespace WorldEditor.Classes.History
{
    public enum HistoryAction
    {
        Undo, Redo
    };

    public class WorldHistory
    {
        const int MAX_HISTORY_ITEMS = 50;

        HistoryStack undoStack, redoStack;

        public bool CanUndo
        {
            get { return undoStack.Count > 0; }
        }

        public bool CanRedo
        {
            get { return redoStack.Count > 0; }
        }

        public WorldHistory()
        {
            undoStack = new HistoryStack(MAX_HISTORY_ITEMS);
            redoStack = new HistoryStack(MAX_HISTORY_ITEMS);
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            HistoryItem item = undoStack.Pop();

            redoStack.Push(item);

            item.Undo();
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            HistoryItem item = redoStack.Pop();

            undoStack.Push(item);
           
            item.Redo();
        }

        public void Add(HistoryItem item)
        {
            undoStack.Push(item);

            redoStack.Clear();
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public HistoryItem NextUndo()
        {
            return undoStack.Last();
        }

        public HistoryItem NextRedo()
        {
            return redoStack.Last();
        }
    }
}
