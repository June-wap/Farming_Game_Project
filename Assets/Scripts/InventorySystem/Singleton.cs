using UnityEngine;

// Singleton Pattern chuẩn: Đảm bảo chỉ có 1 Manager duy nhất tồn tại và có thể gọi từ bất kỳ đâu.
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instant;
    public static T Instant
    {
        get
        {
            if (_instant == null)
            {
                _instant = FindAnyObjectByType<T>();
                if (_instant == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instant = obj.AddComponent<T>();
                }
            }
            return _instant;
        }
    }
    
    protected virtual void Awake()
    {
        if (_instant == null)
        {
            _instant = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instant != this as T)
        {
            Destroy(gameObject);
        }
    }
}
