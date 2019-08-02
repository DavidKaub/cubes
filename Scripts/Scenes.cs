using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Scenes
{
    //as in https://forum.unity.com/threads/unity-beginner-loadlevel-with-arguments.180925/

    private static Dictionary<string, string> parameters;


    public static void MyLoad(string sceneName)
    {
        Debug.Log("Loading Scene");
        if(parameters != null)
        {
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                Debug.Log("Entry = "+ entry.Key + " = " + entry.Value);
                // do something with entry.Value or entry.Key
            }
        }

        SceneManager.LoadScene(sceneName);

    }

    public static void Drop()
    {
        Scenes.parameters = new Dictionary<string, string>();
    }

    public static void Load(string sceneName, Dictionary<string, string> parameters = null)
    {
        Scenes.parameters = parameters;
        SceneManager.LoadScene(sceneName);
    }

    public static void Load(string sceneName, string paramKey, string paramValue)
    {
        Scenes.parameters = new Dictionary<string, string>();
        Scenes.parameters.Add(paramKey, paramValue);
        SceneManager.LoadScene(sceneName);
    }

    public static Dictionary<string, string> getSceneParameters()
    {
        return parameters;
    }

    public static string getParam(string paramKey)
    {
        if (parameters == null) return "";
        return parameters[paramKey];
    }

    public static void setParam(string paramKey, string paramValue)
    {
        if (parameters == null)
            Scenes.parameters = new Dictionary<string, string>();
        Scenes.parameters.Add(paramKey, paramValue);
    }         
}