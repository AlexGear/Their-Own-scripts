using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour {
    [SerializeField] public string abilityName;
    [Tooltip("Set -1 if not bound")]
    [SerializeField] public int slot = -1;

    [SerializeField] [Range(0, 1)] private float _charge = 1;
    public float charge {
        get { return _charge; }
        set {
            if (value <= 0) {
                value = 0;
                if(_charge > 0) OnChargeExhausted();
            }
            else if (value >= 1) {
                value = 1;
                if(_charge < 1) OnChargeFilledUp();
            }
            _charge = value;
        }
    }

    [Tooltip("Time in seconds")]
    [SerializeField] protected float rechargeDuration = 50;

    protected Unit unit;

    private bool _isButtonPressed;
    public bool isButtonPressed {
        get { return _isButtonPressed; }
        set {
            bool current = _isButtonPressed;
            _isButtonPressed = value;

            if(!current && value)
                OnButtonPressed();
            else if(current && !value)
                OnButtonReleased();
        }
    }

    protected virtual void Start() {
        unit = GetComponent<Unit>();
    }

    protected virtual void Update() {
        UpdateCharge();
    }

    protected virtual void UpdateCharge() {
        charge += 1 / rechargeDuration * Time.deltaTime;
    }

    protected virtual void OnButtonPressed() { }
    protected virtual void OnButtonReleased() { }

    protected virtual void OnChargeExhausted() { }
    protected virtual void OnChargeFilledUp() { }
}
