using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private CapsuleCollider2D coll;
    private Rigidbody2D rb;
    private PlayerController playerController;
    [Header("Check Parameters")] 
    public bool manual;
    
    public bool isPlayer;
    
    public Vector2 bottomOffset;
    public Vector2 leftOffset;
    public Vector2 rightOffset;
    
    public float checkRadius;
    public LayerMask groundLayer;
    [Header("Status")]
    public bool isGround;

    public bool touchLeftWall;
    public bool touchRightWall;

    public bool onWall;

    private void Awake()
    {
        coll=GetComponent<CapsuleCollider2D>();
        rb=GetComponent<Rigidbody2D>();

        if (!manual)
        {
            rightOffset = new Vector2((coll.bounds.size.x + coll.offset.x) / 2, coll.bounds.size.y / 2);
            leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
        }

        if (isPlayer)
        {
            playerController = GetComponent<PlayerController>();
        }
        
    }

    private void Update()
    {
        Check();
    }

    public void Check()
    {
        //地面判断
isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(bottomOffset.x * transform.localScale.x, bottomOffset.y), checkRadius, groundLayer);        
        //壁判断
        touchLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(leftOffset.x, leftOffset.y), checkRadius, groundLayer);
        touchRightWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(rightOffset.x, rightOffset.y), checkRadius, groundLayer);
        
        //壁につく
        if (isPlayer)
        {
            onWall = ((touchLeftWall && playerController.inputDirection.x < 0) ||
                      (touchRightWall && playerController.inputDirection.x > 0) ) && !isGround && rb.velocity.y < 0;
        }

    }

    //偏移量を描画
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(bottomOffset.x * transform.localScale.x, bottomOffset.y), checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(leftOffset.x, leftOffset.y), checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(rightOffset.x, rightOffset.y), checkRadius);
    }
}
