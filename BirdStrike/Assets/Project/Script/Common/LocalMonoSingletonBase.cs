using UnityEngine;

namespace Common
{
    /// <summary>
    /// シーンをまたがないシングルトン
    /// </summary>
    public class LocalMonoSingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType<T>();
                }
                return instance;
            }
    
        }
    }
}

