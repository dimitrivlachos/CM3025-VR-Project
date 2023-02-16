/*
 * 
 * Code by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotBehaviour : MonoBehaviour
{
    [Header("Pot Stuff")]
    float boilingTime;
    private bool isBoiling;
    public GameObject foodParent;

    [SerializeField, Tooltip("Time, in seconds, until locking food in the pot")]
    float parentTime;

    [Header("Sounds")]
    [SerializeField, Tooltip("List of dropping sounds to use")]
    List<AudioClip> dropSounds;
    [SerializeField, Tooltip("List of boiling sounds to use")]
    List<AudioClip> boilSounds;
    [SerializeField] AudioSource audioSource;

    public List<GameObject> foodInPot = new();

    [Header("Nutritional information")]
    FoodItem potFoodItem;

    [Header("Objective information")]
    public int numPotatos = 0;
    [SerializeField] float minPotatoSize = 40;
    public int numCarrots = 0;
    [SerializeField] float minCarrotSize = 1;

    // Start is called before the first frame update
    void Start()
    {
        isBoiling = false;
        potFoodItem = GetComponent<FoodItem>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNutrition();
    }

    private void UpdateNutrition()
    {
        float weight = 0f;
        float calories = 0f;
        float protein = 0f;
        float carbohydrates = 0f;
        float sugar = 0f;
        float fibre = 0f;
        float fat = 0f;

        numPotatos = 0;
        numCarrots = 0;

        FoodItem[] foodInPot = foodParent.GetComponentsInChildren<FoodItem>();

        foreach (var f in foodInPot)
        {
            weight += f.weight;
            calories += f.calories;
            protein += f.protein;
            carbohydrates += f.carbohydrates;
            sugar += f.sugar;
            fibre += f.fibre;
            fat += f.fat;

            switch (f.type)
            {
                case FoodItem.FoodType.Potato:
                    if (f.weight >= minPotatoSize && f.hasBeenCut)
                    {
                        numPotatos ++;
                    }
                    break;
                case FoodItem.FoodType.Carrot:
                    if (f.weight >= minCarrotSize && f.hasBeenCut)
                    {
                        numCarrots++;
                    }
                    break;
            }
        }

        potFoodItem.weight = weight;
        potFoodItem.calories = calories;
        potFoodItem.protein = protein;
        potFoodItem.carbohydrates = carbohydrates;
        potFoodItem.sugar = sugar;
        potFoodItem.fibre = fibre;
        potFoodItem.fat = fat;
    }

    void Boil()
    {
        isBoiling = true;
        int randomSound = Random.Range(0, boilSounds.Count);
        AudioClip clip = boilSounds[randomSound];
        audioSource.clip = clip;
        audioSource.loop = true;
        StartCoroutine(BoilCountdown());
    }

    IEnumerator BoilCountdown()
    {
        yield return new WaitForSeconds(boilingTime);
        isBoiling = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 6) return;
        PlayDropSound();
    }

    private void PlayDropSound()
    {
        if (isBoiling) return; // Guard statement to prevent boiling sound stopping
        if (audioSource.isPlaying) return;

        int randomSound = Random.Range(0, dropSounds.Count);
        AudioClip clip = dropSounds[randomSound];
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        // Guard statements
        //if (obj.layer != 6) return; // If object not on food layer
        if (!obj.CompareTag("Potato") && !obj.CompareTag("Carrot") && !obj.CompareTag("Carrot"))
        {
            return;
        }

        FoodItem f = obj.GetComponentInParent<FoodItem>();
        
        Debug.Log("Object entered: " + obj);
        f.StartCountdowm(foodParent);
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;

        // Guard statements
        if (!obj.CompareTag("Potato") && !obj.CompareTag("Carrot") && !obj.CompareTag("Carrot"))
        {
            return;
        }

        Debug.Log("Object exited: " + other);
        FoodItem f = obj.GetComponentInParent<FoodItem>();

        Debug.Log("Object entered: " + obj);
        f.EndCountDown();
    }

    /*IEnumerator ParentFoodCountdown(GameObject obj)
    {
        // Wait parenting cooldown time
        yield return new WaitForSeconds(parentTime);

        // If the object is still in the trigger area
        // then parent it to the pot
        if (foodInPot.Contains(obj))
        {
            obj.transform.SetParent(transform);
            obj.GetComponent<Rigidbody>().isKinematic = true;
        }
    }*/
}
