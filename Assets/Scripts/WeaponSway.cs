using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private PlayerMoovement player;
    [SerializeField] private PlayerGun gun;

    [Header("Sway")]
    [SerializeField] private float amplitudeX = 0.08f;     // ширина качания
    [SerializeField] private float amplitudeY = 0.04f;     // глубина дуги
    [SerializeField] private float cyclesPerMeter = 0.35f; // полных циклов влево-вправо на метр пути
    [SerializeField] private float maxSpeedForSway = 6f;   // = maxSpeed игрока
    [SerializeField] private float followSpeed = 14f;      // как быстро оружие догоняет цель

    private Vector3 startLocalPos;
    private float phase;

    private void Start()
    {
        startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        float speed = player.HorizontalSpeed;
        bool swaying = speed > 0.1f && player.IsGrounded && !gun.IsFiring;

        Vector3 offset = Vector3.zero;

        if (swaying)
        {
            phase += speed * cyclesPerMeter * Mathf.PI * 2f * Time.deltaTime;

            float k = Mathf.Clamp01(speed / maxSpeedForSway);
            offset.x = Mathf.Cos(phase) * amplitudeX * k;
            offset.y = -Mathf.Abs(Mathf.Sin(phase)) * amplitudeY * k;
        }
        else
        {
            // следующий старт начнётся из центра
            phase = Mathf.PI * 0.5f;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            startLocalPos + offset,
            followSpeed * Time.deltaTime);
    }
}
