using UnityEngine;

public class JumpPad : MonoBehaviour
{

    public float JumpForce; // 점프력

    // 트리거 콜라이더일 경우
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rigid = other.GetComponent<Rigidbody>();
            PadJump(rigid);
        }
    }

    // 콜리젼 콜라이더이기 때문에 사용
    private void OnCollisionEnter(Collision collision)
    {
        // 태그를 감지할때 collision.gameObject.CompareTag
        if (collision.gameObject.CompareTag("Player"))
        {
            // 현재 충돌한 물체에서 리지드바디를 가져옴(플레이어)
            Rigidbody rigid = collision.gameObject.GetComponent<Rigidbody>();
            PadJump(rigid);
        }
    }

    /// <summary>
    /// 매개변수로 받은 리지드 바디에 점프력을 추가해준다.
    /// </summary>
    /// <param name="rigid">리지드 바디</param>
    private void PadJump(Rigidbody rigid)
    {
        // 기존의 Y값을 초기화시켜야함.
        rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, 0, rigid.linearVelocity.z);
        // 점프력만큼 위쪽으로 힘을가해주어야함.
        rigid.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
    }
}