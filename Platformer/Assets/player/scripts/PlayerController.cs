using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
 


    private enum State { idle, walk, jump, fall, hurt, climb};
    private State state = State.idle;
    private Collider2D coll;

    [HideInInspector] public bool canClimb = false;
    [HideInInspector] public bool bottomLadder = false;
    [HideInInspector] public bool topLadder = false;
    public Ladder ladder;
    private float naturalGravity;
    [SerializeField] float climbSpeed = 3f;



    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpForce = 40f;
    [SerializeField] private int monkey = 0;
    [SerializeField] private TextMeshProUGUI monkeyText;
    [SerializeField] private float hurtForce = 40f;
    [SerializeField] private AudioSource monkeyGet;
    [SerializeField] private AudioSource hurtSound;
    [SerializeField] private int health;
    [SerializeField] private TextMeshProUGUI healthAmountText;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        naturalGravity = rb.gravityScale;
    }
    private void Update()
    {
        if(state == State.climb)
        {
            Climb();
        }
        else if(state != State.hurt)
        {
            Movement();
        }
       
        AnimationState();
        anim.SetInteger("state", (int)state);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Collectible")
        {
            monkeyGet.Play();
            Destroy(collision.gameObject);
            monkey += 1;
            monkeyText.text = monkey.ToString(); 
        }
        if(collision.tag == "PowerUp")
        {
            monkeyGet.Play();
            Destroy(collision.gameObject);
            jumpForce = 60f;
            GetComponent<SpriteRenderer>().color = Color.yellow;           // If you want to change the sprite color to indicate powerup
                                                                            // Dont want to do it here since aestically it doesn't look good
            //GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0); // This will make the sprite completely transparent
                                                                            //Confusingly, instead of being a value between 0 and 255, like it was in the editors colour changer, now we have to give it 
                                                                            //a value between 0 and 1 for each colour property, 0 being no colour, 1 being full colour for that property.
            StartCoroutine(ResetPower());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Trampoline") && (state == State.jump || state == State.fall))
        {
            Jump();
        }
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if(other.gameObject.tag == "Enemy")
        {
            if (state == State.fall)
            {
                enemy.JumpedOn();
                Jump();
            }
            else
            {
                state = State.hurt;
                HandleHealth(); // Updates health ui and changes scene when health is 0
                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    hurtSound.Play();
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    hurtSound.Play();
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
        }
    }

    private void HandleHealth()
    {
        health -= 1;
        healthAmountText.text = health.ToString();
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Movement()
    {
        float hDirection = Input.GetAxis("Horizontal");

        if (canClimb && Mathf.Abs(Input.GetAxis("Vertical")) > .1f)
        {
            state = State.climb;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            transform.position = new Vector3(ladder.transform.position.x, rb.position.y);
            rb.gravityScale = 0f;
        }
     
        if (hDirection < 0)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            gameObject.GetComponent<SpriteRenderer>().flipX = true;

        }
        else if (hDirection > 0)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            gameObject.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {

        }
        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            Jump();
        }
    }
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jump;
    }
    private void AnimationState()
    {
        if(state == State.climb)
        {

        }
        else if(state == State.jump)
        {
            if(rb.velocity.y < .1f)
            {
                state = State.fall;
            }
        }
        else if(state == State.fall)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if(state == State.hurt)
        {
            if(Mathf.Abs(rb.velocity.x) < .1f)
            {
                state = State.idle;
            }
        }
        else if(Mathf.Abs(rb.velocity.x) > 2f)
        {
            state = State.walk;
        }

        else
        {
            state = State.idle;
        }
        
    }
    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(10);
        jumpForce = 40f;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
    
    private void Climb()
    {
        if (Input.GetButtonDown("Jump"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.gravityScale = naturalGravity;
            anim.speed = 1f;
            Jump();
            return;
        }
        float vDirection = Input.GetAxis("Vertical");
        if(vDirection > .1f && !topLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
        }
        else if(vDirection < -.1f && !bottomLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
        }
        else
        {
            anim.speed = 0f;
            rb.velocity = Vector2.zero;
        }
    }
}
