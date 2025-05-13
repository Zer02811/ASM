using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    public Transform player;

    public float patrolRadius = 10f;
    public Transform patrolCenter;

    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float loseChaseRange = 15f;

    public float fadeAudioRange = 20f; // Phạm vi bắt đầu giảm âm lượng (lớn hơn loseChaseRange)
    public float audioFadeOutSpeed = 1f; // Tốc độ giảm âm lượng (điều chỉnh trong Inspector)
    private float maxChaseVolume = 1f; // Âm lượng tối đa khi đuổi theo

    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    public float attackCooldown = 2f;
    private float lastAttackTime;

    // Sát thương gây ra
    public float attackDamage = 20f; // Điều chỉnh lượng sát thương trong Inspector

    // Animation Controller
    public EnemyAnimationController animationController;

    // Patrol Pause
    public float patrolWaitTime = 2f;
    private float patrolWaitTimer = 0f;
    private bool isWaitingPatrol = false;

    // Chase Audio
    public AudioClip chaseAudioClip;
    private AudioSource audioSource;
    private bool hasPlayedChaseAudio = false;

    public enum AIState
    {
        Patrol,
        WaitPatrol,
        Chase,
        Attack
    }
    public AIState currentState = AIState.Patrol;

    private Vector3 currentPatrolTarget;

    private float currentChaseVolume; // Lưu trữ âm lượng hiện tại của chase audio

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            enabled = false;
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            enabled = false;
            return;
        }

        animationController = GetComponent<EnemyAnimationController>();
        if (animationController == null)
        {
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        navMeshAgent.speed = patrolSpeed;
        lastAttackTime = Time.time - attackCooldown;

        if (patrolCenter == null)
        {
            patrolCenter = transform;
        }

        SetRandomPatrolPoint();
        UpdateAnimation();
        currentChaseVolume = 0f; // Khởi tạo âm lượng chase về 0
        audioSource.volume = 0f; // Đặt âm lượng AudioSource ban đầu về 0
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                PatrolState();
                break;
            case AIState.WaitPatrol:
                WaitPatrolState();
                break;
            case AIState.Chase:
                ChaseState();
                break;
            case AIState.Attack:
                AttackState();
                break;
        }
        UpdateAnimation();
    }

    void PatrolState()
    {
        navMeshAgent.speed = patrolSpeed;

        // Kiểm tra nếu đã đến điểm tuần tra hiện tại hoặc chưa có điểm tuần tra
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f || navMeshAgent.destination != currentPatrolTarget)
        {
            currentState = AIState.WaitPatrol;
            isWaitingPatrol = true;
            patrolWaitTimer = 0f;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRange)
        {
            currentState = AIState.Chase;
            hasPlayedChaseAudio = false;
        }
        else if (distanceToPlayer <= fadeAudioRange) // Nếu trong fadeAudioRange nhưng không chase nữa
        {
            FadeOutChaseAudio(); // Giảm âm lượng chase audio
        }
        else
        {
            StopChaseAudio(); // Dừng hoàn toàn chase audio khi ra khỏi fadeAudioRange
        }
    }

    void WaitPatrolState()
    {
        navMeshAgent.speed = 0f;
        navMeshAgent.velocity = Vector3.zero;

        patrolWaitTimer += Time.deltaTime;

        if (patrolWaitTimer >= patrolWaitTime)
        {
            currentState = AIState.Patrol;
            SetRandomPatrolPoint();
            isWaitingPatrol = false;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRange)
        {
            currentState = AIState.Chase;
            hasPlayedChaseAudio = false;
        }
        else if (distanceToPlayer <= fadeAudioRange) // Nếu trong fadeAudioRange nhưng không chase nữa
        {
            FadeOutChaseAudio(); // Giảm âm lượng chase audio
        }
        else
        {
            StopChaseAudio(); // Dừng hoàn toàn chase audio khi ra khỏi fadeAudioRange
        }
    }

    void ChaseState()
    {
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(player.position);
        // Đặt âm lượng tối đa khi vào Chase
        audioSource.volume = maxChaseVolume;

        if (!hasPlayedChaseAudio && audioSource != null && chaseAudioClip != null)
        {
            audioSource.clip = chaseAudioClip; // Gán clip trước khi Play
            audioSource.loop = true; // Lặp lại nhạc đuổi theo
            audioSource.Play();
            hasPlayedChaseAudio = true;
        }

        if (Vector3.Distance(transform.position, player.position) < attackRange)
        {
            currentState = AIState.Attack;
        }
        else if (Vector3.Distance(transform.position, player.position) > loseChaseRange)
        {
            currentState = AIState.Patrol;
            SetRandomPatrolPoint();
        }
    }

    void AttackState()
    {
        navMeshAgent.speed = 0f;
        navMeshAgent.velocity = Vector3.zero;

        transform.LookAt(player);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }

        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            currentState = AIState.Chase;
        }
    }

    void SetRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += patrolCenter.position;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, patrolRadius, NavMesh.AllAreas);

        if (navHit.hit)
        {
            currentPatrolTarget = navHit.position;
            navMeshAgent.SetDestination(currentPatrolTarget);
        }
        else
        {
            SetRandomPatrolPoint();
        }
    }

    void PerformAttack()
    {
        // *** NƠI THỰC HIỆN HÀNH ĐỘNG TẤN CÔNG ***
        animationController.TriggerAttackAnimation();

        // Tìm component Health trên Player và gây sát thương
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage); // Gọi TakeDamage trên Health script của Player
        }
    }

    void UpdateAnimation()
    {
        bool isMoving = navMeshAgent.velocity.magnitude > 0.1f && currentState != AIState.Attack && currentState != AIState.WaitPatrol;

        if (currentState == AIState.Patrol || currentState == AIState.WaitPatrol)
        {
            animationController.PlayWalkAnimation(isMoving);
        }
        else if (currentState == AIState.Chase)
        {
            animationController.PlayRunAnimation(isMoving);
        }
        else if (currentState == AIState.Attack)
        {
            animationController.ResetAllAnimations();
        }
    }
    void FadeOutChaseAudio()
    {
        if (audioSource.isPlaying)
        {
            currentChaseVolume = Mathf.MoveTowards(audioSource.volume, 0f, audioFadeOutSpeed * Time.deltaTime);
            audioSource.volume = currentChaseVolume;
            if (currentChaseVolume <= 0f)
            {
                StopChaseAudio(); // Dừng hẳn khi âm lượng về 0
            }
        }
    }

    void StopChaseAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.volume = 0f; // Đảm bảo volume về 0 khi dừng
        }
    }
}