using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    [System.Serializable]
    public class Selector<TPart> : BaseSelector where TPart : Part
    {
        [SerializeField] List<TPart> parts = new List<TPart>();
        [SerializeField, HideInInspector] int i = 0;

        public int count => parts.Count;

        public override int selectedIndex
        {
            get => i;
            set
            {
                if (i == value)
                {
                    return;
                }
                if (value < 0 || value >= count)
                {
                    string message = $"selectedIndex = {value} is out of range [0; {count})";
                    throw new System.IndexOutOfRangeException(message);
                }
                int oldI = i;
                OnDeselected(parts[i]);
                i = value;
                OnSelected(parts[i]);
                if (Application.isPlaying)
                {
                    SelectedChanged?.Invoke(this, i);
                }
            }
        }

        public TPart selected => parts[selectedIndex];

        public event System.Action<Selector<TPart>, TPart> PartSelected;
        public event System.Action<Selector<TPart>, TPart> PartDeselected;
        public event System.Action<Selector<TPart>, int> SelectedChanged;

        public override IReadOnlyList<Part> GetParts() => parts;

        public void OnStart()
        {
            if (count == 0) throw new System.InvalidOperationException("Parts count == 0");
            OnSelected(selected);
        }

        public void ScrollUp(bool wrap = true)
        {
            if (count == 0) throw new System.InvalidOperationException("Parts count == 0. Failed to scroll up");
            if (count == 1) return;

            int index = selectedIndex - 1;
            if (index < 0)
            {
                index = wrap ? count - 1 : 0;
            }
            selectedIndex = index;
        }
        public void ScrollDown(bool wrap = true)
        {
            if (count == 0) throw new System.InvalidOperationException("Parts count == 0. Failed to scroll down");
            if (count == 1) return;

            int index = selectedIndex + 1;
            if (index >= count)
            {
                index = wrap ? 0 : count - 1;
            }
            selectedIndex = index;
        }

        public void Scroll(bool up, bool wrap = true)
        {
            if (up) ScrollUp(wrap);
            else ScrollDown(wrap);
        }

        private void OnSelected(TPart part)
        {
            if (!Application.isPlaying || part == null) return;
            part.Select();
            PartSelected?.Invoke(this, part);
        }

        private void OnDeselected(TPart part)
        {
            if (!Application.isPlaying || part == null) return;
            part.Deselect();
            PartDeselected?.Invoke(this, part);
        }
    }
}