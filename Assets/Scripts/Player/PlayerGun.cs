using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerGun : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float fireHoldTime = 0.25f;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite ShootSprite;
    [SerializeField] private float shootSpriteTime = 0.2f;

    private Sprite defaultSprite;
    private float lastShotTime = -100f;
    private Coroutine shootCoroutine;

    public bool IsFiring => Time.time < lastShotTime + fireHoldTime;

    private void Start()
    {
        defaultSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            lastShotTime = Time.time;

            if (shootCoroutine != null)
                StopCoroutine(shootCoroutine);

            shootCoroutine = StartCoroutine(ShowShootSprite());

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                if (hit.collider.TryGetComponent(out Enemy enemy))
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    private IEnumerator ShowShootSprite()
    {
        spriteRenderer.sprite = ShootSprite;

        yield return new WaitForSeconds(shootSpriteTime);

        spriteRenderer.sprite = defaultSprite;
        shootCoroutine = null;
    }
}