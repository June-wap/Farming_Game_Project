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
        WriteDatabase("User1", "Hello Firebase Database!");
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
        
    }
}


