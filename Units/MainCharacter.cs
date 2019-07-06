using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : AnimatedUnit {
    public float movespeed = 10.0f;
    [Space(15)]
    [SerializeField] private List<BaseWeapon> weapons;
    [SerializeField] private int selectedWeaponNumber = 0;
    [SerializeField] private float weaponScrollDelay = 0.1f;
    [Space(15)]
    [SerializeField] private List<Ability> abilities;
    private Ability[] selectedAbilities = new Ability[4];
    [Space]
    [SerializeField] private bool drawGUI = true;
    [SerializeField] private bool doClampVelocityTrend = true;

    private bool _cutsceneInputEnabled = true;
    public bool cutsceneInputEnabled {
        get { return _cutsceneInputEnabled; }
        set {
            if(value != _cutsceneInputEnabled) {
                _cutsceneInputEnabled = value;
                if(rb != null) {
                    rb.bodyType = value ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
                }
            }
        }
    }

    public bool inputEnabled {
        get {
            if(UI.instance != null) {
                return _cutsceneInputEnabled && !UI.instance.isPaused;
            }
            return _cutsceneInputEnabled;
        }
    }

    /*public BaseWeapon selectedWeapon {
        get {
            int i = selectedWeaponNumber;
            return i >= 0 && i < weapons.Count ? weapons[i] : null;
        }
    }*/

    public Vector2 mousePosition {
        get {
            Vector2 rawMousePosition = Input.mousePosition;
            Vector2 clamped = rawMousePosition.Clamp(Vector2.zero, new Vector2(Screen.width, Screen.height));
            return Camera.main.ScreenToWorldPoint(clamped);
        }
    }

    private Rigidbody2D rb;

    private int shootHash = Animator.StringToHash("Shoot");
    private int weaponTypeHash = Animator.StringToHash("WeaponType");
    private int weaponSelectHash = Animator.StringToHash("WeaponSelect");

    private const float distantUnitsCheckInterval = 0.5f;

    public static MainCharacter current { get; private set; }
    
    private float scrollTimer;
    private string hpText;

    protected override void Awake() {
        base.Awake();
        current = this;
        //EnsureSelectedWeaponIsAvailable();
        StartCoroutine(DistantUnitsDisableRoutine());
    }

    private IEnumerator DistantUnitsDisableRoutine() {
        yield return new WaitForSeconds(0.7f);
        while(true) {
            var position = this.position;
            foreach(var unit in allUnits) {
                unit.UpdateDistanceActiveness(position);
            }
            yield return new WaitForSeconds(distantUnitsCheckInterval);
        }
    }

    private void EnsureSelectedWeaponIsAvailable() {
        ScrollToNextAvailableWeapon(true);
        ScrollToNextAvailableWeapon(false);
    }

    void OnGUI() {
        if(!drawGUI || (UI.instance?.isPlayingCutscene ?? false)) {
            return;
        }
        GUI.skin.box.normal.textColor = Color.white;
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        GUI.skin.box.fontSize = 20;
        GUIContent hpContent = new GUIContent(hpText);
        Vector2 hpSize = GUI.skin.box.CalcSize(hpContent);
        hpSize.x += 10f;
        Vector2 hpPosition = new Vector2((Screen.width - hpSize.x) / 2f, Screen.height - hpSize.y - 20f);
        GUI.Box(new Rect(hpPosition, hpSize), hpContent);

        GUI.skin.box.fontSize = 13;
        Vector2 weaponIconSize = new Vector2(100, 68);
        float interval = 30f;
        Vector2 position = new Vector2(interval, Screen.height - weaponIconSize.y - interval);
        int count = weapons.Count;
        /*for(int i = 0; i < count; i++) {
            BaseWeapon weapon = weapons[i];
            if(weapon.isAvailable) {
                string text = weapon.name + "\n" + weapon.ammo + "/" + weapon.maxAmmo;
                GUI.skin.box.normal.textColor = (i == selectedWeaponNumber) ? new Color(1f, 0.7f, 0f, 1f) : Color.gray;
                GUI.Box(new Rect(position, weaponIconSize), text);
                position.x += weaponIconSize.x + interval;
            }
        }*/

        GUI.skin.box.fontSize = 13;
        Vector2 AbilityIconSize = new Vector2(100, 68);
        /*position = new Vector2(interval, Screen.height / 2 - AbilityIconSize.y - interval);
        for (int i = 0; i < 4; i++) {
            if (selectedAbilities[i] != null) {
                string text = selectedAbilities[i].abilityName + "\n" + selectedAbilities[i].charge;
                var continuousAbility = selectedAbilities[i] as ContinuousAbility;
                if(continuousAbility != null) {
                    GUI.skin.box.normal.textColor = (continuousAbility.isActive) ? new Color(1f, 0.7f, 0f, 1f) : Color.gray;
                }
                GUI.Box(new Rect(position, AbilityIconSize), text);
                position.y += AbilityIconSize.y + interval;
            }
        }*/
    }

    protected override void Start() {
        hpText = "HP: " + health.ToString();
        rb = GetComponent<Rigidbody2D>();
        foreach (var weapon in weapons) {
            weapon.OnFire += OnWeaponFire;
        }
        UpdateWeapons();
        FillAbilitySlots();
    }

    public override void ApplyDamage(float value, Unit source) {
        base.ApplyDamage(value, source);
        StartDamageRed();
        Invoke(nameof(StopDamageRed), 0.2f);
    }

    protected override void OnHealthChanged(float delta) {
        base.OnHealthChanged(delta);
        hpText = "HP: " + health.ToString();
    }

    private void StartDamageRed() {
        foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true)) {
            sr.material.color = new Color32(255, 90, 90, 255);
        }
    }

    private void StopDamageRed() {
        foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true)) {
            sr.material.color = Color.white;
        }
    }

    protected override void UpdateVelocityTrend(float deltaTime) {
        base.UpdateVelocityTrend(deltaTime);
        if(doClampVelocityTrend) {
            velocityTrend = Vector3.ClampMagnitude(velocityTrend, movespeed);
        }
    }

    protected override void OnDied() {
        GetComponent<Blood>().SpawnBloodBurst();
        if(UI.instance != null) {
            UI.instance.ShowDeathScreen();
        }
        Invoke(nameof(ReloadScene), 3f);

        base.OnDied();
    }

    private void ReloadScene() {
        SaveSystem.instance.Load();
    }

    private void OnWeaponFire(BaseWeapon weapon) {
        animator.SetTrigger(shootHash);
    }

    void FixedUpdate() {
        if(inputEnabled) {
            Movement();
            Rotate();
        }
    }

    private void Movement() {
        Vector2 direction = Vector2.zero;
        if(Input.GetAxis("Vertical") > 0) { direction += Vector2.up; }
        if(Input.GetAxis("Vertical") < 0) { direction += Vector2.down; }
        if(Input.GetAxis("Horizontal") < 0) { direction += Vector2.left; }
        if(Input.GetAxis("Horizontal") > 0) { direction += Vector2.right; }
        rb.velocity = direction.normalized * movespeed;
    }

    private void Rotate() {
        //Transform weaponTransform = selectedWeapon.transform;
        /*if (((Vector3)mousePosition - weaponTransform.position).sqrMagnitude < 0.4f) {
            return;
        }
        float angle = Vector3.SignedAngle(weaponTransform.up, (Vector3)mousePosition - weaponTransform.position + weaponTransform.up * 1.5f, Vector3.forward);
        rb.rotation += angle;*/
        //Vector2 weaponVector = weaponTransform.position - transform.position;
        //Vector2 weaponDirection = weaponTransform.up;
        Vector2 toMouse = mousePosition - (Vector2)transform.position;
        rb.rotation = Vector2.SignedAngle(Vector2.up, toMouse);
        /*float dot = Vector2.Dot(weaponVector, weaponDirection);
        float descriminant = dot * dot - weaponVector.sqrMagnitude + toMouse.sqrMagnitude;
        if(descriminant < 0) {
            return;
        }
        float k = -dot + Mathf.Sqrt(descriminant);
        Vector2 circlePosition = weaponVector + k * weaponDirection;
        float angle = Vector2.SignedAngle(circlePosition, toMouse);
        rb.rotation += angle * Time.fixedDeltaTime * 30;*/
    }
    
    protected override void OnUpdate() {
        base.OnUpdate();
        if(inputEnabled) {
            /*WeaponScrolling();
            WeaponDirectSelecion();
            */
            bool isTriggerPressed = Input.GetAxis("Fire") > 0;
            bool triggerConsumed = false;
            UpdateAbilitiesInput(isTriggerPressed, ref triggerConsumed);
            /*if(!triggerConsumed) {
                selectedWeapon.isTriggerPressed = isTriggerPressed;
            }*/
        }
        /*else {
            selectedWeapon.isTriggerPressed = false;
            for (int i = 0; i < 4; i++) {
                if (selectedAbilities[i] != null) {
                    selectedAbilities[i].isButtonPressed = false;
                }
            }
        }*/
    }
    
    private void WeaponScrolling() {
        if(scrollTimer > 0) {
            scrollTimer -= Time.deltaTime;
        }
        else {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if(scroll != 0f) {
                bool scrollUp = scroll < 0;
                ScrollToNextAvailableWeapon(scrollUp);
                scrollTimer = weaponScrollDelay;
            }
        }
    }

    private void WeaponDirectSelecion() {
        for(int i = 0; i < 4; i++) {
            if(Input.GetAxis("Weapon" + i) > 0) {
                SelectAvailableWeapon(i);
                return;
            }
        }
    }

    private void UpdateAbilitiesInput(bool isTriggerPressed, ref bool triggerConsumed) {
        for(int i = 0; i < 4; i++) {
            Ability selectedAbility = selectedAbilities[i];
            if(selectedAbility == null)
                continue;
            selectedAbility.isButtonPressed = Input.GetAxis("Ability" + i) > 0;

            var targetPointAbility = selectedAbility as TargetPointAbility;
            if(targetPointAbility == null || !targetPointAbility.isAiming)
                continue;
            targetPointAbility.isFireTriggerPressed = isTriggerPressed;
            triggerConsumed = true;
        }
    }

    private void ScrollToNextAvailableWeapon(bool scrollUp) {
        int scrollDirection = scrollUp ? 1 : -1;
        int i = selectedWeaponNumber;
        do {
            i += scrollDirection;
            if(i >= weapons.Count) i = 0;
            else if(i < 0) i = weapons.Count - 1;

            if(weapons[i].isAvailable) {
                SelectWeapon(i);
                break;
            }
        } while(i != selectedWeaponNumber);
    }

    public void SelectAvailableWeapon(int number) {
        List<BaseWeapon> availableWeapons = weapons.FindAll(w => w != null && w.isAvailable);
        BaseWeapon weapon = availableWeapons.ElementAtOrDefault(number);
        if(weapon != default(BaseWeapon)) {
            SelectWeapon(weapon);
        }
    }

    public int GetSelectedWeaponNumber() => selectedWeaponNumber;
    
    public void SelectWeapon(BaseWeapon weapon) {
        SelectWeapon(weapons.IndexOf(weapon));
    }

    public void SelectWeapon(int i) {
        if(i < 0 || i >= weapons.Count)
            return;

        if(weapons[i] == null || !weapons[i].isAvailable)
            return;

        selectedWeaponNumber = i;
        UpdateWeapons();

        animator.ResetTrigger(shootHash);
        animator.SetInteger(weaponTypeHash, weapons[selectedWeaponNumber].animationType);
        animator.SetTrigger(weaponSelectHash);
    }

    public void SelectWeapon(string name) {
        int? i = GetWeaponIndex(name);
        if (i != null) {
            SelectWeapon((int)i);
        }
    }

    private void UpdateWeapons() {
        for(int i = 0; i < weapons.Count; i++) {
            var weapon = weapons[i];
            if(i == selectedWeaponNumber) {
                weapon.gameObject.SetActive(true);
            }
            else {
                weapon.gameObject.SetActive(false);
                weapon.isTriggerPressed = false;
            }
        }
    }

    public int? GetWeaponIndex(string name) {
        for (int i = 0; i < weapons.Count; i++) {
            if (weapons[i].name == name) {
                return i;
            }
        }
        return null;
    }

    public void SetWeaponAvailable(int i, bool isAvailable) {
        weapons[i].isAvailable = isAvailable;
    }

    public void SetWeaponAvailable(string name, bool isAvailable) {
        int? i = GetWeaponIndex(name);
        if (i != null) {
            SetWeaponAvailable((int)i, isAvailable);
        }
    }

    public void DropWeapon(int i) {
        weapons[i].GetComponent<LootDropper>()?.DropLoot(transform.position);
        weapons[i].isAvailable = false;
    }

    public void DropWeapon(string name) {
        int? i = GetWeaponIndex(name);
        if (i != null) {
            DropWeapon(i.Value);
        }
    }

    public void FillAbilitySlots() {
        for (int i = 0; i < 4; i++) {
            foreach (Ability ability in abilities) {
                if (ability.slot == i) {
                    selectedAbilities[i] = ability;
                    continue;
                }
            }
            
        }
    }
}
