using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LazySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
        static readonly Lazy<T> LazyInstance = new Lazy<T>(CreateSingleton);
        public static T Inst => LazyInstance.Value;

        static T CreateSingleton()
        {
            var obj = new GameObject("(Singleton)" + typeof(T).Name);
            DontDestroyOnLoad(obj);
            return obj.AddComponent<T>();
        }
#if UNITY_EDITOR
        protected virtual void Reset()
        {
            UnityEditor.EditorUtility.DisplayDialog("LazySingletons should not be manually instantiated", "Lazyloaded Singletons will load when first accessed. These act as managers objects should reference themselves to", "Ok");
            DestroyImmediate(this);
        }
#endif
}
