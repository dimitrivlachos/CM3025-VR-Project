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
/// State <c>RecipeState1</c> is the first step of the recipe, chopping potatos
/// </summary>
public class RecipeState1 : RecipeBaseState
{
    public override void EnterState(RecipeManager rm)
    {
        base.EnterState(rm);

        // Set text
        objective.text = "\nRoughly cut potatoes and place in the pot.\n" +
                         "(8 chunks - 40g minimum)";

        status.text = "\n\n\n0 / 8 potato chunks";
    }

    public override void Update()
    {
        numberOfPotatos = potBehaviour.numPotatos;

        if(numberOfPotatos < 8)
        {
            status.text = "\n\n\n" + numberOfPotatos + " / 8 potato chunks";
        }
        else
        {
            status.text = "\n\n\n" + numberOfPotatos + " / 8 potato chunks" +
                "\n Objective complete!";
            recipeManager.StartCoroutine(MoveToStep2());
        }
    }

    public override void FixedUpdate()
    {
        // Do nothing
    }

    IEnumerator MoveToStep2()
    {
        yield return new WaitForSeconds(4);
        recipeManager.MoveToState(recipeManager.state2);
    }
}
