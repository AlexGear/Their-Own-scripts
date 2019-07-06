using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public abstract class ChargingShotController : ShootingController
    {
        [SerializeField] float chargingDuration = 1f;
        [SerializeField] float cooldown = 1f;

        private Timer chargingTimer;
        private Timer cooldownTimer;

        protected bool isCoolingDown { get; private set; }
        protected bool isCharging { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            chargingTimer = new Timer(chargingDuration);
            cooldownTimer = new Timer(cooldown);
        }

        public override void OnWeaponReconfigured()
        {
            if (isCharging)
            {
                InterruptCharging();
            }
        }

        public override void OnUpdateBeingSelected()
        {
            if (isCoolingDown)
            {
                if (!cooldownTimer.Tick())
                    return;

                isCoolingDown = false;
            }

            if (!Input.GetButton("Fire") || weapon.magazine.isEmpty)
            {
                if (isCharging)
                {
                    InterruptCharging();
                }
                return;
            }

            if (isCharging)
            {
                NotifyChargingProgress();
                if (chargingTimer.Tick())
                {
                    Fire();
                }
            }
            else
            {
                StartCharging();
            }
        }

        protected virtual void OnChargingStarted()
        {
        }

        protected virtual void OnChargingInterrupted()
        {
        }

        protected virtual void OnChargingProgress(float t)
        {
        }

        protected virtual void OnFire()
        {
        }

        private void StartCharging()
        {
            isCharging = true;
            chargingTimer.Reset();
            OnChargingStarted();
        }

        private void InterruptCharging()
        {
            isCharging = false;
            OnChargingInterrupted();
        }

        private void NotifyChargingProgress()
        {
            float t = 1f - chargingTimer.remaining / chargingTimer.interval;
            OnChargingProgress(t);
        }
        
        private void Fire()
        {
            isCharging = false;
            isCoolingDown = true;
            cooldownTimer.Reset();
            OnFire();
        }
    }
}