using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Keeps additive scene loading separate from screen navigation.
public class GameSceneLoader
{
    private readonly Scene menuScene;
    public Scene GameplayScene { get; private set; }
    public bool HasActiveGame => GameplayScene.IsValid() && GameplayScene.isLoaded;
    public string LastError { get; private set; } = "";

    public GameSceneLoader(Scene menuScene)
    {
        this.menuScene = menuScene;
    }

    public IEnumerator ChangeScene(bool loadGame, string scenePath)
    {
        LastError = "";
        if (HasActiveGame)
        {
            if (menuScene.IsValid() && menuScene.isLoaded)
                SceneManager.SetActiveScene(menuScene);
            yield return SceneManager.UnloadSceneAsync(GameplayScene);
            GameplayScene = default;
        }

        if (loadGame)
        {
            AsyncOperation loading = null;
            try
            {
                loading = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
            if (loading != null)
            {
                yield return loading;
                GameplayScene = SceneManager.GetSceneByPath(scenePath);
            }
            if (HasActiveGame)
                SceneManager.SetActiveScene(GameplayScene);
            else
                LastError = "Couldn't open the level. Please try again.";
        }

    }
}
