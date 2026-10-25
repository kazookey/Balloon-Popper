using Unity.Netcode;
using UnityEngine;

public class PlayerController2D : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 8f;
    private Rigidbody2D rb;
    private float baseMoveSpeed; 
    private Coroutine speedRoutine;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        baseMoveSpeed = moveSpeed; 
    }

    private void Update()
    {
       
        if (!IsOwner) return;

       
        if (GameManager.Instance != null && !GameManager.Instance.isMatchActive.Value) return;

       
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

           
            if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        if (speedRoutine != null) StopCoroutine(speedRoutine);
        speedRoutine = StartCoroutine(SpeedRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator SpeedRoutine(float multiplier, float duration)
    {
        moveSpeed = baseMoveSpeed * multiplier; 
        yield return new WaitForSeconds(duration);
        
        moveSpeed = baseMoveSpeed; 
        speedRoutine = null;
    }
}