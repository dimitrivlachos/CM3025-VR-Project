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

public abstract class RecipeBaseState
{
    #region Variables
    protected TextMeshProUGUI objective;
    protected TextMeshProUGUI status;
    protected GameObject pot;
    protected PotBehaviour potBehaviour;

    // Objective information
    protected int numberOfPotatos;
    protected int numberOfCarrots;
    protected int water;

    protected RecipeManager recipeManager;
    #endregion
    public virtual void EnterState(RecipeManager rm)
    {
        // Set variables
        objective = rm.objective;
        status = rm.status;
        pot = rm.pot;
        potBehaviour = rm.potBehaviour;

        numberOfPotatos = rm.numberOfPotatos;
        numberOfCarrots = rm.numberOfCarrots;
        water = rm.water;
        
        recipeManager = rm;
    }

    public abstract void Update();

    public abstract void FixedUpdate();


}
