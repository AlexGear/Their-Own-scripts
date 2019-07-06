using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousAbility : Ability {
    [SerializeField] protected bool toggle = true;
    [Tooltip("Time in seconds")]
    [SerializeField] protected float maxDuration = 10;

    private bool _isActive;
    public bool isActive {
        get { return _isActive; }
        protected set {
            bool current = _isActive;
            _isActive = value;
            if(!current && value) {
                Activated?.Invoke(this);
                OnActivated();
            }
            else if(current && !value) {
                Deactivated?.Invoke(this);
                OnDeactivated();
            }
        }
    }

    public event Action<Ability> Activated;
    public event Action<Ability> Deactivated;

    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }

    protected override void OnButtonPressed() {
        if(toggle)
            isActive = !isActive;
        else
            isActive = true;
    }

    protected override void OnButtonReleased() {
        if(!toggle)
            isActive = false;
    }

    protected override void OnChargeExhausted() => isActive = false;
    
    protected override void UpdateCharge() {
        if(isActive) {
            charge -= Time.deltaTime / maxDuration;
        }
        else {
            charge += Time.deltaTime / rechargeDuration;
        }
    }
}
