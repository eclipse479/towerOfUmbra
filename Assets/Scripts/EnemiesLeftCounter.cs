using UnityEngine;
using UnityEngine.UI;

public class EnemiesLeftCounter : MonoBehaviour
{
    private Text counterText;
    private float enemyCounter;
    // Start is called before the first frame update
    void Start()
    {
        counterText = GetComponent<Text>();
        //getNumberOfEnemies();
    }

    public void subtract()
    {
        enemyCounter--;
        counterText.text = "Enemies left: " + enemyCounter;
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
