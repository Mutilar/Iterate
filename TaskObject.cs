using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskObject
{
    public int jumpTo_line;
    public int starting_line;
    public int ending_line;

    public string condition;
    public string variable_modifier; //for loops (i++);

    public TaskObject(int starting_line, int ending_line, string condition, string variable_modifier)
    {
        this.jumpTo_line = starting_line;
        this.starting_line = starting_line;
        this.ending_line = ending_line;
        this.condition = condition;
        this.variable_modifier = variable_modifier;
    }
    public TaskObject(int jumpTo_line, int starting_line, int ending_line, string condition, string variable_modifier)
    {
        this.jumpTo_line = jumpTo_line;
        this.starting_line = starting_line;
        this.ending_line = ending_line;
        this.condition = condition;
        this.variable_modifier = variable_modifier;
    }

    public override string ToString()
    {
        return "Starting at : " + (starting_line + 1) + "\nJumping to : " + (jumpTo_line + 1) + "\nEnding at : " + (ending_line + 1) + "\nWith the condition : " + condition + "\nModifying : " + variable_modifier;
    }
}
