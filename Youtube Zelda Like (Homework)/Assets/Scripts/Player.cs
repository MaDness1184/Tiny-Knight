using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    idle,
    walk,
    interact,
    attack,
    stagger,
    transition,
    dead
}

public class Player : MonoBehaviour
{
    [Header("Player State")]
    public PlayerState currentState; // current PlayerState
    public bool invulnerable = false;

    [Header("Player Stats")]
    public float health = 5;
    public float walkSpeed = 5f; // Player walk speed

    [Header("Coroutines")]
    public float attackDelay = 0.25f; // Attack freeze delay
    public float transitionHalt = 2.5f; // Room Transition freeze delay

    [Header("Cached References")]
    // Private Chached References
    private Rigidbody2D myRigidbody2D; // cached Rigidbody2D reference
    private Vector2 changeInVelocity; // current velocity direction and magnitude on the player
    private Animator myAnimator; // chached Animator Reference

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>(); // Set myRigidbody to the object's Rigidbody component
        myAnimator = GetComponent<Animator>(); // Set myAnimator to the object's Animator component
        myAnimator.SetFloat("moveX", 0);
        myAnimator.SetFloat("moveY", -1);
        currentState = PlayerState.idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != PlayerState.attack && currentState != PlayerState.stagger
            && currentState != PlayerState.transition) // If Player is not attacking / staggered / transitioning
        {
            MoveCharacter(); // Move the character every frame
            Attack(); // Attack on button press
        }
    }

    public void ChangeState(PlayerState newState) // Change PlayerState
    {
        if (currentState != newState)
        {
            currentState = newState;
        }
    }

    public void ChangeState(PlayerState oldState, PlayerState newState) // Change PlayerState only if currentState = oldState
    {
        if (currentState == oldState)
        {
            currentState = newState;
        }
    }

    void MoveCharacter()
    {
        changeInVelocity.Normalize(); // Makes diagnal speed slightly slower
        changeInVelocity.x = Input.GetAxisRaw("Horizontal") * walkSpeed; // (1, 0 or -1) * walkspeed * seconds from the last frame
        changeInVelocity.y = Input.GetAxisRaw("Vertical") * walkSpeed; // (1, 0 or -1) * walkspeed * seconds from the last frame
        myRigidbody2D.velocity = changeInVelocity;

        bool playerIsMoving = Mathf.Abs(myRigidbody2D.velocity.x) > Mathf.Epsilon
           || Mathf.Abs(myRigidbody2D.velocity.y) > Mathf.Epsilon;
        if (playerIsMoving) // If velocity magnitude > Epsilon
        {
            if (Mathf.Abs(changeInVelocity.x) > Mathf.Abs(changeInVelocity.y))
            {
                myAnimator.SetFloat("moveX", changeInVelocity.x); // change idle animation relative to where the Player is facing
                myAnimator.SetFloat("moveY", 0);
            }
            else
            {
                myAnimator.SetFloat("moveX", 0);
                myAnimator.SetFloat("moveY", changeInVelocity.y); // change idle animation relative to where the Player is facing
            }
            myAnimator.SetBool("isWalking", true);
            ChangeState(PlayerState.walk);
        }
        else
        {
            myAnimator.SetBool("isWalking", false);
            ChangeState(PlayerState.idle); // change to idle if player is not walking
        }
    }

    void Attack() // Runs attack Co if Player presses attack button
    {
        if(Input.GetButtonDown("Attack") && currentState != PlayerState.attack
             && currentState != PlayerState.stagger)
        {
            StartCoroutine(AttackCo());
        }
    }

    public void Hit(float knocktime, float recoverDelay, float damage) // Start KnockCo and take damage
    {
        if (invulnerable != true)
            TakeDamage(damage);
        if (health > 0f)
            StartCoroutine(KnockCo(knocktime, recoverDelay));
    }

    private void TakeDamage(float damage) // Take dmg and update hp
    {
        if (currentState != PlayerState.stagger && health > 0f)
        {
            health -= damage;
        }
        if (health <= 0f) // kill off the Player once health reaches 0
        {
            ChangeState(PlayerState.dead);
            this.gameObject.SetActive(false); // will be put into a coroutine later
            Debug.Log("Player has died.");
        }
        invulnerable = true;
    }

    public void Transition()
    {
        StartCoroutine(TransitionCo());
    }

    private IEnumerator KnockCo(float knocktime, float recoverDelay) // Player KnockCo
    {
        if (myRigidbody2D != null && currentState != PlayerState.stagger)
        {
            ChangeState(PlayerState.stagger);
            yield return new WaitForSeconds(knocktime);
            myRigidbody2D.velocity = Vector2.zero;
            yield return new WaitForSeconds(recoverDelay);
            invulnerable = false;
            ChangeState(PlayerState.idle);
        }
    }

    private IEnumerator AttackCo() // Player AttackCo
    {
        myRigidbody2D.velocity = Vector2.zero;
        ChangeState(PlayerState.attack);
        myAnimator.SetBool("isAttacking", true);
        yield return null;
        myAnimator.SetBool("isAttacking", false);
        yield return new WaitForSeconds(attackDelay);
        ChangeState(PlayerState.idle);
    }

    private IEnumerator TransitionCo() // Player TransitionCo
    {
        ChangeState(PlayerState.transition);
        myAnimator.SetBool("isWalking", false);
        myRigidbody2D.velocity = Vector2.zero;
        yield return new WaitForSeconds(transitionHalt);
        ChangeState(PlayerState.idle);
    }
}
