using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    // Time Scale: 1 real second = 1 in-game minute
    // 60 real seconds (1 real minute) = 60 in-game minutes = 1 in-game hour
    private float timer;
    
    public int gameMinute = 0;
    public int gameHour = 6; // Start at 6 AM
    public int gameDay = 1;

    public Action<int, int, int> OnGameMinutePassed; // Minute, Hour, Day
    public Action<int, int> OnGameHourPassed; // Hour, Day
    public Action<int> OnGameDayPassed; // Day

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
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        // 1 real second has passed = 1 in-game minute
        if (timer >= 1f)
        {
            timer -= 1f;
            gameMinute++;

            if (gameMinute >= 60)
            {
                gameMinute = 0;
                gameHour++;
                OnGameHourPassed?.Invoke(gameHour, gameDay);

                if (gameHour >= 24)
                {
                    gameHour = 0;
                    gameDay++;
                    OnGameDayPassed?.Invoke(gameDay);
                }
            }

            OnGameMinutePassed?.Invoke(gameMinute, gameHour, gameDay);
        }
    }
}
