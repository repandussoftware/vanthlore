using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class SettingsManager : MonoBehaviour
{
    private bool _isSaving = false; // Kaydetme işlemi devam ediyor mu kontrolü
    [System.Serializable]
    public struct SettingsTab
    {
        public Button tabButton;
        public GameObject boardObject;
        public GameObject selectedBorder;
    }

    public List<SettingsTab> tabs;

    public static SettingsManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    void Start()
    {
        // Başlangıçta ilk sekmeyi (örneğin Sound) aktif et, diğerlerini kapat
        ShowTab(0);

        // Butonlara tıklama dinleyicilerini otomatik ekle
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i; // Closure hatasını önlemek için
            tabs[i].tabButton.onClick.AddListener(() => ShowTab(index));
        }
    }

    public void ShowTab(int index)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            // Eğer mevcut index ise aktif et, değilse pasif et
            bool isActive = (i == index);

            tabs[i].boardObject.SetActive(isActive);
            tabs[i].selectedBorder.SetActive(isActive);
        }
    }

    public async void saveSoundSettings()
    {
        try
        {
            if (StatsManager.Instance != null && SaveManager.instance != null)
            {
                SaveData currentData = new SaveData();
                StatsManager.Instance.ExportToSaveData(currentData);
                currentData.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                currentData.saveName = "Settings_Save_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                currentData.masterVolume = StatsManager.Instance.masterVolume;
                currentData.musicVolume = StatsManager.Instance.musicVolume;
                currentData.sfxVolume = StatsManager.Instance.sfxVolume;
                await SaveManager.instance.SaveGame(currentData, "Aritheon_QuickSave");
            }
        }
        catch (System.Exception e) { Debug.LogError("Save Hatası: " + e.Message); }
        finally { _isSaving = false; }
    }
}

