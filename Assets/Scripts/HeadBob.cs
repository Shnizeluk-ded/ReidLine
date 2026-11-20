using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [SerializeField] private PlayerMoovement player;

    [Header("Bob")]
    [SerializeField] private float bobAmplitude = 0.06f;  
    [SerializeField] private float stepsPerMeter = 0.7f;   
    [SerializeField] private float returnSpeed = 8f;       
    [SerializeField] private float maxSpeedForBob = 6f;   

    private Vector3 startLocalPos;
    private float phase;

    private void Start()
    {
        startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        float speed = player.HorizontalSpeed;
        bool moving = speed > 0.1f && player.IsGrounded;

        if (moving)
        {
            phase += speed * stepsPerMeter * Mathf.PI * 2f * Time.deltaTime;

            float speedFactor = Mathf.Clamp01(speed / maxSpeedForBob);
            float offsetY = Mathf.Sin(phase) * bobAmplitude * speedFactor;

            Vector3 target = startLocalPos + new Vector3(0f, offsetY, 0f);
            transform.localPosition = target;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, startLocalPos, returnSpeed * Time.deltaTime);

            phase = Mathf.Lerp(phase, Mathf.Round(phase / Mathf.PI) * Mathf.PI, returnSpeed * Time.deltaTime);
        }
    }
}
