using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Event Listeners")]
    public SceneLoadEventSO sceneLoadEvent;
    public VoidEventSO afterSceneLoadedEvent;
    public VoidEventSO loadDataEvent;
    public VoidEventSO backToMenuEvent;
    
    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private CapsuleCollider2D coll;
    private PlayerAnimation playerAnimation;
    private Character character;
    
    public Vector2 inputDirection;
    
    [Header("Movement")]
    public float speed;
    private float runSpeed;
    private float walkSpeed => speed/2.5f;
    public float jumpForce;
    
    public float hurtForce;
    
    public float wallJumpUpScale;
    public float wallJumpForce;
    
    public float slideDistance;
    public float slideSpeed;
    public int slidePowerCost;
    
    
    [Header("Materials")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;
    
    
    [Header("Status")]
    public bool isCrouch;
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool wallJump;
    public bool isSlide;
    
    
    private Vector2 originalOffset;
    private Vector2 originalSize;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        coll = GetComponent<CapsuleCollider2D>();
        playerAnimation = GetComponent<PlayerAnimation>();
        character = GetComponent<Character>();
        
        inputControl=new PlayerInputControl();
        
        originalSize = coll.size;
        originalOffset = coll.offset;
        
        //InputController
        //複数関数可
        inputControl.Gameplay.Jump.started += Jump;//この瞬間Jump関数を実行する

        inputControl.Gameplay.Attack.started += PlayerAttack;

        inputControl.Gameplay.Slide.started += Slide;
        
#region 強制Walk
        runSpeed = speed;
        //Walk Button押しているか速度を決める
        inputControl.Gameplay.Walk.performed += ctx =>
        {
            if (physicsCheck.isGround)
            {
                speed = walkSpeed;
            }
        };

        inputControl.Gameplay.Walk.canceled += ctx =>
        {
            if (physicsCheck.isGround)
            {
                speed = runSpeed;
            }
        };
#endregion

        inputControl.Enable();
    }

    


    private void OnEnable()
    {
        sceneLoadEvent.LoadRequestEvent += OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
    }

    

    private void OnDisable()
    {
        inputControl.Disable();
        sceneLoadEvent.LoadRequestEvent -= OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
    }

    


    private void Update()
    {
        //移動Input Action
        
            inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();
        
    }

    //物理関係
    private void FixedUpdate()
    {
        
        CheckState();
        if (!isHurt && !(isAttack && physicsCheck.isGround) &&!wallJump && !isSlide)
        {
            Move();
        }
        
    }
    
    private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
    {
        inputControl.Gameplay.Disable();
    }
    
    //load game
    private void OnLoadDataEvent()
    {
        isDead = false;
    }
    
    private void OnAfterSceneLoadedEvent()
    {
        inputControl.Gameplay.Enable();
    }

    public void Move()
    {
        //移動
        rb.velocity = new Vector2(0,rb.velocity.y);
        if (!isCrouch)//しゃがんでいない状態
        {
            rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime,rb.velocity.y);
        }
        
        
        //反転
        {
            int faceDir = (int)transform.localScale.x;
            if (inputDirection.x >0)
                faceDir = 1;
            if (inputDirection.x < -0)
                faceDir = -1;

            transform.localScale = new Vector3(faceDir, 1, 1);
        }
        
        //しゃがむ
        isCrouch = physicsCheck.isGround&& inputDirection.y<-0.5f;
        if (isCrouch)
        {
            //コライダーを縮める
            coll.offset = new Vector2(-0.05f, 0.7f);
            coll.size = new Vector2(0.7f, 1.4f);
        }
        else
        {
            //コライダーを元に戻す
            coll.size = originalSize;
            coll.offset = originalOffset;
        }
        
    }
    
    private void Jump(InputAction.CallbackContext obj)
    {
        //地面にいる
        if (physicsCheck.isGround)
        {
            // //垂直方向の速度を０にする
            // var vector2 = rb.velocity;
            // vector2.y = 0;
            // rb.velocity = vector2;
            
            //力を与える
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            GetComponent<AudioDefination>()?.PlayAudioClip();
            
            //SlideからBreak
            isSlide = false;
            StopAllCoroutines();
            
        }else if (physicsCheck.onWall)
        {
            //TODO:(BUG)after scene change, walljump be power up
            rb.AddForce(new Vector2(-inputDirection.x,wallJumpUpScale)*wallJumpForce, ForceMode2D.Impulse);
            wallJump = true;
        }
        
    }
    
    
    private void PlayerAttack(InputAction.CallbackContext obj)
    {
        // if (!physicsCheck.isGround)//地面にいないと攻撃できない
        // {
        //     return;
        // }
        playerAnimation.PlayAttack();
        isAttack = true;
        
    }
    
    //TODO: when attack can slide to break, but audio still play
    private void Slide(InputAction.CallbackContext obj)
    {
        if (!isSlide && physicsCheck.isGround && character.currentPower >= slidePowerCost)
        {
            isSlide = true;

            var targetPos = new Vector3(transform.position.x + slideDistance * transform.localScale.x,
                transform.position.y);
            
            StartCoroutine(TriggerSlide(targetPos));
            
            character.OnSlide(slidePowerCost);
        }
    }

    IEnumerator TriggerSlide(Vector3 target)
    {
        do
        {
            yield return null;

            if (!physicsCheck.isGround)
            {
                break;
            }

            if (physicsCheck.touchLeftWall && transform.localScale.x < 0f || physicsCheck.touchRightWall && transform.localScale.x > 0f)
            {
                isSlide = false;
                break;
            }
            
            rb.MovePosition(new Vector2((transform.position.x + transform.localScale.x * slideSpeed),transform.position.y));
            
            
        } while (Mathf.Abs(target.x - transform.position.x) > 0.2f);

        isSlide = false;
    }

    #region Unity Events

    public void GetHurt(Transform attacker)
    {

        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(transform.position.x - attacker.position.x, 0).normalized;
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
    }

    public void PlayerDead()
    {
        isDead = true;
        inputControl.Gameplay.Disable();
    }

    #endregion

    public void CheckState()
    {
        //マテリアル変換
        coll.sharedMaterial = physicsCheck.isGround ? normal : wall;

        if (physicsCheck.onWall)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y/2);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }

        if (wallJump && rb.velocity.y < 0)
        {
         wallJump = false;   
        }
        
        //死んだら敵は攻撃しない
        //Slide途中は攻撃しない
        if (isDead || isSlide)
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        else
            gameObject.layer = LayerMask.NameToLayer("Player");
    }

}
