/*
 * 
 * Code by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 * 
 */

using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NutritionScaleBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private FoodItem item;

    [Header("Text Displays")]
    [SerializeField] private GameObject foodName;
    [SerializeField] private GameObject weight;
    [SerializeField] private GameObject calories;
    [SerializeField] private GameObject protein;
    [SerializeField] private GameObject carbohydrates;
    [SerializeField] private GameObject sugar;
    [SerializeField] private GameObject fiber;
    [SerializeField] private GameObject fat;

    private TextMeshProUGUI textMeshPro_name_text;
    private TextMeshProUGUI textMeshPro_weight_text;
    private TextMeshProUGUI textMeshPro_calories_text;
    private TextMeshProUGUI textMeshPro_protein_text;
    private TextMeshProUGUI textMeshPro_carbohydrates_text;
    private TextMeshProUGUI textMeshPro_sugar_text;
    private TextMeshProUGUI textMeshPro_fiber_text;
    private TextMeshProUGUI textMeshPro_fat_text;

    // Start is called before the first frame update
    void Start()
    {
        obj = null;
        item = null;

        textMeshPro_name_text           = foodName.GetComponent<TextMeshProUGUI>();
        textMeshPro_weight_text         = weight.GetComponent<TextMeshProUGUI>();
        textMeshPro_calories_text       = calories.GetComponent<TextMeshProUGUI>();
        textMeshPro_protein_text        = protein.GetComponent<TextMeshProUGUI>();
        textMeshPro_carbohydrates_text  = carbohydrates.GetComponent<TextMeshProUGUI>();
        textMeshPro_sugar_text          = sugar.GetComponent<TextMeshProUGUI>();
        textMeshPro_fiber_text          = fiber.GetComponent<TextMeshProUGUI>();
        textMeshPro_fat_text            = fat.GetComponent<TextMeshProUGUI>();



    }

    // Update is called once per frame
    void Update()
    {
        // Guard statement
        if (obj == null)
        {
            item = null;
            textMeshPro_name_text.text          = "";

            textMeshPro_weight_text.text        = "";
            textMeshPro_calories_text.text      = "";
            textMeshPro_protein_text.text       = "";
            textMeshPro_carbohydrates_text.text = "";
            textMeshPro_sugar_text.text         = "";
            textMeshPro_fiber_text.text         = "";
            textMeshPro_fat_text.text           = "";
            return;
        }
        if (item == null)
        {
            item = obj.GetComponentInParent<FoodItem>();
        }
        if (item == null) return;

        // Functional code

        // Set the food name
        textMeshPro_name_text.text = item.type.ToString();

        // Get the nutritional values and round them to one decimal place
        float weight        =   Mathf.Round(item.weight * 10f) / 10f;
        float calories      =   Mathf.Round(item.calories * 10f) / 10f;
        float protein       =   Mathf.Round(item.protein * 10f) / 10f;
        float carbohydrates =   Mathf.Round(item.carbohydrates * 10f) / 10f;
        float sugar         =   Mathf.Round(item.sugar * 10f) / 10f;
        float fibre         =   Mathf.Round(item.fibre * 10f) / 10f;
        float fat           =   Mathf.Round(item.fat * 10f) / 10f;

        // Set the text to the newly rounded values
        textMeshPro_weight_text.text        = weight.ToString() + "g";
        textMeshPro_calories_text.text      = calories.ToString() + "kcal";
        textMeshPro_protein_text.text       = protein.ToString() + "g";
        textMeshPro_carbohydrates_text.text = carbohydrates.ToString() + "g";
        textMeshPro_sugar_text.text         = sugar.ToString() + "g";
        textMeshPro_fiber_text.text         = fibre.ToString() + "g";
        textMeshPro_fat_text.text           = fat.ToString() + "g";
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Object entered: " + other.name);
        obj = other.GameObject();
        
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("Object exited");
        obj = null;
    }
}
