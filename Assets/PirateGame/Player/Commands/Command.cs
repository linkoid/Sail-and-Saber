using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public bool isActive;
    public bool isValidTarget;
    public GameObject owner;
    public GameObject currentTarget;
    public GameObject activeTarget;
    public int targetType;

    public Command()
    {
        isActive = false;
        isValidTarget = false;
        targetType = 0;
    }
    
    public void Execute()
    {
        if(!isActive && isValidTarget)
        {
            isActive = true;
            //Execute Command
        }

    }

    public void Cancel()
    {
        isActive = false;
    }

    public void TargetCheck()
    {
        //Run our check here with like a switch statement or something for various commands
        //is a command only a valid target when targetting an enemy, self, or another object?
    }

    public void Update()
    {
        //Run on every frame
    }
}
