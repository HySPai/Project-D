using System;
using UnityEditor;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;

    public static T Instance
    {
        get
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return null;
            }
#endif
            if (instance == null)
            {
                Type t = typeof(T);
                instance = (T)FindObjectOfType(t);

                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        CheckInstance();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected bool CheckInstance()
    {
        if (instance == null)
        {
            instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }
        Destroy(this);
        return false;
    }

    public static T getInstance
    {
        get
        {
            if (instance)
            {
                return instance;
            }

            Type t = typeof(T);
            instance = (T)FindObjectOfType(t);

            return instance;
        }

    }

    public static bool HasInstance
    {
        get { return instance != null; }
    }

    public bool Destroy()
    {
        if (HasInstance && instance == this && instance.gameObject != null)
        {
            Destroy(instance.gameObject);
            instance = null;
            return true;
        }
        return false;
    }
}

