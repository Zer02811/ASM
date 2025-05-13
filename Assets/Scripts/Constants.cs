using UnityEngine;

// Lớp chứa các hằng số dùng chung trong dự án
public static class SceneNames
{
    public const string MainMenu = "MainMenu";
    public const string GameScene = "ASM";
    // Thêm tên các scene khác nếu có
}

public static class Tags
{
    public const string Player = "Player";
    public const string Enemy = "Enemy";
    // Thêm các tag khác nếu cần
}

public static class AnimationParameters
{
    // Player Movement Hashes
    public static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    public static readonly int Horizontal = Animator.StringToHash("Horizontal");
    public static readonly int Vertical = Animator.StringToHash("Vertical");
    public static readonly int IsJumping = Animator.StringToHash("IsJumping"); // Hoặc tên trigger Jump của bạn

    // Health/Death Hashes
    public static readonly int IsDead = Animator.StringToHash("IsDead"); // Ví dụ nếu dùng bool
    public static readonly int DieTrigger = Animator.StringToHash("DieTrigger"); // Ví dụ nếu dùng trigger

    // Enemy AI Hashes
    public static readonly int IsWalking = Animator.StringToHash("isWalking");
    public static readonly int IsRunning = Animator.StringToHash("isRunning");
    public static readonly int IsAttacking = Animator.StringToHash("isAttacking"); // Trigger
}