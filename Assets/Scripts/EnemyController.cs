using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Rigidbody2D rb;
    private float horizontal = 1f;
    private bool isFacingRight = true;
    
    private Vector2 vecGravity;

    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpPower = 26f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform wallCheckForJump;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private PlayerController player;
    
    private Vector2 knockbackPower = new Vector2(12f, 10f);

    public bool shouldBeKnockedBack = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsOnWall(wallCheck) )
        {
            // if (!IsOnWall(wallCheckForJump) )
            // {
            //     rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            //     Debug.Log("Prig");
            // }
            // else
            {
                horizontal = -horizontal;
                rb.velocity = new Vector2(0, 0);
            }
        }

        if (rb.velocity.y > .2f)
        {
            rb.velocity = new Vector2(horizontal * speed / 2, rb.velocity.y);
        }
        else if (!shouldBeKnockedBack)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
        
        if (rb.velocity.y < 0)
        {
            rb.velocity -= vecGravity * fallMultiplier * Time.deltaTime;
        }

        Flip();
    }

    private void FixedUpdate()
    {
    }

    public IEnumerator StopKnockbacking()
    {
        shouldBeKnockedBack = true;
        yield return new WaitForSeconds(.2f);
        shouldBeKnockedBack = false;
    }
    
    private bool IsOnWall(Transform target)
    {
        return Physics2D.OverlapCircle(target.position, .2f, wallLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}