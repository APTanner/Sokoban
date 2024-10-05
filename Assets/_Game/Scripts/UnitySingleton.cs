using UnityEngine;

public abstract class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_instance;
    private static bool m_bQuitting = false;

    public static T Instance
    {
        get
        {
            if (m_bQuitting)
            {
                Debug.LogWarning($"Singleton of type '{typeof(T).ToString()}' was referenced during application quit");
                return null;
            }

            if (s_instance == null)
            {
                s_instance = FindObjectOfType<T>();

                if (s_instance == null)
                {
                    Debug.Log($"Didn't find an instance of {typeof(T).ToString()}");
                    GameObject singleton = new GameObject();
                    s_instance = singleton.AddComponent<T>();
                    singleton.name = "[Singleton] " + typeof(T).Name.ToString();
                }
            }
            return s_instance;
        }
    }

    public static bool IsAvailable => !m_bQuitting;

    protected virtual void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this as T;
        }
        else if (s_instance != this)
        {
            Debug.LogError($"Multiple instances of singleton class '{typeof(T).Name}'. Destroying instance.");
            Destroy(this.gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        if (Application.isPlaying)
        {
            m_bQuitting = true;
        }
    }
}