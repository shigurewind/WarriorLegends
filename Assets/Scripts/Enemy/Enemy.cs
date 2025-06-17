using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D),typeof(Animator),typeof(PhysicsCheck))]

public class Enemy : MonoBehaviour
{
    //子クラスだけ訪問できる
    public Rigidbody2D rb;
    [HideInInspector]public Animator anim;
    [HideInInspector]public PhysicsCheck physicsCheck;
    
    [Header("Basic Parameters")]
    public float normalSpeed;
    public float chaseSpeed;
    [HideInInspector]public float currentSpeed;

    public Vector3 faceDir;
    public float hurtForce;

    public Transform attacker;
    
    public Vector3 spawnPoint;


    [Header("Check")] 
    public Vector2 centerOffset;
    public Vector2 checkSize;
    public float checkDistance;
    public LayerMask attackLayer;
    
    [Header("Timer")]
    public float waitTime;
    public float waitTimeCounter;
    public bool wait;
    public float lostTime;
    public float lostTimeCounter;

    [Header("Status")] 
    public bool isHurt;
    public bool isDead;

    private BaseState currentState;
    protected BaseState patrolState;
    protected BaseState chaseState;
    protected BaseState skillState;
    

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        physicsCheck = GetComponent<PhysicsCheck>();
        
        currentSpeed = normalSpeed;
        waitTimeCounter = waitTime;

        spawnPoint = transform.position;
    }

    private void OnEnable()
    {
        currentState = patrolState;
        currentState.OnEnter(this);
    }

    private void Update()
    {
        faceDir = new Vector3(-transform.localScale.x, 0, 0);
        
        currentState.LogicUpdate();
        TimeCounter();
    }

    private void FixedUpdate()
    {
        if (!isHurt && !isDead && !wait)
        {
            Move();
        }
        
        currentState.PhysicsUpdate();
        
    }

    private void OnDisable()
    {
        currentState.OnExit();
    }

    public virtual void Move()//virtual:固定ではない
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("PreMove") 
            &&!anim.GetCurrentAnimatorStateInfo(0).IsName("snailRecover") )//Snail専用判断
        {
            rb.velocity = new Vector2(currentSpeed*faceDir.x*Time.deltaTime, rb.velocity.y);
        }
        
    }

    /// <summary>
    /// Timer
    /// </summary>
    public void TimeCounter()
    {
        if (wait)
        {
            waitTimeCounter -= Time.deltaTime;
            if (waitTimeCounter<=0 )
            {
                wait = false;
                waitTimeCounter = waitTime;
                transform.localScale = new Vector3(faceDir.x, 1, 1);
            }
        }

        if (!FoundPlayer() )
        {
            if (lostTimeCounter>0)
            {
                lostTimeCounter -= Time.deltaTime;
            }
        }
        else
        {
            lostTimeCounter = lostTime;
        }
        
    }

    public virtual bool FoundPlayer()
    {
        return Physics2D.BoxCast(transform.position + (Vector3)centerOffset, checkSize, 0, faceDir, checkDistance,
            attackLayer);
    }

    public void SwitchState(NPCState state)
    {
        var newState = state switch
        {
            NPCState.Patrol => patrolState,
            NPCState.Chase => chaseState,
            NPCState.Skill => skillState,
            
            _ => null
        };
        
        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter(this);
    }

    public virtual Vector3 GetNewPoint()
    {
        return transform.position;
    }


    #region Unity Events
    
    public void OnTakeDamage(Transform attackTrans)
    {
        attacker = attackTrans;
        //振り返り
        if (attackTrans.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (attackTrans.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        
        //傷つくと後退
        isHurt = true;
        anim.SetTrigger("hurt");
        Vector2 dir = new Vector2(transform.position.x - attackTrans.position.x, 0).normalized;
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        StartCoroutine(OnHurt(dir));

    }

    IEnumerator OnHurt(Vector2 dir)
    {
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.5f);
        isHurt = false;
    }

    public void OnDie()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        anim.SetBool("dead",true);
        isDead = true;
    }

    public void DestroyAfterAnimation()
    {
        Destroy(this.gameObject);
    }
    #endregion

    public virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3)centerOffset + new Vector3(checkDistance*-transform.localScale.x,0,0), 0.2f);
    }
}
