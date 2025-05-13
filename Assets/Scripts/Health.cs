using  UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float deathDelay = 5f;

    public Animator animator;
    public string deathAnimationName = "Die";

    private bool isDead = false;
    public bool IsDead { get { return isDead; } set { isDead = value; } } //Public property
    private GameManager gameManager; // Tham chiếu đến GameManager
    private EnemySpawner spawner;

    void Start()
    {
        currentHealth = maxHealth;
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("Không tìm thấy Animator component!");
            }
        }
        // Tìm GameManager
        gameManager = Object.FindFirstObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("Không tìm thấy GameManager trong scene!");
            enabled = false; // Vô hiệu hóa Health nếu không tìm thấy GameManager
            return;
        }

        // Find the EnemySpawner in the scene
        spawner = Object.FindFirstObjectByType<EnemySpawner>();
        if (spawner == null)
        {
            Debug.LogError("Không tìm thấy EnemySpawner trong scene!");
            // Optionally disable this script if spawner is required
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log(transform.name + " nhận sát thương! HP còn lại: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(transform.name + " đã chết!");

        // Gọi hàm EnemyDefeated trong GameManager (chỉ khi là Enemy)
        if (GetComponent<EnemyAI>() != null && gameManager != null) // Kiểm tra EnemyAI và GameManager
        {
            gameManager.EnemyDefeated();
        }
        else if (GetComponent<PlayerMovement>() != null && gameManager != null) // Kiểm tra PlayerMovement (là Player)
        {
            gameManager.PlayerDied();    // Gọi PlayerDied cho Player
        }

        // Play animation chết
        if (animator != null && !string.IsNullOrEmpty(deathAnimationName))
        {
            animator.Play(deathAnimationName);
        }

        StartCoroutine(DeathCoroutine());
    }

    IEnumerator DeathCoroutine()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        yield return new WaitForSeconds(deathDelay);

        // Return enemy to the pool if possible.

        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null && spawner != null)
        {
            spawner.ReturnEnemyToPool(enemyAI);  // Return to Pool
        }
        else
        {
            Destroy(gameObject);  // Fallback if no pooling
        }

        //Destroy(gameObject); //Old line - destroy.
    }
}