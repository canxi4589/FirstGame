using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator animator;
    private bool hasOpened = false; // Track if the door has opened this session
    private bool playerInside = false; // Track if the player is currently inside the trigger

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
        else
        {
            Debug.Log("Animator found on " + gameObject.name);
        }
    }

    // Optional: Update method to handle IsOpening if needed
    void Update()
    {
        // If IsOpening is used and needs to be reset after opening, add logic here
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Door1Open") && stateInfo.normalizedTime >= 0.5f)
        {
            animator.SetBool("IsOpening", true); // Transition to DoorIsOpen
        }
        if (stateInfo.IsName("Door1") && stateInfo.normalizedTime >= 1.0f)
        {
            animator.SetBool("IsOpening", false); // Transition to DoorIsOpen
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered the 2D trigger: " + other.gameObject.name + " with tag: " + other.tag);

        
        if (other.CompareTag("Player") && !playerInside)
        {
            playerInside = true;
            if (!hasOpened)
            {
                Debug.Log("Triggering OpenDoor for Player");
                animator.SetTrigger("OpenDoor");
                hasOpened = true; // Set to true after the first open
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //animator.ResetTrigger("CloseDoor")
        Debug.Log("Something exited the 2D trigger: " + other.gameObject.name + " with tag: " + other.tag);
        if (other.CompareTag("Player") && playerInside)
        {
            playerInside = false;
            Debug.Log("Triggering CloseDoor for Player");
            animator.SetTrigger("CloseDoor");
            // Optionally reset hasOpened to allow reopening (remove this line if you want it to open only once ever)
            hasOpened = false;
        }
    }
}