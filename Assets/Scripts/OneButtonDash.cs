using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneButtonDash : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundMask;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    [Header("SFX")]
    [SerializeField] private AudioClip dashSource;
    [SerializeField] private AudioClip deathSource;
    [SerializeField] private AudioClip landSource;

    [Header("UI")]
    [SerializeField] private GameObject levelCompletedPanel;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isDead;
    private bool isCompleted;
    private SFXPlayer sfx;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sfx = GetComponent<SFXPlayer>();

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }

        if (levelCompletedPanel != null)
        {
            levelCompletedPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDead || isCompleted) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        CheckGround();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }

    private void CheckGround()
    {
        if (groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundMask
            );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isCompleted) return;

        GameObject other = collision.gameObject;

        if (other.CompareTag("Ground"))
        {
            if (isGrounded)
            {
                sfx?.PlaySFX(landSource, 0.98f, 1.02f);
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            Die();
        }
        else if (other.CompareTag("Goal"))
        {
            CompleteLevel();
        }
    }

    private void TogglePause()
    {
        if (Time.timeScale > 0f)
        {
            Time.timeScale = 0f;
            // TODO: show pause menu
        }
        else
        {
            Time.timeScale = 1f;
            // TODO: hide pause menu
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            //Debug.Log("Jumped!");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            sfx?.PlaySFX(dashSource, 0.95f, 1.05f);
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.velocity = Vector2.zero;

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        sfx?.PlaySFX(deathSource, 0.95f, 1.05f);

        Invoke(nameof(RestartLevel), 2.5f);
    }

    private void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void CompleteLevel()
    {
        isCompleted = true;
        rb.velocity = Vector2.zero;

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        if (levelCompletedPanel != null)
        {
            levelCompletedPanel.SetActive(true);
        }
    }

    private void OnGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
