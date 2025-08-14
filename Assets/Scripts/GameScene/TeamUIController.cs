using UnityEngine;
using UnityEngine.UI;

public class TeamUIController : MonoBehaviour
{
    [SerializeField] private Image teamIcon; // 팀 아이콘을 위한 Image 컴포넌트
    [SerializeField] private Sprite redTeamIcon; // Red 팀 아이콘
    [SerializeField] private Sprite blueTeamIcon; // Blue 팀 아이콘

    public void SetTeam(Team team)
    {
        if (team == Team.Red)
        {
            teamIcon.sprite = redTeamIcon; // Red 팀 아이콘 설정
        }
        else
        {
            teamIcon.sprite = blueTeamIcon; // Blue 팀 아이콘 설정
        }
    }
}
