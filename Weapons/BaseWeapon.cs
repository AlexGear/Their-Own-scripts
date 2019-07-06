using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour {
    [SerializeField] public AmmoType ammoType;
    [SerializeField] protected float shootCooldown = 0.3f;
    [SerializeField] public bool isAvailable = true;
    [SerializeField] GameObject laserDesignatorPrefab = null;

    [SerializeField]
    [Range(0, 20)]
    [LabelOverride("Scatter")]
    private float _scatter = 0f;

    public int animationType;
    [SerializeField] protected Transform bulletStart;
    [SerializeField] protected LayerMask fireHindrances;
    [SerializeField] protected AudioSource shotAudioSource;
    [SerializeField] protected float shotAttentionAttractionRadius = 25f;
    protected CameraShake camShake;
    private Transform laserDesignator;

    protected static Lazy<int> defaultAttentionMask = new Lazy<int>(() => 
        (1 << LayerMask.NameToLayer("Allies")) | 
        (1 << LayerMask.NameToLayer("Enemies"))
    );

    private Unit _owner;
    public Unit owner => _owner != null ? _owner : (_owner = GetComponentInParent<Unit>());

    public Vector2 bulletStartPoint => bulletStart.position;

    protected int attentionMask;

    protected float cooldownTimer = 0f;    

    public float scatter {
        get { return _scatter; }
        set { _scatter = Mathf.Clamp(value, 0, 360); }
    }

    public virtual int ammo {
        get { return owner.ammoBag.GetAmmo(ammoType); }
        set { owner.ammoBag.SetAmmo(ammoType, value); }
    }

    public virtual int maxAmmo {
        get { return owner.ammoBag.GetMaxAmmo(ammoType); }
        set { owner.ammoBag.SetMaxAmmo(ammoType, value); }
    }

    public bool isTriggerPressed;

    public event Action<BaseWeapon> OnFire;
    
    public virtual bool isReadyToFire => (cooldownTimer <= 0 && ammo > 0&& isFireLineUnlocked);
    public LazyUpdate<bool> isFireLineUnlocked;

    protected virtual bool UpdateIsFireLineUnlocked() {
        if(owner == null)
            return true;
        return null == Physics2D.Linecast(owner.transform.position, bulletStart.position, fireHindrances).collider;
    }

    protected virtual void Start() {
        int isFireLineUnlockedUpdatePeriod = owner == MainCharacter.current ? 1 : 4;
        isFireLineUnlocked = new LazyUpdate<bool>(isFireLineUnlockedUpdatePeriod, UpdateIsFireLineUnlocked);

        camShake = GetComponent<CameraShake>();
        int attentionMaskExculsion = owner != null ? (1 << owner.gameObject.layer) : 0;
        attentionMask = defaultAttentionMask.Value & ~attentionMaskExculsion;
        
        if(laserDesignatorPrefab != null/* && owner == MainCharacter.current*/) {
            laserDesignator = Instantiate(laserDesignatorPrefab, this.transform).transform;
            laserDesignator.localPosition = Vector3.zero;
            laserDesignator.localRotation = Quaternion.identity;
        }
    }

    public bool FireAttempt() {
        if(isReadyToFire) {
            Fire();
            return true;
        }
        return false;
    }

    protected virtual void Fire() {
        ammo--;
        cooldownTimer = shootCooldown;
        OnFire?.Invoke(this);
        if(camShake != null) {
            camShake.ShakeCameraOmni();
        }

        if(shotAttentionAttractionRadius > 0) {
            AI.Attention.Attract(transform.position, shotAttentionAttractionRadius, attentionMask);
        }
    }

    protected virtual void Update() {
        if(cooldownTimer > 0) {
            cooldownTimer -= Time.deltaTime;
        }
        if(isTriggerPressed) {
            FireAttempt();
        }
        PositionLaserDesignator();
    }

    private void PositionLaserDesignator() {
        if(laserDesignator == null)
            return;

        laserDesignator.gameObject.SetActive(isFireLineUnlocked);
    }
}
