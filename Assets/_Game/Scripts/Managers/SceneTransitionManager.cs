using UnityEngine;
using UnityEngine.SceneManagement;

namespace K2Y.Managers
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [Header("Scene Transition Settings")]
        [SerializeField] private float transitionDelay = 0.5f;
        
        // Singleton pattern for easy access across the game
        public static SceneTransitionManager Instance { get; private set; }

        private void Awake()
        {
            // Implement singleton pattern
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

        #region Public Methods

        /// <summary>
        /// Load LevelOne from TitleScreen
        /// </summary>
        public void StartGame()
        {
            LoadSceneWithDelay("LevelOne");
        }

        /// <summary>
        /// Return to the title screen
        /// </summary>
        public void ReturnToTitle()
        {
            LoadSceneWithDelay("TitleScreen");
        }

        /// <summary>
        /// Load a specific scene by name
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void LoadScene(string sceneName)
        {
            LoadSceneWithDelay(sceneName);
        }

        /// <summary>
        /// Load the next level in sequence
        /// </summary>
        public void LoadNextLevel()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            
            // Check if next scene exists
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                LoadSceneByIndex(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("No next level available. Returning to title screen.");
                ReturnToTitle();
            }
        }

        /// <summary>
        /// Restart the current scene
        /// </summary>
        public void RestartCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            LoadSceneWithDelay(currentSceneName);
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        #endregion

        #region Private Methods

        private void LoadSceneWithDelay(string sceneName)
        {
            if (transitionDelay > 0)
            {
                Invoke(nameof(LoadSceneAsync), transitionDelay);
                currentSceneToLoad = sceneName;
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
        }

        private void LoadSceneByIndex(int sceneIndex)
        {
            if (transitionDelay > 0)
            {
                Invoke(nameof(LoadSceneByIndexAsync), transitionDelay);
                currentSceneIndexToLoad = sceneIndex;
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneIndex);
            }
        }

        // Helper variables for delayed loading
        private string currentSceneToLoad;
        private int currentSceneIndexToLoad;

        private void LoadSceneAsync()
        {
            SceneManager.LoadSceneAsync(currentSceneToLoad);
        }

        private void LoadSceneByIndexAsync()
        {
            SceneManager.LoadSceneAsync(currentSceneIndexToLoad);
        }

        #endregion

        #region Debug Methods (Remove in final build)

        private void Update()
        {
            // Debug hotkeys for testing (remove these in final build)
            if (Debug.isDebugBuild)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LoadScene("TitleScreen");
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadScene("LevelOne");
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartCurrentScene();
                }
            }
        }

        #endregion
    }
}