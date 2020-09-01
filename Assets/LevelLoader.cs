using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    SceneManager sceneManager;
    Animator fadeout;
    
    [Tooltip("Wait time before loading the scene")] public float transitionTime = 1.0f;

    private void Awake()
    {
        // So noone has to do a thing to get the Animator
        fadeout = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadLevelNext()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int level_index)
    {
        // Play animation
        fadeout.SetTrigger("Start");

        // Wait til it's done
        yield return new WaitForSeconds(transitionTime);

        // Load level
        SceneManager.LoadScene(level_index);
    }
}
