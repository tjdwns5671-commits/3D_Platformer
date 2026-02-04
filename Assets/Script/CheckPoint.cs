using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [Header("수정할 매테리얼")]
    public Material CheckTrueMat; // 활성화되었을때 매테리얼
    public Material CheckFalseMat; // 비활성화상태의 매테리얼

    private MeshRenderer meshRenderer; // 매테리얼 변경을 위한 메쉬렌더러
    private bool isActivated; // 작동되었는지의 여부.

    private void Start()
    {
        // 메쉬렌더러 초기화
        meshRenderer = GetComponent<MeshRenderer>();
        // 매테리얼 초기화
        UpdateMaterial();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 물체가 플레이어 일때, 동시에 아직 작동하지않은 체크포인트인지 확인
        if (other.CompareTag("Player") && !isActivated)
        {
            // 충돌한 콜라이더에서 PlayerMovement를 가져옴
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            // PlayerMovement를 가져올 수 있을때
            if (player != null)
            {
                // 스폰포인트 변경 함수를 호출
                player.SetSpawnPoint(transform.position);

                // 이미 작동한 체크포인트는 다시 작동하지 않도록
                isActivated = true;
                // 작동한 매테리얼 적용
                UpdateMaterial();
            }

        }
    }

    /// <summary>
    /// 작동 상태에 따라서 매테리얼을 변경함.
    /// </summary>
    private void UpdateMaterial()
    {
        // 작동 여부에 따라서
        if (isActivated)
        {
            // 작동한 매테리얼
            meshRenderer.sharedMaterial = CheckTrueMat;
        }
        else
        {
            // 작동하지 않은 매테리얼
            meshRenderer.sharedMaterial = CheckFalseMat;
        }
    }




}