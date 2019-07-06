using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

using SaverID = System.String;

public class SaveSystem : MonoBehaviour {
    [Serializable]
    private struct SaveContainer {
        public string sceneName;
        public Dictionary<SaverID, object> saverDatas;
    }
    private const string saveFilename = "savedat";
    private string savePath;
    
    public static SaveSystem instance { get; private set; }

    [NonSerialized]
    public int currentSlot = 0;

    private Dictionary<int, SaveContainer> saveSlots;
    private Dictionary<SaverID, Saver> currentSavers = new Dictionary<SaverID, Saver>();

    private Thread saveThread;

    void Awake() {
        if(instance != null) {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        savePath = Application.persistentDataPath + "/" + saveFilename;

        DontDestroyOnLoad(this.gameObject);
        FindAndRegisterSavers();
        InitSaveSlots();

        SceneManager.sceneLoaded += (s, m) => FindAndRegisterSavers();
        SceneManager.sceneUnloaded += (s) => currentSavers.Clear();
    }

    private void InitSaveSlots() {
        if(!File.Exists(savePath)) {
            Debug.Log("No save file found. Creating a new one at " + savePath);
            CreateNewSaveFile();
        }
        else {
            Debug.Log("Loading slots from file " + savePath);
            if(LoadSlotsFromSaveFile()) {
                Debug.Log("Slots loaded successfully");
            }
            else {
                // TODO: Show message to user
                CopyBrokenSaveFile();
                Debug.Log("Trying to create a new save file");
                CreateNewSaveFile();
            }
        }
    }

    private void CopyBrokenSaveFile() {
        string brokenPath = savePath + "_BROKEN_" + DateTime.Now.ToShortDateString();
        Debug.Log("Copying broken file into " + brokenPath);
        try {
            File.Copy(savePath, brokenPath);
        }
        catch(Exception ex) {
            Debug.LogErrorFormat("Cannot copy broken file\n {0}\n StackTrace: {1}", ex.Message, ex.StackTrace);
        }
    }

    private bool CreateNewSaveFile() {
        FileStream file = null;
        try {
            file = File.Create(savePath);
            saveSlots = new Dictionary<int, SaveContainer>();
            using(var bs = new BufferedStream(file)) {
                new BinaryFormatter().Serialize(bs, saveSlots);
            }
            Debug.Log("New save file created successfully");
            return true;
        }
        catch(Exception ex) {
            Debug.LogErrorFormat("Creating new save file failed\n {0}\n StackTrace: {1}", ex.Message, ex.StackTrace);
            return false;
        }
        finally {
            file?.Close();
        }
    }

    private bool LoadSlotsFromSaveFile() {
        FileStream file = null;
        try {
            file = File.OpenRead(savePath);
            using(var bs = new BufferedStream(file)) {
                saveSlots = (Dictionary<int, SaveContainer>)new BinaryFormatter().Deserialize(bs);
            }
            return true;
        }
        catch(Exception ex) {
            Debug.LogErrorFormat("Loading slots from file failed\n {0}\n StackTrace: {1}", ex.Message, ex.StackTrace);
            return false;
        }
        finally {
            file?.Close();
        }
    }

    private void FindAndRegisterSavers() {
        foreach(var roots in SceneManager.GetActiveScene().GetRootGameObjects()) {
            foreach(var saver in roots.GetComponentsInChildren<Saver>(includeInactive: true)) {
                Register(saver.id, saver);
            }
        }
    }

    public void Register(SaverID id, Saver saver) {
        if(!currentSavers.ContainsKey(id)) {
            currentSavers[id] = saver;
        }
    }

    public void Unregister(SaverID id, Saver saver) {
        if(currentSavers.ContainsKey(id)) {
            currentSavers.Remove(id);
        }
    }

    public void Save() {
        Debug.Log("Saving to slot #" + currentSlot);

        var data = new SaveContainer() {
            sceneName = SceneManager.GetActiveScene().name,
            saverDatas = CollectSaverDatas()
        };
        saveThread?.Join();
        saveSlots[currentSlot] = data;

        saveThread = new Thread(SaveThreadProc);
        saveThread.Start();
    }

    private void SaveThreadProc() {
        Debug.Log("Saving thread started");
        string backupPath = MakeBackup(savePath);
        FileStream file = null;
        try {
            file = File.OpenWrite(savePath);
            new BinaryFormatter().Serialize(file, saveSlots);
            Debug.Log("Save successful");
        }
        catch(Exception ex) {
            Debug.LogErrorFormat("Save failed\n {0}\n StackTrace: {1}", ex.Message, ex.StackTrace);
            RestoreBackup(savePath, backupPath);
            throw;
        }
        finally {
            file?.Close();
            RemoveBackup(backupPath);
        }
    }

    private string MakeBackup(string originalPath) {
        if(!File.Exists(originalPath)) {
            return null;
        }
        string backupPath = originalPath + ".bck";
        File.Copy(originalPath, backupPath);
        return backupPath;
    }

    private void RestoreBackup(string originalPath, string backupPath) {
        if(backupPath == null) {
            return;
        }
        File.Copy(backupPath, originalPath);
    }

    private void RemoveBackup(string backupPath) {
        if(File.Exists(backupPath)) {
            File.Delete(backupPath);
        }
    }

    private Dictionary<SaverID, object> CollectSaverDatas() {
        var saverDatas = new Dictionary<SaverID, object>();
        foreach(var pair in currentSavers) {
            try {
                object entry = pair.Value.OnSave();
                saverDatas[pair.Key] = entry;
            }
            catch(Exception ex) {
                var name = pair.Value != null ? $"{pair.Value.name} (ID: {pair.Value.id})" : "<NULL>";
                Debug.LogError($"Failed to save saver {name}.\n{ex}");
            }
        }
        return saverDatas;
    }

    public void Load() {
        Debug.Log("Loading from slot #" + currentSlot);
        
        saveThread?.Join();
        if(!saveSlots.ContainsKey(currentSlot)) {
            Debug.LogErrorFormat("[ERROR] Slot #{0} doesn't exist", currentSlot);
            return;
        }
        
        StartCoroutine(LoadCoroutine(saveSlots[currentSlot]));
    }

    public void PassToNextSceneAndSave(string sceneName) {
        Debug.LogFormat("Passing from level {0} to level {1} (slot #{2})", 
            SceneManager.GetActiveScene().name, sceneName, currentSlot);

        saveThread?.Join();
        StartCoroutine(PassToNextSceneCoroutine(sceneName));
    }

    private IEnumerator PassToNextSceneCoroutine(string sceneName) {
        var prevSaverDatas = CollectSaverDatas();

        yield return UI.instance.LoadScene(sceneName);

        foreach(var pairPrev in prevSaverDatas) {
            SaverID prevSaverID = pairPrev.Key;
            if(currentSavers.ContainsKey(prevSaverID)) {
                Saver currSaver = currentSavers[prevSaverID];
                object prevSaverData = pairPrev.Value;
                currSaver.OnLoad(prevSaverData);
            }
        }
        Save();
    }

    private IEnumerator LoadCoroutine(SaveContainer data) {
        yield return UI.instance.LoadScene(data.sceneName);
        
        foreach(var pair in data.saverDatas) {
            if(!currentSavers.ContainsKey(pair.Key)) {
                Debug.LogWarningFormat("Load data for nonexistent object: {0}", pair.Key.ToString());
                continue;
            }
            Saver saver = currentSavers[pair.Key];
            if(saver == null) {
                Debug.LogError($"Saver of ID {pair.Key} is null");
                continue;
            }
            try {
                saver.OnLoad(pair.Value);
            }
            catch(Exception ex) {
                Debug.LogError($"Failed to load saver {saver.name} (ID: {saver.id}).\n{ex}");
            }
        }
    }
}
