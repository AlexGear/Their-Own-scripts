using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public abstract class Part : MonoBehaviour
    {
        [SerializeField] Part[] incompatibleParts = new Part[0];

        private HashSet<Part> incompatiblePartsHS;

        public Weapon weapon { get; private set; }

        public bool isSelected { get; private set; }

        protected virtual void Awake()
        {
            incompatiblePartsHS = new HashSet<Part>(incompatibleParts);
        }

        public virtual bool IsCompatibleWith(Part other)
        {
            return !incompatiblePartsHS.Contains(other);
        }

        public static bool AreCompatible(Part a, Part b)
        {
            return a.IsCompatibleWith(b) && b.IsCompatibleWith(a);
        }

        public void BindTo(Weapon weapon)
        {
            this.weapon = weapon;
        }

        public void Select()
        {
            isSelected = true;
            OnSelected();
        }

        public void Deselect()
        {
            isSelected = false;
            OnDeselected();
        }
        public virtual void OnWeaponReconfigured()
        {
        }

        public override string ToString() => name;


        protected virtual void OnSelected()
        {
        }

        protected virtual void OnDeselected()
        {
        }
    }
}