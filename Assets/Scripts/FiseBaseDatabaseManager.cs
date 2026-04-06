using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FiseBaseDatabaseManager : MonoBehaviour
{
    private DatabaseReference reference;

    private void Awake()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;

    }

    public void Start()
    {
        TilemapDetail tilemapDetail = new TilemapDetail(0, 0, TilemapState.Ground);
        WriteDatabase("User1", tilemapDetail.ToString());
        ReadDatabase("User1");
    }

    public void WriteDatabase(string id, string message)
    {
        reference.Child("Users").Child(id).SetValueAsync(message).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Du Lieu Da Duoc Ghi Vao Database");
            }
            else
            {
                Debug.LogError("Du Lieu Khong Duoc Ghi Vao Database " + task.Exception);
            }
        });
    }

    public void ReadDatabase(string id)
    {
        reference.Child("Users").Child(id).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("Du Lieu Da Duoc Doc Tu Database: " + snapshot.Value.ToString());
            }
            else
            {
                Debug.LogError("Du Lieu Khong Duoc Doc Tu Database " + task.Exception);
            }
        });
    }
}
