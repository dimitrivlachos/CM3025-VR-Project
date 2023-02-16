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
using UnityEngine;

/// <summary>
/// State <c>RecipeState1</c> is the second step of the recipe, chopping carrots
/// </summary>
public class RecipeState2 : RecipeBaseState
{
    public override void EnterState(RecipeManager rm)
    {
        base.EnterState(rm);

        // Set text
        objective.text = "\nChop carrot into thin slices.\n" +
                         "(10 slices - 1g minimum)";

        status.text = "\n\n\n0 / 10 carrot slices";
    }

    public override void Update()
    {
        numberOfCarrots = potBehaviour.numCarrots;

        if (numberOfCarrots < 10)
        {
            status.text = "\n\n\n" + numberOfCarrots + " / 10 carrot slices";
        }
        else
        {
            status.text = "\n\n\n" + numberOfCarrots + " / 10 carrot slices" +
                "\n Objective complete!";
            recipeManager.StartCoroutine(MoveToStep3());
        }
    }

    public override void FixedUpdate()
    {
        // Do nothing
    }

    IEnumerator MoveToStep3()
    {
        yield return new WaitForSeconds(4);
        //recipeManager.MoveToState(recipeManager.state2);
        Debug.Log("Not there yet!");
    }
}
