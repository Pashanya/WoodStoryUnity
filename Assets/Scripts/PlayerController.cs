using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Animator anim;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    private bool isJumping;
    private bool doubleJump;
    private bool isJumpingFalling;
    private float coyoteTime = .2f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = .1f;
    private float jumpBufferCounter;

    private bool isKnockbacking;
    private Vector2 knockbackDirection;
    public float knockbackDuration = .4f;
    private Vector2 knockbackPower = new Vector2(12f, 10f);
    private float damageDuration = .5f;
    private bool canTakeDamage = true;

    private Vector2 vecGravity;

    private bool onWall;
    private bool underWall;
    private bool onWallUp;
    private bool onWallDown;
    [SerializeField] private float wallCheckRayDistance = 1f;
    private float wallCheckRadiusDown;
    [SerializeField] private bool canLedge = true;
    [SerializeField] private bool onLedge;
    public float ledgeRayCorrectY = 0.5f;
    public float offsetY;

    private float gravityDef;
    private float horizontal;
    private int wallDirection;
    private bool isFacingRight = true;
    [SerializeField] public int health = 10;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float jumpPower = 30f;
    [SerializeField] private float fallMultiplier = 2f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheckUp;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private TextMeshProUGUI text;

    private enum MovementState
    {
        idle,
        run,
        jump,
        fall,
        grab
    }

    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        gravityDef = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Health: " + health.ToString();
        horizontal = Input.GetAxisRaw("Horizontal");

        if (isDashing)
        {
            return;
        }

        if (IsOnGround())
        {
            doubleJump = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        if (rb.velocity.y < 0 && rb.velocity.y > -40)
        {
            rb.velocity -= vecGravity * fallMultiplier * Time.deltaTime;
            // Debug.Log(rb.velocity);
        }

        // wallDirection = (int)Mathf.Sign(horizontal);

        HandleJump();
        Flip();
        UpdateAnimationState();
    }


    private void FixedUpdate()
    {
        CheckingLedge();

        if (isDashing)
        {
            return;
        }

        if (!isKnockbacking && !onLedge)
        {
            rb.gravityScale = gravityDef;
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
    }

    void CheckingLedge()
    {
        if (!IsOnGround() && canLedge)
        {
            onWallUp = Physics2D.Raycast(wallCheckUp.position,
                new Vector2(transform.localScale.x, 0),
                wallCheckRayDistance,
                wallLayer);

            underWall = Physics2D.Raycast(
                wallCheckUp.position,
                Vector2.up,
                ledgeRayCorrectY,
                wallLayer
            );

            if (onWallUp && !underWall)
            {
                onLedge = !Physics2D.Raycast(
                    new Vector2(wallCheckUp.position.x, wallCheckUp.position.y + ledgeRayCorrectY),
                    new Vector2(transform.localScale.x, 0),
                    wallCheckRayDistance,
                    wallLayer);
            }
            else
            {
                onLedge = false;
            }

            if (onLedge && Input.GetAxis("Vertical") != -1)
            {
                doubleJump = true;
                rb.gravityScale = 0;
                rb.velocity = new Vector2(0, 0);
                offsetCalculateAndCorrect();
            }
        }  else
        {
            onLedge = false;
        }
    }

    private float minCorrectDistance = .01f;

    void offsetCalculateAndCorrect()
    {
        
        offsetY = Physics2D.Raycast(
            new Vector2(wallCheckUp.position.x + wallCheckRayDistance * transform.localScale.x,
                wallCheckUp.position.y + ledgeRayCorrectY),
            Vector2.down,
            ledgeRayCorrectY,
            wallLayer
        ).distance;

        if (offsetY > minCorrectDistance * 1.5f)
            transform.position = new Vector3(transform.position.x, transform.position.y - offsetY + minCorrectDistance,
                transform.position.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallCheckUp.position,
            new Vector2(wallCheckUp.position.x + wallCheckRayDistance * transform.localScale.x,
                wallCheckUp.position.y));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector2(wallCheckUp.position.x, wallCheckUp.position.y + ledgeRayCorrectY),
            new Vector2(wallCheckUp.position.x + wallCheckRayDistance * transform.localScale.x,
                wallCheckUp.position.y + ledgeRayCorrectY));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector2(wallCheckUp.position.x + wallCheckRayDistance * transform.localScale.x,
                wallCheckUp.position.y + ledgeRayCorrectY),
            new Vector2(wallCheckUp.position.x + wallCheckRayDistance * transform.localScale.x,
                wallCheckUp.position.y));

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            wallCheckUp.position,
            new Vector2(wallCheckUp.position.x, wallCheckUp.position.y + ledgeRayCorrectY)
        );
    }


    void HandleJump()
    {
        if (IsOnGround())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
            if (onLedge)
            {
                onLedge = false;
                rb.gravityScale = gravityDef;
            }
            StartCoroutine(LedgeCooldown());
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || doubleJump) && !isJumping)
        {
            if (coyoteTimeCounter < 0f)
                doubleJump = false;

            rb.velocity = new Vector2(rb.velocity.x, jumpPower);

            jumpBufferCounter = 0f;

            StartCoroutine(JumpCooldown());
        }
    }

    private bool IsOnGround()
    {
        
        RaycastHit2D hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .2f, groundLayer);

        
        if (hit.collider != null)
        {
            Debug.DrawLine(coll.bounds.center, hit.point, Color.red);
        }
        else
        {
            Debug.DrawRay(coll.bounds.center, Vector2.down * 0.2f, Color.green);
        }

        return hit;

        // return Physics2D.OverlapCircle(groundCheck.position, .2f, groundLayer);
    }

    private bool IsOnWall()
    {
        return false;
    }

    private void Flip()
    {
        if (!onLedge && (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator LedgeCooldown()
    {
        canLedge = false;
        yield return new WaitForSeconds(0.2f);
        canLedge = true;
    }

    private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(horizontal * dashingPower, 0f);
        trail.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        trail.emitting = false;
        rb.gravityScale = gravityDef;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void UpdateAnimationState()
    {
        MovementState state;
        if (horizontal != 0f)
        {
            if (IsOnGround()) state = MovementState.run;
            else
                state = MovementState.fall;
        }
        else
        {
            state = MovementState.idle;
            anim.SetInteger("state", 0);
        }

        if (rb.velocity.y > .1f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -.1f && !IsOnGround())
        {
            state = MovementState.fall;
        }

        anim.SetInteger("state", (int)state);
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageDuration);
        canTakeDamage = true;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && canTakeDamage)
        {
            StartCoroutine(DamageCooldown());
            health--;

            isKnockbacking = true;
            knockbackDirection = (transform.position - other.transform.position).normalized;
            rb.velocity = new Vector2(knockbackDirection.x * knockbackPower.x, knockbackPower.y);

            if (knockbackDirection.x < 1 && knockbackDirection.x > 0) knockbackDirection.x = 1;
            if (knockbackDirection.x > -1 && knockbackDirection.x < 0) knockbackDirection.x = -1;

            EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();

            if (enemyController != null)
            {
                Vector2 enemyKnockbackDirection = -knockbackDirection;
                enemyController.rb.velocity = new Vector2(Mathf.Sign(enemyKnockbackDirection.x) * knockbackPower.x, 0);
                enemyController.shouldBeKnockedBack = true;
                StartCoroutine(enemyController.StopKnockbacking());
            }

            if (transform.localScale.x != knockbackDirection.x)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopKnockbacking), knockbackDuration);
        }
    }

    private void StopKnockbacking()
    {
        isKnockbacking = false;
    }
}