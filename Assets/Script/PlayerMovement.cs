using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("1. 이동 관련 설정값")]
    public float MoveSpeed; // 이동속도
    public float JumpPower; // 점프력
    public float RotateSpeed; // 회전속도
    public float RespawnHeight; // 리스폰 하는 높이
    public float WalkingDamping; // 걸을때 마찰력

    [Header("2. 지면 감지 관련 설정값")]
    public Transform GroundCheckPos; // 플레이어의 발밑 위치
    public float GroundCheckRadius; // 지면 감지 구체의 지름
    public LayerMask GroundCheckLayerMask; // 감지할 지면의 레이어

    [Header("3. 카메라 관련 설정값")]
    public GameObject CameraRoot; // 시점의 중심이 될 위치.
    public float TopClamp; // 위쪽 시야의 최댓값
    public float BottomClamp; // 아랫쪽 시야의 최솟값
    public bool InvertXAxis; // 상하 회전 반전 여부

    public Animator Animator;

    //내부 변수
    private Rigidbody rb; // 물리엔진 이동용.
    private Vector2 moveInput; // 이동 입력값
    private Vector2 lookInput; // 회전 입력값
    private bool isGrounded; // 지면 감지용
    private float targetX; // X축 회전값
    private float targetY; // Y축 회전값
    private Vector3 spawnPoint; // 리스폰 포인트
    private int animIDSpeed; // 애니메이션 해시값 - Speed;
    private int animIDJump; // 애니메이션 해시값 - Jump;
    // 플레이어 관련 주석을 추가함.

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        targetY = CameraRoot.transform.rotation.eulerAngles.y;

        // 애니메이션 해시값 할당
        animIDSpeed = Animator.StringToHash("Speed");
        animIDJump = Animator.StringToHash("Jump");
    }

    // 입력부
    /// <summary>
    /// 마우스 입력을 받는 함수
    /// </summary>
    /// <param name="value">마우스 입력값</param>
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    /// <summary>
    /// 키보드 입력을 받아 이동에 사용하는 함수
    /// </summary>
    /// <param name="value">키보드 입력</param>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    /// <summary>
    /// 스페이스 바 입력을 받아 점프하는 함수
    /// </summary>
    /// <param name="value"></param>
    public void OnJump(InputValue value)
    {
        //땅에 닿아있을때만 점프가능.
        if (isGrounded)
        {
            // 현재 점프하고 있는 속력 초기화.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            // 위쪽을 향해 한번에 힘을 줌.
            rb.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);

            // 점프 트리거 실행.
            Animator.SetTrigger(animIDJump);
        }
    }

    /// <summary>
    /// 지면에 닿았는지 감지하는 함수
    /// </summary>
    private void GroundCheck()
    {
        // 플레이어의 발밑의 위치.
        if (GroundCheckPos != null)
        {
            // 만약, 그라운드 레이어가 감지되면 true, 아니면 false
            isGrounded = Physics.CheckSphere(GroundCheckPos.position, GroundCheckRadius, GroundCheckLayerMask, QueryTriggerInteraction.Ignore);
            // 땅에 붙어있을때는
            if (isGrounded)
            {
                // 미리정해진 마찰력 적용
                rb.linearDamping = WalkingDamping;
            }
            else
            {
                // 빨리 떨어질수있도록 마찰력 0 적용
                rb.linearDamping = 0;
            }
        }
        else
        {
            Debug.LogError("지면감지필요");
        }
    }

    void Update()
    {
        CameraRotation();
    }

    private void FixedUpdate()
    {
        Move();
        GroundCheck();
        CheckRespawn();
    }


    /// <summary>
    /// 카메라를 마우스 입력에 따라 플레이어를 기준으로 회전하도록 만듬.
    /// </summary>
    private void CameraRotation()
    {
        // 좌우회전
        targetY += lookInput.x * RotateSpeed;

        // 상하회전, 카메라 반전 여부에 따라 더하고 빼줌.
        if (InvertXAxis)
            targetX += lookInput.y * RotateSpeed;
        else
            targetX -= lookInput.y * RotateSpeed;

        // 상하 이동은 정해진 값 안에서만, 좌우이동은 float 최대,최소값으로
        targetX = Mathf.Clamp(targetX, BottomClamp, TopClamp);
        targetY = Mathf.Clamp(targetY, float.MinValue, float.MaxValue);

        // 좌우로 회전하면, 플레이어의 몸 자체가 회전함.
        transform.rotation = Quaternion.Euler(0f, targetY, 0f);

        //상하 회전. 카메라 루트를 회전시킴.
        CameraRoot.transform.localRotation = Quaternion.Euler(targetX, 0f, 0f);

    }


    /// <summary>
    /// 이동함수
    /// </summary>
    private void Move()
    {
        // 이동 입력이 있을때만 이동
        if (moveInput != Vector2.zero)
        {
            // 보고 있는 방향 확인용
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // WASD 이동은 Y축 이동 없음.
            forward.y = 0f;
            right.y = 0f;

            //벡터 정규화
            forward.Normalize();
            right.Normalize();

            // 움직일 방향 벡터 = (앞쪽 방향 * W,S키 입력 + 옆쪽방향 * A,D키 입력).정규화;
            Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;

            // 최종적으로 움직일 벡터 = 움직일 방향 벡터 * 이동속도;
            Vector3 finalVelocity = moveDir * MoveSpeed;

            // 최종 속도 = 최종 벡터의 X값,Z값만 가지고 오면 됨.
            rb.linearVelocity = new Vector3(finalVelocity.x, rb.linearVelocity.y, finalVelocity.z);

            // 기존 최종 움직임 벡터 길이를 가져와서 애니메이션 적용.
            Animator.SetFloat(animIDSpeed, finalVelocity.magnitude, 0.01f, Time.deltaTime);
        }
        else // 이동 입력이 없을때
        {
            // 이동 입력이 없으면, 속력을 0으로 하여 애니메이션 적용.
            Animator.SetFloat(animIDSpeed, 0f, 0.01f, Time.deltaTime);
        }
    }

    /// <summary>
    /// 특정 위치까지 떨어지면 리스폰하게 만드는 함수.
    /// </summary>
    private void CheckRespawn()
    {
        // 만약, 플레이어의 높이가 특정 수치보다 낮으면
        if (transform.position.y <= RespawnHeight)
        {
            Respawn();
        }
    }

    /// <summary>
    /// 플레이어를 리스폰하는 함수
    /// </summary>
    public void Respawn()
    {
        // 현재 위치를 저장된 스폰 포인트로 옮기고
        transform.position = spawnPoint;
        // 속력을 초기화한다.
        rb.linearVelocity = Vector3.zero;
    }

    /// <summary>
    /// 스폰포인트를 매개변수로 받아 변경.
    /// </summary>
    /// <param name="point">변경한 스폰포인트 위치</param>
    public void SetSpawnPoint(Vector3 point)
    {
        // 스폰포인트를 받은 위치로 변경
        spawnPoint = point;
    }

    private void OnDrawGizmosSelected()
    {
        // 지면 체크 위치가 있을때만
        if (GroundCheckPos != null)
        {
            //기즈모 색 결정
            Gizmos.color = Color.green;
            // 구형 그리기
            Gizmos.DrawWireSphere(GroundCheckPos.position, GroundCheckRadius);
        }
    }
}