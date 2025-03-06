
using UnityEditor.VersionControl;
using UnityEngine;


public class ThirdPersonController : MonoBehaviour
{

    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;
    [Tooltip("The higher the value, the higher the character will jump.")]
    public float jumpForce = 18f;
    [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
    public float jumpTime = 0.85f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;

    float jumpElapsedTime = 0;

    // Player status
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    Animator animator;
    CharacterController cc;


    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Als er geen animaties zijn
        if (animator == null)
            Debug.LogWarning("Waar zijn de animaties");
    }


    
    void Update()
    {

        // Input checkers
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        // Getdown moet apart want dat werkt niet met get status
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        // Dit kijkt of je crouched
        if ( inputCrouch )
            isCrouching = !isCrouching;

        // Run en crouch
        if ( cc.isGrounded && animator != null )
        {

            // Crouch;
            // Dit zorgt dat de collider niet krimt als je crouched
            animator.SetBool("crouch", isCrouching);
            
            // Rennen
            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed );

            
            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
            animator.SetBool("sprint", isSprinting );

        }

        // Jump animatie
        if( animator != null )
            animator.SetBool("air", cc.isGrounded == false );

        // regelt wel of niet springen
        if ( inputJump && cc.isGrounded )
        {
            isJumping = true;
            // Kan niet crouchen wanneer je springt
           isCrouching = false; 
        }

        HeadHittingDetect();

    }


   
    private void FixedUpdate()
    {

 
        float velocityAdittion = 0;
        if ( isSprinting )
            velocityAdittion = sprintAdittion;
        if (isCrouching)
            velocityAdittion =  - (velocity * 0.50f); 

        // Direction 
        float directionX = inputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
        float directionZ = inputVertical * (velocity + velocityAdittion) * Time.deltaTime;
        float directionY = 0;

        // Jump handler
        if ( isJumping )
        {

            
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

            // Jump timer
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        // voeg zwaartekracht toe aan Y 
        directionY = directionY - gravity * Time.deltaTime;

        
        // --- Character rotatie--- 

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

       
        forward = forward * directionZ;
        right = right * directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        // --- End rotation ---

        
        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 moviment = verticalDirection + horizontalDirection;
        cc.Move( moviment );

    }


   // dit zorgt ervoor dat als je character iets met zijn hoofd raakt het stopt met springen
   // Dat zorgt ervoor dat je niet in de lucht blijft vliegen
    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;


        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }

}
