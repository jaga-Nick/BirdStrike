using UnityEngine;


namespace Common
{
    /// <summary>
    /// シーンをまたぐシングルトン
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GlobalMonoSingletonBase<T> : MonoBehaviour where T : GlobalMonoSingletonBase<T>
    {
        protected static T instance;

        /// <summary>
        /// 生成
        /// </summary>
        public static T Instance()
        {
            if (instance == null)
            {
                var gameObject = new GameObject(typeof(T).Name);
                instance = gameObject.AddComponent<T>();
                DontDestroyOnLoad(gameObject);
            }
            return instance;
        }
    }
}