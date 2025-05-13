using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private readonly int isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int isRunningHash = Animator.StringToHash("isRunning");
    private readonly int isAttackingHash = Animator.StringToHash("isAttacking");

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on " + gameObject.name + " or its children.");
        }
    }

    public void PlayWalkAnimation(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool(isWalkingHash, isWalking);
            animator.SetBool(isRunningHash, false); // ??m b?o t?t animation run
            //animator.ResetTrigger(isAttackingHash); // ??m b?o reset trigger attack
        }
    }

    public void PlayRunAnimation(bool isRunning)
    {
        if (animator != null)
        {
            animator.SetBool(isRunningHash, isRunning);
            animator.SetBool(isWalkingHash, false); // ??m b?o t?t animation walk
            //animator.ResetTrigger(isAttackingHash); // ??m b?o reset trigger attack
        }
    }

    public void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            Debug.Log(gameObject.name + ": Triggering Attack Animation");
            animator.SetTrigger(isAttackingHash);
            animator.SetBool(isWalkingHash, false); // ??m b?o t?t animation walk
            animator.SetBool(isRunningHash, false); // ??m b?o t?t animation run
        }
        else
        {
            Debug.LogError(gameObject.name + ": Animator component is null in TriggerAttackAnimation!"); // Thêm dòng này ?? debug null animator
        }
    }

    public void ResetAllAnimations() // Hàm tùy ch?n ?? reset t?t c? animation v? tr?ng thái idle
    {
        if (animator != null)
        {
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
            animator.SetTrigger(isAttackingHash);
        }
    }
}