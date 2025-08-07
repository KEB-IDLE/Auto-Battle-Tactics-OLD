using System.Collections;

public interface IMatchInterface
{
    int UserId { get; }                  // ✅ SessionManager에서 가져오기
    IEnumerator StartMatchFlow();       // 매칭 시작
    IEnumerator EndMatchFlow();         // 매칭 종료
}
