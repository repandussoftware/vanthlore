using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class CloudSceneBootstrapper : MonoBehaviour
{
    public static CloudSceneBootstrapper Instance { get; private set; }

    [Header("--- BACKEND CONFIG ---")]
    [SerializeField] private string backendApiUrl = "https://vanthlore.repandus.com/api/game/scene/";

    private SceneInstance loadedCellSceneInstance;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void InitiateCloudSceneLoad(string sceneName)
    {
        StartCoroutine(ExecuteFullCloudPipeline(sceneName));
    }

    private IEnumerator ExecuteFullCloudPipeline(string sceneName)
    {
        Debug.Log($"<color=cyan><b>[Aritheon]</b></color> '{sceneName}' haritası buluttan alınıyor...");

        string targetUrl = $"{backendApiUrl}{sceneName}/objects";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(targetUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Pipeline] Veritabanı bağlantısı patladı: {webRequest.error}");
                yield break;
            }

            string jsonResponse = webRequest.downloadHandler.text;
            CloudSceneDataWrapper incomingData = JsonUtility.FromJson<CloudSceneDataWrapper>(jsonResponse);

            var sceneLoadHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return sceneLoadHandle;

            if (sceneLoadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedCellSceneInstance = sceneLoadHandle.Result;
                SceneManager.SetActiveScene(loadedCellSceneInstance.Scene);
            }

            StartCoroutine(SpawnAndDistributeObjects(incomingData.objects));
        }
    }

    private IEnumerator SpawnAndDistributeObjects(List<CloudObjectNetData> objectsToSpawn)
    {
        GameObject cameraBoundary = GameObject.Find("CameraBoundary");
        int completedSpawns = 0;

        foreach (var obj in objectsToSpawn)
        {
            // Veritabanındaki mutlak değerleri olduğu gibi alıyoruz
            Vector3 targetPos = new Vector3(obj.pos_x, obj.pos_y, obj.pos_z);
            Vector3 targetScale = new Vector3(obj.scale_x, obj.scale_y, obj.scale_z);
            Quaternion targetRot = Quaternion.Euler(obj.rot_x, obj.rot_y, obj.rot_z);

            Addressables.InstantiateAsync(obj.addressable_key, targetPos, targetRot).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject spawnedObj = handle.Result;
                    
                    // Önce sahneye taşı, sonra parent'a al
                    SceneManager.MoveGameObjectToScene(spawnedObj, loadedCellSceneInstance.Scene);
                    
                    // Ebeveyn ataması (Sadece parent atandıktan sonra ölçek/pozisyon etkilenir)
                    if (cameraBoundary != null)
                        spawnedObj.transform.SetParent(cameraBoundary.transform);

                    // Ölçeği en son uyguluyoruz ki ebeveynden kaynaklı bozulmalar sıfırlansın
                    spawnedObj.transform.localScale = targetScale;
                    spawnedObj.transform.localPosition = targetPos; // Yerel koordinatlara zorla

                    Debug.Log($"<color=green>Yerleşti:</color> {obj.addressable_key}");
                }
                else
                {
                    Debug.LogError($"[Addressables] '{obj.addressable_key}' anahtarı katalogda yok!");
                }
                completedSpawns++;
            };
        }

        yield return new WaitUntil(() => completedSpawns >= objectsToSpawn.Count);
        Debug.Log("<color=green><b>[Tüm nesneler mermer nizamında dizildi!]</b></color>");
    }
}

[Serializable] public class CloudSceneDataWrapper { public List<CloudObjectNetData> objects; }

[Serializable]
public class CloudObjectNetData
{
    public int id;
    public string addressable_key;
    public float pos_x, pos_y, pos_z;
    public float scale_x, scale_y, scale_z;
    public float rot_x, rot_y, rot_z;
}