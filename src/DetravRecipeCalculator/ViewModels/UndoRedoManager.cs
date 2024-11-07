using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public interface IUndoRedoObject
    {
        public bool Saved { get; set; }

        public void RestoreState(object state);

        public object SaveState();
    }

    public partial class UndoRedoManager : ViewModelBase
    {
        private readonly List<(string, object)> undoRedoStates = new List<(string, object)>();

        [ObservableProperty]
        private bool canRedo;

        [ObservableProperty]
        private bool canUndo;

        private IUndoRedoObject undoRedoObject;
        private int undoRedoStatesIndex;

        public UndoRedoManager(IUndoRedoObject undoRedoObject)
        {
            this.undoRedoObject = undoRedoObject;
            Reset();
        }

        public void PushState(string name)
        {
            undoRedoStatesIndex++;

            while (undoRedoStatesIndex < undoRedoStates.Count)
            {
                undoRedoStates.RemoveAt(undoRedoStatesIndex);
            }

            undoRedoStates.Add((name, undoRedoObject.SaveState()));
            undoRedoObject.Saved = false;
            UpdateUndoRedoFlags();
        }

        public void Redo()
        {
            if (undoRedoStatesIndex < undoRedoStates.Count - 1)
            {
                undoRedoStatesIndex++;
                RestoreCurrentState();
            }
            UpdateUndoRedoFlags();
        }

        public void Reset()
        {
            undoRedoStatesIndex = 0;
            undoRedoStates.Clear();
            undoRedoStates.Add(("Initial", undoRedoObject.SaveState()));
        }

        public void RestoreCurrentState()
        {
            undoRedoObject.RestoreState(undoRedoStates[undoRedoStatesIndex].Item2);
            UpdateUndoRedoFlags();
        }

        public void Undo()
        {
            if (undoRedoStatesIndex > 0)
            {
                undoRedoStatesIndex--;
                RestoreCurrentState();
            }
            UpdateUndoRedoFlags();
        }

        private void UpdateUndoRedoFlags()
        {
            CanUndo = undoRedoStatesIndex > 0;
            CanRedo = undoRedoStatesIndex < undoRedoStates.Count - 1;
        }
    }
}