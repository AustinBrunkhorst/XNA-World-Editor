using System;

namespace WorldEditor.Classes.History
{
    public class HistoryItem
    {
        Action<HistoryAction, object> _undoAction, _redoAction;
        object argUndo, argRedo;

        public Action<HistoryAction, object> UndoAction
        {
            set { _undoAction = value; }
        }

        public Action<HistoryAction, object> RedoAction
        {
            set { _redoAction = value; }
        }

        public object UndoArg
        {
            get { return argUndo; }
        }

        public object RedoArg
        {
            get { return argRedo; }
        }

        public HistoryItem(object undoArg, object redoArg = null)
        {
            argUndo = undoArg;
            argRedo = redoArg;
        }

        public HistoryItem(object undoArg, object redoArg, Action<HistoryAction, object> action)
        {
            argUndo = undoArg;
            argRedo = redoArg;
            UndoAction = RedoAction = action;
        }

        public HistoryItem(object undoArg, object redoArg, Action<HistoryAction, object> undoAction, Action<HistoryAction, object> redoAction)
        {
            argUndo = undoArg;
            argRedo = redoArg;
            UndoAction = undoAction;
            RedoAction = redoAction;
        }

        public void Undo()
        {
            _undoAction(HistoryAction.Undo, argUndo);
        }

        public void Redo()
        {
            _redoAction(HistoryAction.Redo, argRedo == null ? argUndo : argRedo);
        }
    }
}
