using UnityEngine;
using TMPro;

public class display_score : MonoBehaviour
{
    public GameObject Scoreboard;
    private Score_Noting current_score;
    private TMP_Text content;
    private string label = "Score: ";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        content = GetComponent<TMP_Text>();

        // If a Scoreboard object is wired up, read the score through it.
        if (Scoreboard != null)
        {
            current_score = Scoreboard.GetComponent<Score_Noting>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        show();
    }

    void show()
    {

        int score = current_score != null ? current_score.getscore() : GameData.Clicks;

        content.text = label + score;
    }
}
