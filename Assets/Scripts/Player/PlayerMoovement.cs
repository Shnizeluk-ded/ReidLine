using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoovement : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float accelTime = 0.5f;   
    [SerializeField] private float decelTime = 0.5f;   
    [SerializeField] private float turnSpeed;
    [SerializeField] private float gravity = -20f;

    [SerializeField] private CharacterController cc;

    Vector3 velocity;
    Vector3 moveDir;   
    float t;           
    float vy;

    public float HorizontalSpeed => new Vector3(velocity.x, 0f, velocity.z).magnitude;
    public bool IsGrounded => cc.isGrounded;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        transform.Rotate(0f, mouse.delta.ReadValue().x * turnSpeed, 0f);

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        Vector3 input = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1f);

        if (input.sqrMagnitude > 0.0001f)
        {
            moveDir = input.normalized;
            t = Mathf.MoveTowards(t, 1f, Time.deltaTime / accelTime);
        }
        else
        {
            t = Mathf.MoveTowards(t, 0f, Time.deltaTime / decelTime);
        }

        // квадратичная зависимость: скорость ~ t^2
        velocity = moveDir * (maxSpeed * t * t);

        vy = cc.isGrounded ? -2f : vy + gravity * Time.deltaTime;

        cc.Move((velocity + Vector3.up * vy) * Time.deltaTime);
    }
}

