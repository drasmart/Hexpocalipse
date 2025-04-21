using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeveloperConsole
{
    public class PlayerController : MonoBehaviour
    {
        Rigidbody2D rb;
        Animator animator;
        public SpriteRenderer spriteRenderer;
        Vector2 velocity;
        [SerializeField] float maxSpeed;
        [SerializeField] float maxAcceleration;
        [SerializeField] float jumpSpeed;
        [SerializeField] float groundedDistance;
        [SerializeField] float maxGraceTime;
        bool lastGrounded;
        bool jumpGrace;

        IEnumerator jumpGraceCoroutine;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            Console.AddCommand("tp", TeleportCommand);
            Console.AddCommand("gravity", SetGravityCommand);
            Console.AddCommand("speed", SetSpeedCommand);
        }

        void Update()
        {
            Movement();
        }

        bool IsGrounded()
        {
            LayerMask mask = LayerMask.GetMask("Default");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, groundedDistance, mask);
            Debug.DrawRay(transform.position, -Vector2.up * groundedDistance, hit ? Color.green : Color.yellow);

            return hit ? true : false;
        }

        void Movement()
        {
            velocity = rb.linearVelocity;

            if (velocity.x > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            if (velocity.x < -0.01f)
            {
                spriteRenderer.flipX = true;
            }

            

            Vector2 playerInput = Vector2.zero;
            if (!Console.Singleton.isSelected)
            {
                playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);

            Vector2 desiredVelocity = new Vector2(playerInput.x, 0f) * maxSpeed;

            float maxSpeedChange = maxAcceleration * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);

            

            bool isGrounded = IsGrounded();
            if (!isGrounded && lastGrounded)
            {
                jumpGraceCoroutine = JumpGrace();
                StartCoroutine(jumpGraceCoroutine);
            }
            if (isGrounded && !lastGrounded)
            {
                if (jumpGraceCoroutine != null)
                    StopCoroutine(jumpGraceCoroutine);
            }

            if (Input.GetButtonDown("Jump") && (isGrounded || jumpGrace) && !Console.Singleton.isSelected)
            {
                velocity = new Vector2(velocity.x, jumpSpeed);
            }

            rb.linearVelocity = velocity;

            animator.SetFloat("speedAbs", Mathf.Abs(velocity.x));
            animator.SetBool("isGrounded", isGrounded);

            lastGrounded = isGrounded;
        }

        IEnumerator JumpGrace()
        {
            jumpGrace = true;
            for (float timer = 0f; timer < maxGraceTime; timer += 0.01f)
            {
                yield return new WaitForSeconds(.01f);
            }
            jumpGrace = false;
            yield return null;        
        }

        void TeleportCommand(string[] args)
        {
            if (args.Length < 2 || args.Length > 2)
            {
                Console.PrintWarning("Wrong number of arguments");
                return;
            }

            transform.position = new Vector2(float.Parse(args[0]), float.Parse(args[1]));
        }

        void SetGravityCommand(string[] args)
        {
            Physics2D.gravity = new Vector2(0.0f, float.Parse(args[0]));
            Console.PrintSuccess("Gravity acceleration set to " + Physics2D.gravity.y);
        }

        void SetSpeedCommand(string[] args)
        {
            maxSpeed = float.Parse(args[0]);
            Console.PrintSuccess("Player max speed set to " + maxSpeed);
        }
    }
}
