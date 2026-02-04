using UnityEngine;

// 플레이어가 가까이 오면 폭탄이 기폭시작.
// 시작 3초후 소리를 내면서 폭발.
// 만약, 폭발 시 특정 위치내에 플레이어가 존재하면
// 플레이어는 리스폰
// 소리는 나중에 AudioManager로 진행
// 깜빡거리는 효과만 낸다.
public class Bomb : MonoBehaviour
{
    [Header("1. 기본 설정")]
    public float ExplosionDelay; // 작동 후 폭발까지 얼마나 걸릴 것인지.
    public float ExplosionRadius; // 폭발 반경
    public float ExplosionForce; // 폭발력

    [Header("2. 폭발 경고 효과")]
    public Color WarningColor; // 기폭 시 깜빡거릴 색
    public float BlinkSpeed; // 깜빡거릴 속도

    private float timer; // 폭발 시간 측정을 위한 내부적인 타이머
    private bool isActivated; // 폭탄이 작동을 시작했는지
    private Renderer meshRenderer; // 폭탄 작동 시 색변경을 위한 렌더러
    private Color originColor; // 폭탄 본래의 색


    void Start()
    {
        meshRenderer = GetComponent<Renderer>(); // 내가 가지고 있는 렌더러 대입
        originColor = meshRenderer.material.color; // 최초로 내가 가지고 있는 색
    }

    void Update()
    {
        if (isActivated)
        {
            Blink();
        }
    }

    //만들어야하는 함수

    /// <summary>
    /// 직접적으로 폭발하는 => 주변 물체에 힘을주고, 플레이어는 리스폰시키는 함수
    /// </summary>
    private void Explode()
    {
        //만약 폭발 반경내에 플레이어라면
        Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);

        foreach (Collider col in colliders)
        {
            PlayerMovement player = col.GetComponent<PlayerMovement>();

            // 리스폰
            if (player != null)
            {
                player.Respawn();
            }
        }

        // 완료했으면 폭탄 제거.
        Destroy(gameObject);
    }

    /// <summary>
    /// 깜빡거리는 함수
    /// </summary>
    private void Blink()
    {
        // ★ 우선 타이머에 현재 지나가고 있는 시간을 대입해서 ExplosionTime이 지나가면 Explode()를 호출함.
        timer += Time.deltaTime;

        // 깜빡거리는거 어떻게함?
        // 시간이 지날수록 더 빠르게 깜빡거릴것임.
        float speedMultiplier = 1f + (timer / ExplosionDelay) * 3f;
        //0과 1사이를 왔다갔다하는 보간을 위한 값
        float lerpValue = Mathf.PingPong(Time.time * BlinkSpeed * speedMultiplier, 1f);
        // ★ 색을 자연스럽게 깜빡거리는 연출
        meshRenderer.material.color = Color.Lerp(originColor, WarningColor, lerpValue);

        // ★ 만약 현재시간이 정해진 시간보다 커졌다면
        if (timer >= ExplosionDelay)
        {
            // 폭발 호출
            Explode();
        }
    }

    /// <summary>
    /// 감지했으면, 작동시키는 함수
    /// </summary>
    private void Activate()
    {
        // 작동을 위해 isActivated를 true로 만들어줌.
        isActivated = true;
    }

    /// <summary>
    /// 플레이어와 충돌했음을 감지하는 함수
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // 충돌체의 태그가 Player이면서, isActivated가 false일 때
        if (other.CompareTag("Player") && !isActivated)
        {
            // 작동 시작
            Activate();
        }
    }

    /// <summary>
    /// (선택)폭발 반경을 보여주는 함수
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        //기즈모 색 결정
        Gizmos.color = Color.red;
        // 구형 그리기
        Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
    }
}