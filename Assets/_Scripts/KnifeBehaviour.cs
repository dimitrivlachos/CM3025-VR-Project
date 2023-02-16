/*
 * 
 * Code by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 * 
 * References:
 *       https://forum.unity.com/threads/how-to-get-which-hand-is-grabbing-an-xr-grab-interactable-object.946045/
 */

using DynamicMeshCutter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(AudioSource))]
public class KnifeBehaviour : MonoBehaviour
{
    [Header("Cutting")]
    [SerializeField] private XRDirectInteractor rHand;
    [SerializeField] private XRDirectInteractor lHand;
    [SerializeField] private InputActionReference right_cut_event = null;
    [SerializeField] private InputActionReference left_cut_event = null;
    [SerializeField] private KnifePlaneBehaviour cutter;
    [SerializeField] private float cutCooldownTimer = 0.5f;
    
    [Header("Knife Behaviour")]
    [SerializeField] private XRGrabInteractable grabInteractable;

    [Header("Sounds")]
    [SerializeField, Tooltip("List of cutting sounds to use")]
    List<AudioClip> cuttingSounds;
    [SerializeField] AudioSource audioSource;

    private Collider collider;
    private bool isSelected;

    private bool has_cut_recently;

    private void Awake()
    {
        right_cut_event.action.started += Right_cut;
        left_cut_event.action.started += Left_cut;
    }

    private void OnDestroy()
    {
        right_cut_event.action.started -= Right_cut;
        left_cut_event.action.started -= Left_cut;
    }

    private void Start()
    {
        has_cut_recently = false;
        grabInteractable = GetComponent<XRGrabInteractable>();
        collider = GetComponent<Collider>();
        isSelected = false;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Check if object is grabbed, if so, then make collider a trigger
        if(grabInteractable.isSelected)
        {
            isSelected = true;
            collider.isTrigger = true;
        }
        else if(collider.isTrigger)
        {
            isSelected = false;
            collider.isTrigger = false;
        }
    }

    private void Left_cut(InputAction.CallbackContext context)
    {
        List<IXRSelectInteractable> interactables = lHand.interactablesSelected;

        ConfirmValidCut(interactables);
    }

    private void Right_cut(InputAction.CallbackContext context)
    {
        List<IXRSelectInteractable> interactables = rHand.interactablesSelected;

        ConfirmValidCut(interactables);
    }

    private void ConfirmValidCut(List<IXRSelectInteractable> interactables)
    {
        foreach (IXRSelectInteractable Interactable in interactables)
        {
            if (Interactable.transform.CompareTag("Knife"))
            {
                InitiateCut();
            }
        }
    }

    public void InitiateCut()
    {
        // Guard statements
        if (!isSelected)
        {
            Debug.Log("Can't cut: not selected!");
            return;
        }
        if (has_cut_recently)
        {
            Debug.Log("Can't cut: in cooldown!");
            return;
        }
        Debug.Log("Attempting cut");
        // Set the cut value to true
        has_cut_recently = true;

        // Perform the cut
        cutter.Cut();
        PlayCutSounds();

        // Begin cut cooldown timer
        StartCoroutine(ResetCutter());
    }

    IEnumerator ResetCutter() {
        // Wait for cooldown time
        yield return new WaitForSeconds(cutCooldownTimer);
        Debug.Log("Cooldown complete: cut ready");
        has_cut_recently = false;
    }

    void PlayCutSounds()
    {
        // Select a sound from the list at random
        int randomSound = Random.Range(0, cuttingSounds.Count);
        // Pull that clip from the list
        AudioClip clip = cuttingSounds[randomSound];
        audioSource.clip = clip;
        audioSource.Play();
    }
}
