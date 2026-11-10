using UnityEngine;

public class EnemyMoovement : MonoBehaviour
{
    [SerializeField] private Enemy enemyStats;

    [SerializeField] private float speed;
    private float curSpeed;
    enum State
    {
        Idle,
        Chase,
        Attack,
        Pain
    }

    private State state = State.Idle;

    private void Update()
    {
        if (state == State.Chase)
            curSpeed = speed;
        else
            curSpeed = 0;

        switch (state)
        {
            case State.Idle:
                
                break;
            case State.Chase:
                
                break;
            case State.Attack:

                break;
        }
    }
}
