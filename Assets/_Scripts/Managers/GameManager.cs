using UnityEngine;

namespace _Scripts.Managers
{
    [DefaultExecutionOrder(-150)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public WordManager WordManager { get; private set; }
        
        public SoundManager SoundManager { get; private set; }

        private void Awake()
        {
            Persist();
            GetManagers();
        }
        
        private void Persist()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void GetManagers()
        {
            if (TryGetComponent(out WordManager wordManager))
            {
                WordManager = wordManager;
            }            
            if (TryGetComponent(out SoundManager soundManager))
            {
                SoundManager = soundManager;
            }
                
        }
    }
}
