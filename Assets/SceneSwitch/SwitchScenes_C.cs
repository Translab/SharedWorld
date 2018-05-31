using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScenes_C : MonoBehaviour {
    public string scenename;
    public static bool switching = false;
    // Use this for initialization
    private int i = 0;

    void OnTriggerEnter(Collider collision)
    {
        if (i == 0)
        {
            switching = true;
            StartCoroutine(Loading_scene());
            i++;
        }
    }

    IEnumerator Loading_scene()
    {

        yield return new WaitForSecondsRealtime(1.75f);
        SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Single);
    }
    void OnDestroy()
    {
        switching = false;
    }
}
