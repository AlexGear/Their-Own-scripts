using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum AmmoType {
    Plasma = 1,
    Laser = 2,
    Deagle = 3,
    AK = 4,
    Shotgun = 5,
    Minigun = 6,
    Rocket = 7,
    Zapurasser = 8
}

public class AmmoBag : MonoBehaviour {
    [Serializable]
    public class AmmoSlot {
        public AmmoType ammoType = AmmoType.Minigun;

        public int _ammo = 0;
        public int ammo {
            get { return _ammo; }
            set { _ammo = Mathf.Clamp(value, 0, _maxAmmo); }
        }

        public int _maxAmmo = 0;
        public int maxAmmo {
            get { return _maxAmmo; }
            set {
                _maxAmmo = value;
                if (_ammo > _maxAmmo)
                    _ammo = _maxAmmo;
            }
        }
    }

    [SerializeField] private List<AmmoSlot> ammoBag;

    public int GetAmmoIndex(AmmoType ammoType) {
        for (int i = 0; i < ammoBag.Count; i++) {
            if (ammoBag[i].ammoType == ammoType) {
                return i;
            }
        }
        return 0;
    }

    private int GetAmmo(int i) => ammoBag[i].ammo;
    public int GetAmmo(AmmoType ammoType) => GetAmmo(GetAmmoIndex(ammoType));

    private void SetAmmo(int i, int value) => ammoBag[i].ammo = value;
    public void SetAmmo(AmmoType ammoType, int value) => ammoBag[GetAmmoIndex(ammoType)].ammo = value;

    private int GetMaxAmmo(int i) => ammoBag[i].maxAmmo;
    public int GetMaxAmmo(AmmoType ammoType) => GetMaxAmmo(GetAmmoIndex(ammoType));

    private void SetMaxAmmo(int i, int value) => ammoBag[i].maxAmmo = value;
    public void SetMaxAmmo(AmmoType ammoType, int value) => ammoBag[GetAmmoIndex(ammoType)].maxAmmo = value;
}