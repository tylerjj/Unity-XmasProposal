using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KirbyManager : MonoBehaviour
{

    // X-axis speed modifier. (i.e. walk/run speed modifier)
    public float speedX;
    const int defaultSpeedX = 3;
    // Y-axis speed modifier. (i.e. jump speed modifier)
    public float jumpSpeedY;
    // Speed used to calculate Rigidbody2D movement.
    public float speed;

    // Denotes which direction character is facing.
    public bool isFacingRight;
    // Denotes if character is in air.
    public bool isGrounded;
    // Denotes if character is receiving horizontal movement commands from Input.
    bool hasHorizontalMovementInput;
    // Denotes if character is performing the 'Inhale' action.
    bool isInhaling;
    // Denotes if character's mouth is full (i.e. successful inhale).
    bool isFull;
    // Flag used to prevent movement, both vertical and horizontal.
    bool canMove;
    
    // Stores number of charges remaining for Kirby's "Jump" action.
    public int jumpCharges;
    const int defaultJumpCharges = 6;
    // Stores number of charges remaining for Kirby's "Inhale" action.
    int inhaleCharges;
    const int defaultInhaleCharges = 50;

    // Stores remaining # of frames that Kirby loses control for. 
    int timeoutCount;
    const int fullTimeout = 5;


    public AudioClip[] sounds;

    AudioSource audioSource;
    // Declaration of Kirby character's components.
    SpriteRenderer spriteRenderer;
    Animator animator;
    Rigidbody2D rb;
    
    /* Sources of Input:
     *      0.) Button1  - A - i0
     *      1.) Button2  - B - i1
     *      2.) UpKey    - UpArrow - i2
     *      3.) RightKey - RightArrow - i3
     *      4.) DownKey  - DownArrow  - i4 
     *      5.) LeftKey  - LeftArrow  - i5
     *      6.) Button3  - L - i6
     *      7.) Button4  - R - i7
    */
    
    /* Animation State Machines:
     *      0.) State Machine: Idle
     *          0.0.) Entry State: idle
     *      1.) State Machine: Walk
     *          1.0.) Entry State: walk
     *      2.) State Machine: Run
     *          2.0.) Entry State: run
     *          2.1.) Exit State: run_stop
     *      3.) State Machine: Jump
     *          3.0.) Entry State: jump_start
     *      4.) State Machine: Inhale
     *          4.0.) Entry State: inhale_start
     *          4.1.) State: inhale_cycle
     *          4.2.) State: inhale_timeout
     *          4.3.) State Machine: Full
     *            4.3.0.) Entry State: full_idle
     *            4.3.1.) State: full_walk
     *            4.3.2.) State: full_jump
     *            4.3.3.) Exit State: full_swallow
     *      5.) State Machine: Fall
     *          5.0.) Entry State: full_fall
     */

    /* Control Scheme:
     * 
     *      Change Direction:
     *          0.) Face Right: Press RightArrow
     *              Exception: LeftArrow is being held.
     *          1.) Face Left:  Press LeftArrow
     *              Exception: RightArrow is being held.
     * 
     *      Grounded Horizontal Movement:
     *          0.) Walk Right: Hold RightArrow
     *              Exception: LeftArrow is being held.
     *          1.) Walk Left:  Hold LeftArrow
     *              Exception: RightArrow is being held.
     *          2.) Run Right:  Hold RightArrow + Hold A
     *              Exception: LeftArrow is being held.
     *          3.) Run Left:   Hold LeftArrow + Hold A
     *              Exception: RightArrow is being held.
     *     
     *      Airborne Movement:
     *          Note: Each 'Jump' action decrements jumpCharges by 1.
     *                If jumpCharges reaches 0, 
     *                  then KIRBY CANNOT JUMP again until he has 
     *                  landed on a platform, which restores his 
     *                  jumpCharges to 5.
     *                    
     *          0.) Jump Up: Press B
     *              Exception: jumpCharges == 0
     *          1.) Move Right: Hold RightArrow
     *              Exception: LeftArrow is being held.
     *          2.) Move Left: Hold LeftArrow.
     *              Exception: RightArrow is being held.
     * 
     *      Ability: 
     *          Note: Each consecutive second that the 'Inhale' action 
     *                is active, inhaleCharges is decremented by 1.
     *                If inhaleCharges reaches 0,
     *                  then Kirby's inhale times-out, causing Kirby to
     *                  have to wait to inhale again. 
     *                Kirby's inhaleCharges are restored once the
     *                'Inhale' action is no longer active. 
     *          
     *          0.) Inhale Enemy: Hold DownArrow + Hold A
     *              i.)  Exception: inhaleCharges == 0.
     *              ii.) While Full:  
     *                  a.) Movement: Controls stay the same,
     *                      except running has been disabled.
     *                  b.) Swallow Enemy: Press A
     *                ~ c.) Exhale Enemy:  Hold DownArrow + Press A 
     *                  
     */

    // Use this for initialization
	void Start ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        speedX = defaultSpeedX;

        isFacingRight = true;
        isGrounded = true;

        hasHorizontalMovementInput = false;
        isInhaling = false;
        isFull = false;
        canMove = true;

        jumpCharges = defaultJumpCharges;
        inhaleCharges = defaultInhaleCharges;
        timeoutCount = 0;

        initializeAnimatorParameters();
	}
	void initializeAnimatorParameters()
    {
        // Currently Animator Parameters entail:
        //  Bool isGrounded
        //  Bool canMove
        //  Bool isFull
        //  Integer State (with int mapping to animation state as follows)
        //      0 : idle
        //      1 : walking
        //      2 : running
        //      3 : jumping
        //      4 : inhale
        //      5 : full_fall
        //      6 : inhale_success
        //      7 : inhale_timeout
        //      8 : full_swallow
        //      9 : full_exhale
        //      10 : run_stop
        //      11 : idle_up
        //      12 : idle_twirl
        //      13 : idle_down
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("canMove", canMove);
        animator.SetBool("isFull", isFull);
        animator.SetBool("isInhaling", isInhaling);
        animator.SetInteger("State", 0);
        animator.SetInteger("jumpCharges", jumpCharges);
        animator.SetInteger("inhaleCharges", inhaleCharges);
        animator.SetInteger("timeoutCount", timeoutCount);

    } 

	// Update is called once per frame
	void Update ()
    {
        animator.SetBool("isGrounded", isGrounded);
        MovePlayer(speed);
        
        if ((Input.GetKeyDown(KeyCode.RightArrow)) && (!Input.GetKey(KeyCode.LeftArrow))) 
        {
            if (!isFacingRight)
            {
                changeDirection();
            }
        }
        else if ((Input.GetKeyDown(KeyCode.LeftArrow)) && (!Input.GetKey(KeyCode.RightArrow)))
        {
            if (isFacingRight)
            {
                changeDirection();
            }
        }
    
        if (Input.GetKeyDown(KeyCode.S)){
            triggerJump();
        }

        if (animator.GetBool("isInhaling"))
        {
            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                // isInhaling = false;
                animator.SetBool("isInhaling", false);
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (animator.GetBool("isFull"))
            {
                triggerExhale();
            }
        }
        
        // if a movement key is being held, but not both
        if (!((Input.GetKey(KeyCode.RightArrow)) && (Input.GetKey(KeyCode.LeftArrow)))
            && !((!Input.GetKey(KeyCode.RightArrow)) && (!Input.GetKey(KeyCode.LeftArrow))))
        {
            if (Input.GetKeyUp(KeyCode.A))
            {
                triggerMove();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                triggerJump();
            }
            else if ((Input.GetKey(KeyCode.A)) && (!isFull))
            {
                triggerRun();
            }
            else
            {
                triggerMove();
            }
        }

        // On release of a movement key, and the opposite direction not being held, 
        // trigger behavior depending on what keys are being currently held.
        if (Input.GetKeyUp(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)
            || (Input.GetKeyUp(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            || (!Input.GetKey(KeyCode.RightArrow)) && (!Input.GetKey(KeyCode.LeftArrow)))
        {
            if (((Input.GetKey(KeyCode.DownArrow) && (Input.GetKey(KeyCode.A)))))
            {
                triggerInhale();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (animator.GetBool("isFull"))
                {
                    triggerExhale();
                }
            }
            else
            {
                triggerIdle();
            }
        } // On releasing a movement key, if another movement key is being held, change direction.
        else if (Input.GetKeyUp(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
        {
            if (isFacingRight)
            {
                changeDirection();
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            if (!isFacingRight)
            {
                changeDirection();
            }
        }
            if (Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)
           || (Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)))
        {
            if (Input.GetKeyUp(KeyCode.A))
            {
                triggerMove();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                triggerJump();
            }
            else if ((Input.GetKey(KeyCode.A)) && (!isFull))
            {
                triggerRun();
            }
            else
            {
                triggerMove();
            }
        }
        // [STATIONARY ACTION: INPUT DownArrow, Button1]
        // Grounded
        //      Form - Default
        //          Inhale:
        //              if (Input.Key("DownArrow")) && (Input.Key("Button1")) && (inhaleCharges == defaultInhaleCharges)
        //                  (decrement inhaleCharges by 1,
        //                   disableMovement(),
        //                   transition animation state to inhale_start.)
        //              else if (Input.Key("DownArrow")) && (Input.Key("Button1")) && (inhaleCharges > 0)
        //                  (decrement inhaleCharges by 1,
        //                   disableMovement(),
        //                   transition animation state to inhale_cycle,
        //                   if (inhaleCharges == 0)
        //                      timeoutCount = fullTimeout,
        //                      transition animation state to inhale_timeout)
        //              else if (timeoutCount > 0)
        //                  (timeoutCount --,
        //                   if (timeoutCount == 0)
        //                   inhaleCharges = defaultInhaleCharges,
        //                   enableMovement(),
        //                   transition animation state to idle.)
        //               else 
        //                  (inhaleCharges = defaultInhaleCharges,
        //                   enableMovement().)       
        //                   
        //        ~ Dance: TODO -Low-Priority
        //      Form - Full
        //          Swallow: 
        //              if (Input.KeyDown("Button1")
        //        ~ Exhale: TODO -Low-Priority
    }
    /*
     * http://www.gamasutra.com/blogs/JoeStrout/20150807/250646/2D_Animation_Methods_in_Unity.php 
     */
    public void PlaySound(string name)
    {
        if (!audioSource.enabled)
        {
            return;
        }
        foreach (AudioClip clip in sounds)
        {
            if (clip.name == name)
            {
                audioSource.clip = clip;
                audioSource.Play();
                return;
            }
        }
        Debug.LogWarning(gameObject + ": AudioClip not found: " + name);
    } 
    void MovePlayer(float playerSpeed)
    {
        if (!animator.GetBool("canMove")) {
            playerSpeed = 0;
        }
        if (isGrounded)
        {
            rb.velocity = new Vector3(playerSpeed, rb.velocity.y, 0);
        }
        else
        {
            rb.velocity = new Vector3(playerSpeed / 2, rb.velocity.y, 0);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isGrounded)
        {
            if (collision.gameObject.tag == "PRESENT")
            {
                animator.SetBool("canMove", false);
            }
        }
        if (!isGrounded)
        {
            if (collision.gameObject.tag == "GROUND")
            {
                isGrounded = true;
                animator.SetBool("isGrounded", isGrounded);
                animator.SetInteger("State", 0);
                jumpCharges = defaultJumpCharges;

                // if a movement key is being held, but not both
                if (!((Input.GetKey(KeyCode.RightArrow)) && (Input.GetKey(KeyCode.LeftArrow)))
                    && !((!Input.GetKey(KeyCode.RightArrow)) && (!Input.GetKey(KeyCode.LeftArrow))))
                {
                    if (Input.GetKeyUp(KeyCode.A))
                    {
                        triggerMove();
                    }
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        triggerJump();
                    }
                    else if ((Input.GetKey(KeyCode.A)) && (!isFull))
                    {
                        triggerRun();
                    }
                    else
                    {
                        triggerMove();
                    }
                }

                // On release of a movement key, and the opposite direction not being held, 
                // trigger behavior depending on what keys are being currently held.
                if (Input.GetKeyUp(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)
                    || (Input.GetKeyUp(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)))
                {
                    if (((Input.GetKey(KeyCode.DownArrow) && (Input.GetKey(KeyCode.A)))))
                    {
                        triggerInhale();
                    }
                    else
                    {
                        triggerIdle();
                    }
                } // On releasing a movement key, if another movement key is being held, change direction.
                else if (Input.GetKeyUp(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
                {
                    if (isFacingRight)
                    {
                        changeDirection();
                    }
                }
                else if (Input.GetKeyUp(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                {
                    if (!isFacingRight)
                    {
                        changeDirection();
                    }
                }
                    if (Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)
                    || (Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)))
                {
                    if (Input.GetKeyUp(KeyCode.A))
                    {
                        triggerMove();
                    }
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        triggerJump();
                    }
                    else if ((Input.GetKey(KeyCode.A)) && (!isFull))
                    {
                        triggerRun();
                    }
                    else
                    {
                        triggerMove();
                    }
                }
                }
             }
    }
    void changeDirection()
    {
        if (isFacingRight)
        {
            isFacingRight = false;
            spriteRenderer.flipX = true;
        }
        else if (!isFacingRight)
        {
            isFacingRight = true;
            spriteRenderer.flipX = false;
        }
    }
    void triggerIdle()
    {
        if (isFacingRight)
        {
            //speed = speedX - speed;
            speed = 0;
        }
        else if (!isFacingRight)
        {
            //speed = speedX + speed;
            speed = 0;
        }
        if (isGrounded)
        {
            if (animator.GetInteger("State") < 11 || animator.GetInteger("State") > 13)
            {
                if (Input.GetKey(KeyCode.D) && Input.GetKeyDown(KeyCode.F)
                    || (Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.D))
                    || (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.F)))
                {
                    animator.SetInteger("State", 12);
                }
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D))
                {
                    animator.SetInteger("State", 11);
                }
                else if (Input.GetKeyDown(KeyCode.F) || Input.GetKey(KeyCode.F))
                {
                    animator.SetInteger("State", 13);
                }
                else
                {
                    animator.SetInteger("State", 0);
                }

            }
            if (Input.GetKey(KeyCode.D) && Input.GetKeyDown(KeyCode.F)
                || (Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.D))
                || (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.F)))
            {
                animator.SetInteger("State", 12);
            } 
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D))
            {
                animator.SetInteger("State", 11);
            }
            else if (Input.GetKeyDown(KeyCode.F)||Input.GetKey(KeyCode.F))
            {
                animator.SetInteger("State", 13);
            } else
            {
                animator.SetInteger("State", 0);
            }
                    



        }
    }
    void triggerMove()
    {
        
        if (animator.GetBool("canMove"))
        {
           
            if (isFacingRight)
            {
                speed = speedX;
            } 
            else
            {
                speed = -speedX;
            }
            if (isGrounded)
            {
                animator.SetInteger("State", 1);
            }
        }
    }

    void triggerRun()
    {
        
        if (animator.GetBool("canMove") && isGrounded)
        {
           
            if (isFacingRight)
            {
                //speed = speedX;
                speed = 2 * speedX;
            } else
            {
                //speed = -speedX;
                speed = -2 * speedX;
            }
            animator.SetInteger("State", 2);
        }
    }

    void triggerJump()
    {
        if (animator.GetBool("canMove"))
        {
            if (jumpCharges > 0)
            {
                if (isFull)
                {
                    rb.AddForce(new Vector2(rb.velocity.x / 2, jumpSpeedY/2));
                }
                else
                {
                    rb.AddForce(new Vector2(rb.velocity.x, jumpSpeedY));
                }
               
                animator.SetInteger("State", 3);
                isGrounded = false;
                if (isFull)
                {
                    jumpCharges = jumpCharges - 3;
                }
                else
                {
                    jumpCharges = jumpCharges - 1;
                }
                if (jumpCharges < 1)
                {
                    animator.SetInteger("State", 5);
                }
            } 
        }
    }
    void triggerInhale()
    {
                // TODO: enable collision triggerBox.
                // TODO: enable air effects.
            if (animator.GetInteger("State") != 7) {
                  animator.SetInteger("State", 4);
            }
                
               // isInhaling = true;
              //  animator.SetBool("isInhaling", true);
        /*
        else if (isInhaling)
        {
            inhaling();
        }
        */
    }
    void triggerExhale()
    {
        if (animator.GetBool("isFull")) {
            animator.SetInteger("State", 9);
        }
    } 
    /*
    void inhaling()
    {
        if (isInhaling && inhaleCharges > 0)
        {
            inhaleCharges = inhaleCharges--;
            if (inhaleCharges == 0)
            {
                timeoutCount = fullTimeout;
                endInhale();
            }
        }
        else
        {
            endInhale();
        }
    }
    void endInhale()
    {
        if (isInhaling)
        {
            // TODO: disable inhale's collision triggerBox.
            // TODO: disable inhale's air effects.

            // If Kirby is in timeout. 
            if (timeoutCount > 0)
            {
                timeoutCount = timeoutCount--;
                // TODO: Behavior while in timeout
                if (timeoutCount == 0)
                {
                    isInhaling = false;
                    inhaleCharges = defaultInhaleCharges;
                    canMove = true;
                }
            }
            else
            {   // If Kirby successfully inhales an enemy.
                if (isFull)
                {
                    // TODO: enter full state. 
                    animator.SetBool("isFull", isFull);
                    animator.SetInteger("State", 0);
                }
                else
                {
                    // TODO: crouch for a frame or two, then exit inhale state.
                }
                isInhaling = false;
                canMove = true;
            }
        }
    } */
}
