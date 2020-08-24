using UnityEngine;
using UnityEngine.UI;

public class EnemiesLeftCounter : MonoBehaviour
{
    private Text counterText;
    private float enemyCounter;
    public Canvas completeCanvas;
    public Canvas gamePlayScreen;
    // Start is called before the first frame update
    void Start()
    {
        completeCanvas.enabled = false;
        counterText = GetComponent<Text>();
    }

    public void subtract()
    {
        enemyCounter--;
        counterText.text = "Enemies left: " + enemyCounter;
        if(enemyCounter <= 0)
        {
            completeCanvas.enabled = true;
            gamePlayScreen.enabled = false;
            Time.timeScale = 0.0f;
        }
    }

    public void add()
    {
        enemyCounter++;
        counterText.text = "Enemies left: " + enemyCounter;
    }
    public void getNumberOfEnemies()
    {
        GameObject[] list = GameObject.FindGameObjectsWithTag("enemy");
        enemyCounter = list.Length;
        counterText.text = "Enemies left: " + enemyCounter;
    }
}
