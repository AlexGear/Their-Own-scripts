using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPointAbility : Ability {
    [SerializeField, Range(0, 1)] float useChargeCost = 1;

    private bool _isAiming;
    public bool isAiming {
        get { return _isAiming; }
        protected set {
            bool current = _isAiming;
            _isAiming = value;
            if(current && !value)
                OnAimingStarted();
            else if(!current && value)
                OnAimingStopped();
        }
    }

    private bool _isFireTriggerPressed;
    public bool isFireTriggerPressed {
        get { return _isFireTriggerPressed; }
        set {
            bool current = _isFireTriggerPressed;
            _isFireTriggerPressed = value;
            if(current && !value)
                OnFireTriggerPressed();
            else if(!current && value)
                OnFireTriggerReleased();
        }
    }

    protected override void OnButtonPressed() => isAiming = !isAiming;

    protected virtual void OnAimingStarted() { }
    protected virtual void OnAimingStopped() { }

    protected virtual void OnFireTriggerReleased() { }

    protected virtual void OnFireTriggerPressed() {
        if(isAiming) {
            if(charge < useChargeCost) {
                OnNotEnoughCharge();
                return;
            }
            Vector2 useAt = MainCharacter.current.mousePosition;
            if(!CanUse(useAt)) {
                OnCannotUseOnOriginalPoint(useAt);
                Vector2? anotherPoint = TryFindAnotherUsePoint(useAt);
                if(anotherPoint == null) {
                    OnCannotUseOnAnotherPoint(useAt);
                    return;
                }
                useAt = anotherPoint.Value;
            }
            Use(useAt);
            isAiming = false;
        }
    }

    protected virtual void OnNotEnoughCharge() { }
    protected virtual void OnCannotUseOnOriginalPoint(Vector2 point) { }
    protected virtual void OnCannotUseOnAnotherPoint(Vector2 originalPoint) { }

    protected virtual bool CanUse(Vector2 at) => true;

    protected virtual Vector2? TryFindAnotherUsePoint(Vector2 originalPoint) => null;

    protected virtual void Use(Vector2 at) {
        charge -= useChargeCost;
    }
}
