using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance;

    private DatabaseReference dbReference;
    private const string PLAYER_ID = "DefaultPlayer"; // Hardcode định danh SinglePlayer

    private void Awake()
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

        // Init Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase Database Initialized!");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    /// <summary>
    /// Lưu toàn bộ dữ liệu Map hiện tại và Inventory
    /// </summary>
    public void SaveGame(string currentMapName)
    {
        if (dbReference == null) return;

        // Xử lý Map Data
        MapSaveData mapToSave = new MapSaveData();
        mapToSave.mapName = currentMapName;
        if (CropManager.Instance != null)
        {
            mapToSave.crops = CropManager.Instance.GetActiveCropsList();
        }

        string mapJson = JsonUtility.ToJson(mapToSave);
        dbReference.Child("Users").Child(PLAYER_ID).Child("Maps").Child(currentMapName)
            .SetValueAsync(mapJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) Debug.LogError("Lưu Map lỗi: " + task.Exception);
            else Debug.Log($"Đã Push dữ liệu Nông trại {currentMapName} lên Firebase!");
        });

        // Xử lý Inventory Data
        RecyclableScrollerIventory inventory = FindObjectOfType<RecyclableScrollerIventory>();
        if (inventory != null)
        {
            InventorySaveData invData = new InventorySaveData();
            invData.items = inventory.GetInventoryList();
            
            string invJson = JsonUtility.ToJson(invData);
            dbReference.Child("Users").Child(PLAYER_ID).Child("Inventory")
                .SetValueAsync(invJson).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) Debug.LogError("Lưu Inventory lỗi: " + task.Exception);
                else Debug.Log("Đã đồng bộ Túi đồ lên mây!");
            });
        }
    }

    /// <summary>
    /// Tải dữ liệu Map và bung ra UI/Map
    /// Callback được dùng để chờ Firebase phản hồi trước khi Render
    /// </summary>
    public void LoadGame(string loadMapName, Action<MapSaveData> onMapLoaded, Action<InventorySaveData> onInventoryLoaded)
    {
        if (dbReference == null) return;

        // Kéo Map
        dbReference.Child("Users").Child(PLAYER_ID).Child("Maps").Child(loadMapName)
            .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Tải Map lỗi: " + task.Exception);
            }
            else if (task.IsCompleted && task.Result.Exists)
            {
                string json = task.Result.Value.ToString();
                MapSaveData mapData = JsonUtility.FromJson<MapSaveData>(json);
                onMapLoaded?.Invoke(mapData);
            }
            else
            {
                Debug.Log($"Map {loadMapName} chưa có Save (Đất trồng rỗng).");
                onMapLoaded?.Invoke(new MapSaveData()); // Trả về Map rỗng nếu New Game ở khu này
            }
        });

        // Kéo Inventory
        dbReference.Child("Users").Child(PLAYER_ID).Child("Inventory")
            .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Tải túi đồ lỗi: " + task.Exception);
            }
            else if (task.IsCompleted && task.Result.Exists)
            {
                string json = task.Result.Value.ToString();
                InventorySaveData invData = JsonUtility.FromJson<InventorySaveData>(json);
                onInventoryLoaded?.Invoke(invData);
            }
            else
            {
                Debug.Log("Không có save túi đồ (Balo rỗng).");
            }
        });
    }
}
