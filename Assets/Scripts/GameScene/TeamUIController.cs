using UnityEngine;
using TMPro;

public class TeamUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI teamText;
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color blueColor = Color.blue;

    public void SetTeam(Team team)
    {
        if (team == Team.Red)
        {
            teamText.text = "TEAM:Red";
            teamText.color = redColor;
        }
        else
        {
            teamText.text = "TEAM:Blue";
            teamText.color = blueColor;
        }
    }
}
