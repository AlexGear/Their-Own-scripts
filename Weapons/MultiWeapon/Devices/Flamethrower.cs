using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

namespace Armament
{
    public class Flamethrower : MonoBehaviour
    {
        [SerializeField] ParticleSystem particles;
        [SerializeField] Collider2D hitArea;
        [SerializeField] float periodicDamage;
        [SerializeField] float damageInterval;
        [SerializeField] LayerMask damageMask;

        private bool isDamagingOn = false;
        private float actualDamage;
        private Unit damageSource;
        private Timer damageTimer;

        public void StartFlameDamaging(Unit damageSource)
        {
            StartFlameDamaging(this.periodicDamage, this.damageInterval, damageSource);
        }

        public void StartFlameDamaging(float periodicDamage, float damageInterval, Unit damageSource)
        {
            isDamagingOn = true;
            StartFlameParticlesOnly();

            actualDamage = periodicDamage;
            this.damageSource = damageSource;
            damageTimer.interval = damageInterval;
            damageTimer.Reset();
        }

        public void StartFlameParticlesOnly()
        {
            if (particles.isStopped)
            {
                particles.Play();
            }
        }

        public void Stop()
        {
            isDamagingOn = false;
            if (particles.isPlaying)
            {
                particles.Stop();
            }
        }

        private void Awake()
        {
            if (hitArea == null)
            {
                Debug.LogError("hitArea is missing", this);
                enabled = false;
                return;
            }
            damageTimer = new Timer(0);
        }

        private void Update()
        {
            if (isDamagingOn && damageTimer.Tick())
            {
                ApplyAreaDamage(actualDamage, damageSource);
            }
        }

        public void ApplyAreaDamage(Unit damageSource) => ApplyAreaDamage(periodicDamage, damageSource);

        public void ApplyAreaDamage(float damage, Unit damageSource)
        {
            foreach (var unit in Unit.GetWithinArea(hitArea, damageMask))
            {
                unit.ApplyDamage(damage, damageSource);
            }
        }
    }
}