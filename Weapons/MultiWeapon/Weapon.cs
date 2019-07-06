using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class Weapon : MonoBehaviour
    {
        [System.Serializable] class ControllerSelector : Selector<WeaponController> { }
        [System.Serializable] class NozzleSelector : Selector<Nozzle> { }
        [System.Serializable] class MagazineSelector : Selector<Magazine> { }

        [SerializeField] ControllerSelector controllerSelector;
        [SerializeField] NozzleSelector nozzleSelector;
        [SerializeField] MagazineSelector magazineSelector;

        private BaseSelector[] allSelectors;
        private Part[] allParts;
        private Timer wheelScrollTimer;

        public Unit owner { get; private set; }

        public WeaponController controller => controllerSelector.selected;
        public Nozzle nozzle => nozzleSelector.selected;
        public Magazine magazine => magazineSelector.selected;

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 300, 30), magazine.ToString());
            GUI.Box(new Rect(10, 40, 300, 30), nozzle.ToString());
        }

        private void Start()
        {
            allSelectors = new BaseSelector[] { controllerSelector, nozzleSelector, magazineSelector };
            allParts = CollectAllParts(allSelectors).ToArray();
            foreach (var part in allParts)
                part.BindTo(this);

            owner = GetComponentInParent<Unit>();

            controllerSelector.OnStart();
            nozzleSelector.OnStart();
            magazineSelector.OnStart();

            controllerSelector.SelectedChanged += OnSelectedIndexChanged;
            nozzleSelector.SelectedChanged += OnSelectedIndexChanged;
            magazineSelector.SelectedChanged += OnSelectedIndexChanged;

            wheelScrollTimer = new Timer(0.1f);
        }

        private static IEnumerable<Part> CollectAllParts(IEnumerable<BaseSelector> selectors)
        {
            foreach (var selector in selectors)
                foreach (var part in selector.GetParts())
                    yield return part;
        }

        private void OnSelectedIndexChanged<T>(Selector<T> sel, int oldIdx) where T : Part
        {
            foreach (var part in allParts)
                part.OnWeaponReconfigured();
        }

        private void Update()
        {
            controller.OnUpdateBeingSelected();
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                controllerSelector.ScrollDown();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                nozzleSelector.ScrollDown();
            }

            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0 && wheelScrollTimer.Tick())
            {
                magazineSelector.Scroll(scroll > 0);
            }
        }
    }
}