# LAB – One-Button Dash (Alpha)

Simple one-button runner built in **Unity 2023.1**, using **2D URP**.

**Alpha Goal:**  
Player auto-runs to the right, jumps with Space, can land on platforms, dies on spikes/side of pillars, reaches a goal that shows “Level Completed!”, with basic music, SFX, and a following camera.

---

## Part 0 – Prototype (Concept First)

> **Prototype = “Make it move and jump.”**  
> One scene, one script, gray boxes, no UI or polish.

For the prototype you’re aiming for:

- One 2D URP project
- A square “Player” that:
  - Auto-moves to the right
  - Jumps when you press **Space**
- A simple flat ground to test on

Once that’s working, you roll into **Alpha**, where it becomes a real level.

---

## Part I – Alpha

### Step 1 – Project Creation

1. Create a new Unity project named **`One-Button Dash`**.
2. Use **Unity 2023.1 (or 2023.x)**.
3. Choose the **2D (URP / Universal 2D)** template.

---

### Step 2 – Player & Scene Setup

1. Rename the default scene to **`Scene_Alpha`** and move it into `Assets/Scenes/`.
2. In the **Hierarchy**, create:
   - `2D Object → Sprite → Square`, rename it to **`Player`**.
3. With `Player` selected, add:
   - **Rigidbody2D**
   - **BoxCollider2D**
4. Set the **Tag** of `Player` to `Player`.

---

### Step 3 – Project Structure, Layers & Tags

#### 3.1 – Folder structure

In the **Project** window, create:

```text
Assets/
├─ Scenes/
├─ Scripts/
├─ Sprites/
├─ Audio/
├─ Prefabs/
└─ UI/
```

Move `Scene_Alpha` into `Assets/Scenes/`.

#### 3.2 – Layers & Tags

- In **Layers**, create a layer: `Ground`.
- In **Tags**, ensure you have:
  - `Player`
  - `Ground`
  - `Pillar`
  - `Obstacle`
  - `Goal`

These will drive behaviour in code (what’s walkable, what kills you, what wins the level).

---

### Step 4 – Player Script (Movement & Jump)

#### 4.1 – Create the script

1. In `Assets/Scripts/`, create **`OneButtonDash.cs`**.
2. Attach `OneButtonDash` to the **Player** object.

#### 4.2 – Paste the Alpha script

Use this version for Alpha:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneButtonDash : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Ground Check")]
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
    [SerializeField] private GameObject levelCompletePanel;

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

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        CheckGround();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead || isCompleted) return;

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

    private void HandleJumpInput()
    {
        if (isDead || isCompleted) return;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log("Jumped!");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            sfx?.PlaySFX(dashSource, 0.95f, 1.05f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isCompleted) return;

        GameObject other = collision.gameObject;

        // Flat ground
        if (other.CompareTag("Ground"))
        {
            if (!isGrounded)
            {
                sfx?.PlaySFX(landSource, 0.98f, 1.02f);
            }
        }
        // Pillars: top safe, sides lethal
        else if (other.CompareTag("Pillar"))
        {
            ContactPoint2D contact = collision.GetContact(0);

            // Hit from above
            if (contact.normal.y > 0.5f)
            {
                if (!isGrounded)
                {
                    sfx?.PlaySFX(landSource, 0.98f, 1.02f);
                }
            }
            else
            {
                Die();
            }
        }
        // Spikes / hazards
        else if (other.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isCompleted) return;

        if (other.CompareTag("Goal"))
        {
            CompleteLevel();
        }
    }

    private void Die()
    {
        if (isDead || isCompleted) return;

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
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void CompleteLevel()
    {
        isCompleted = true;
        rb.velocity = Vector2.zero;

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        Debug.Log("Level Completed!");
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
```

You can reference a separate **“Code Walkthrough”** section instead of explaining this inline.

---

### Step 5 – Ground & GroundCheck

1. Create a `Ground` object:
   - `2D Object → Sprite → Square`, stretch it into a long platform.
   - Add **BoxCollider2D**.
   - Set Tag = `Ground`.
   - Set Layer = `Ground`.
2. Create a `GroundCheck` child:
   - Right-click `Player` → `Create Empty`, rename to `GroundCheck`.
   - Move it slightly below the player’s feet.
3. On `Player`’s `OneButtonDash`:
   - Drag `GroundCheck` into `groundCheck`.
   - Set `groundCheckRadius` ≈ `0.1`.
   - Set `groundMask` to include the `Ground` layer.

The white gizmo circle (OnDrawGizmosSelected) will show the ground check area in the Scene view.

---

### Step 6 – Hazards & Pillars

**Spikes (Obstacles)**

1. Create a `Spike` object:
   - Sprite (triangle or placeholder).
   - Add a **Collider2D** (Box/Polygon).
   - Tag = `Obstacle`.
2. Drag `Spike` into `Assets/Prefabs/` to make a prefab.
3. Place a few spikes in `Scene_Alpha`.

**Pillars**

1. Create a `Pillar` object:
   - Sprite → Square, scale into a vertical column.
   - Add **BoxCollider2D**.
   - Tag = `Pillar`.
   - Layer = `Ground` (so the top counts as ground).
2. Drag `Pillar` into `Assets/Prefabs/`.
3. Place pillars at different positions so the player must jump onto them.

Script behaviour:

- Ground and pillar tops → safe, may play landing SFX.
- Pillar sides (`Pillar` tag with non-top contact) and `Obstacle` → call `Die()`.

---

### Step 7 – Audio & SFX

**Music**

1. Create an Empty GameObject named `Music`.
2. Add **AudioSource** to `Music`.
3. Drag your background music clip into the AudioSource’s `AudioClip` field.
4. Uncheck `Play On Awake` (the script starts it).

**SFX on Player**

1. Select `Player`.
2. Add **AudioSource** (for SFX).
3. Disable `Play On Awake`.

**SFXPlayer Helper**

Create `SFXPlayer.cs` in `Assets/Scripts/`:

```csharp
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (clip == null || source == null) return;

        float randomPitch = Random.Range(pitchMin, pitchMax);
        source.pitch = randomPitch;
        source.PlayOneShot(clip);
    }
}
```

Attach `SFXPlayer` to `Player` (same object that has the SFX AudioSource).

**Hook things up in `OneButtonDash` (Inspector)**

On the `Player`’s `OneButtonDash` component:

- `musicSource` → drag the AudioSource from the `Music` object.
- `dashSource` → jump/dash SFX.
- `landSource` → landing SFX.
- `deathSource` → death SFX.

---

### Step 8 – Goal & “Level Completed!” UI

**Goal Trigger**

1. Create a `Goal` object:
   - Add a sprite (flag/portal/box).
   - Add **BoxCollider2D**.
   - Enable **Is Trigger**.
   - Tag = `Goal`.
2. Place it at the far right of the level.

**UI Canvas**

1. `GameObject → UI → Canvas`.
2. Inside Canvas, create a **Panel**, rename it `LevelCompletePanel`.
   - Stretch/position it where you want.
3. Inside `LevelCompletePanel`, create a **TextMeshPro – Text (UI)**:
   - Set text to `Level Completed!`
   - Center it and bump the font size.
4. Disable `LevelCompletePanel` in the Hierarchy so it’s hidden at start.

**Connect UI to Script**

1. Select `Player`.
2. In `OneButtonDash`, assign `levelCompletePanel` to the `LevelCompletePanel` object.

Now, when the player enters the Goal trigger, `CompleteLevel()` will:

- Stop movement
- Stop music
- Show the `LevelCompletePanel`

---

### Step 9 – Camera Follow (X Only)

**Camera script**

Create `CameraFollow2D.cs` in `Assets/Scripts/` and attach it to `Main Camera`:

```csharp
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float offsetX = 2f;
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private float fixedY;
    private float fixedZ;

    private void Awake()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = new Vector3(
            target.position.x + offsetX,
            fixedY,
            fixedZ
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            smoothTime
        );
    }
}
```

Assign `target` to the `Player` transform.

On `Player`’s **Rigidbody2D**, set:

- **Interpolation** = `Interpolate`

This keeps the camera motion smooth and avoids jitter while only following along the X-axis.
