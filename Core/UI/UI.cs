using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour {
    [SerializeField] string walkthroughStartScene = "level1";
    [SerializeField] GameObject mainMenuRoot;
    [SerializeField] Animator deathScreenAnimator;
    [Header("Loading")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] Slider loadingProgressbar;
    [Header("Cutscenes")]
    [SerializeField] GameObject skipCutsceneText;
    [SerializeField] Animator cutsceneScreenAnimator;
    [SerializeField] Text subtitlesText;
    [Header("Pause")]
    [SerializeField] Animator pauseScreenAnimator;
    [Header("Damage Borders")]
    [SerializeField] Animator damageBordersAnimator;
    [SerializeField] Image damageBordersImage;
    [SerializeField, Range(0, 1)] float damageBordersImageMinAlpha = 0.4f;

    private MainCharacter mainCharacter;
    private static readonly int damageTakenHash = Animator.StringToHash("DamageTaken");

    private bool isSettingsScreenVisible = false;
    public bool isPaused { get; private set; } = false;
    public static UI instance { get; private set; }
    public bool isPlayingCutscene { get; private set; } = false;

    void Awake() {
        if(instance != null) {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
        if(mainCharacter != null)
            mainCharacter.DamageApplied -= OnMainCharacterDamageApplied;
        mainCharacter = MainCharacter.current;
        if(mainCharacter != null)
            mainCharacter.DamageApplied += OnMainCharacterDamageApplied;
    }

    private void OnMainCharacterDamageApplied(Unit mc, float value) {
        Color color = damageBordersImage.color;
        color.a = (1f - mc.health / mc.maxHealth) * (1f - damageBordersImageMinAlpha) + damageBordersImageMinAlpha;
        damageBordersImage.color = color;
        damageBordersAnimator.SetTrigger(damageTakenHash);
    }

    void Update() {
        if(Input.GetButtonDown("Pause")) {
            if(!mainMenuRoot.activeSelf) {
                SetPaused(!isPaused);
            }
        }
    }

    private void SetPaused(bool isPaused) {
        this.isPaused = isPaused;
        isSettingsScreenVisible = false;
        Time.timeScale = isPaused ? 0 : 1;
        if(pauseScreenAnimator) {
            pauseScreenAnimator.SetBool("Visible", isPaused);
            pauseScreenAnimator.SetBool("SettingsVisible", false);
        }
    }

    public void ToggleSettingsScreen() {
        isSettingsScreenVisible = !isSettingsScreenVisible;
        if(pauseScreenAnimator)
            pauseScreenAnimator.SetBool("SettingsVisible", isSettingsScreenVisible);
    }

    public void Quit() {
        Application.Quit();
    }

    public void BeginCutscene(bool isSkippable) {
        if(cutsceneScreenAnimator)
            cutsceneScreenAnimator.SetBool("Visible", true);
        skipCutsceneText.SetActive(isSkippable);

        isPlayingCutscene = true;
    }
    
    public void ShowCutsceneSkipBlackout() {
        if(cutsceneScreenAnimator)
            cutsceneScreenAnimator.SetTrigger("SkipBlackout");
    }

    public void EndCutscene() {
        isPlayingCutscene = false;
        if(cutsceneScreenAnimator)
            cutsceneScreenAnimator.SetBool("Visible", false);
    }

    public void ShowDialoguePanel() {
        if(cutsceneScreenAnimator)
            cutsceneScreenAnimator.SetBool("DialogueVisible", true);
        subtitlesText.text = "";
    }

    public void SetDialogueSubtitles(string text) {
        subtitlesText.text = text;
    }

    public void HideDialoguePanel() {
        if(cutsceneScreenAnimator)
            cutsceneScreenAnimator.SetBool("DialogueVisible", false);
    }

    public void StartWalkthrough(int slot) {
        StartCoroutine(StartWalkthroughCoroutine(slot));
    }

    public void ContinueWalkthrough(int slot) {
        SaveSystem.instance.currentSlot = slot;
        SaveSystem.instance.Load();
    }

    private IEnumerator StartWalkthroughCoroutine(int slot) {
        SaveSystem.instance.currentSlot = slot;
        yield return LoadScene(walkthroughStartScene);
        SaveSystem.instance.Save();
    }

    public void ShowDeathScreen() {
        if(deathScreenAnimator)
            deathScreenAnimator.SetBool("Visible", true);
    }

    public void HideDeathScreen() {
        if(deathScreenAnimator)
            deathScreenAnimator.SetBool("Visible", false);
    }

    public void LoadSceneFromUI(string sceneName) {
        LoadScene(sceneName);
    }

    public IEnumerator LoadScene(string sceneName) {
        mainMenuRoot.SetActive(sceneName == "MainMenu");

        IEnumerator coroutine = LoadingCoroutine(sceneName);
        StartCoroutine(coroutine);
        return coroutine;
    }

    private IEnumerator LoadingCoroutine(string sceneName) {
        LoadingStarted();
        yield return null;
        yield return null;  // delay to make the loading screen appear instantly

        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneName);
        while(!loadingOperation.isDone) {
            UpdateProgress(loadingOperation.progress);
            yield return null;
        }
        LoadingFinished();
    }

    private void LoadingStarted() {
        HideDeathScreen();
        SetPaused(false);
        loadingPanel.SetActive(true);
        loadingProgressbar.value = 0;
    }

    private void UpdateProgress(float progress) {
        loadingProgressbar.value = progress;
    }

    private void LoadingFinished() {
        loadingPanel.SetActive(false);
    }
}
