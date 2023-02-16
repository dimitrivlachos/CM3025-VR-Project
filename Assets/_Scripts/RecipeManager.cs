/*
 * 
 * Code by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 *      
 * Adapted from our Games Dev FSM lecture
 * 
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    #region Variables
    public RecipeBaseState state;
    public GameObject anchor;

    [Header("States")]
    public RecipeState1 state1 = new();
    public RecipeState2 state2 = new();

    [Header("UI")]
    public TextMeshProUGUI objective;
    public TextMeshProUGUI status;

    [Header("Objective information")]
    public GameObject pot;
    public PotBehaviour potBehaviour;
    public int numberOfPotatos;
    public int numberOfCarrots;
    public int water;
    #endregion

    // This makes a singleton Recipe Manager object
    #region Singleton declaration
    public static RecipeManager Recipe { get; private set; }

    private void Awake()
    {
        // Singletone protection stuff
        if (Recipe != null && Recipe != this)
        {
            Destroy(this);
        }
        else
        {
            Recipe = this;
        }
    }
#endregion

    // Start is called before the first frame update
    void Start()
    {
        objective.text = string.Empty;
        status.text = string.Empty;

        numberOfPotatos = 0;
        numberOfCarrots = 0;
        water = 0;

        potBehaviour = pot.GetComponent<PotBehaviour>();

        MoveToState(state1);
    }

    // Update is called once per frame
    void Update()
    {
        state.Update();
        transform.position = anchor.transform.position;
    }

    public void MoveToState(RecipeBaseState state)
    {
        Debug.Log("Entering state: " + state);
        this.state = state;
        state.EnterState(this);
    }
}
