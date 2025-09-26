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
        protected virtual void Awake()
        {
            if (instance == null)
            {
                // 自分自身を静的なインスタンスとして登録
                instance = this as T;
                // シーンをまたいでも破棄されないようにする
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                // 既に別のインスタンスが存在する場合は、自分を破棄する
                Destroy(gameObject);
                return;
            }
            
            instance = this as T;
        }
        
        
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