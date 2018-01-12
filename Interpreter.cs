using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpreter
{
    string class_name = "HelloWorld";
    List<Variable> variables = new List<Variable>();
    List<Line> lines;

    public int current_line = 0;


    public void step()
    {
        if (lines[current_line].initialization != "")
        {

        }
        current_line++;
        


    }




    public void addLines(List<string> lines_in)
    {
        lines = new List<Line>();
        for (int i = 0; i < lines_in.Count; i++)
        {
            lines.Add(new Line(lines_in[i]));
        }
    }

    public bool evaluate_condition(string condition)
    {
        //eval(left)  (conditional)    eval(right)
        //return result


        //all works similarly: bools: "true equals true", true.
        
        //if no conditional, add "equals true" (true == true) etc.

        return false;
    }
    public string evaluate_expression(string expression)
    {


        return "";
    }

    public override string ToString()
    {
        ColorCoder.object_list[0] = class_name;
        string output = "", output_line = "";
        int tab_counter = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            /* NUMBERING LINES */
            if (i + 1 < 10) output_line = "0" + (i + 1) + " " + "\t";
            else output_line = (i + 1) + " " + "\t";
            /* INDENTING LINES */
            if (lines[i].ToString().Contains("}")) tab_counter--;
            for (int k = 0; k < tab_counter; k++) output_line += "\t";
            if (lines[i].ToString().Contains("{")) tab_counter++;
            /* LINE STATEMENT */
            output_line += lines[i].ToString();
            /* COLORING LINES */
            if (lines[i].comment) output_line = ColorCoder.comment(output_line) + '\n';
            else if(current_line == i) output_line = ColorCoder.highlight(output_line) + '\n';
            else output_line = ColorCoder.colorize(output_line) + '\n';
            output += output_line;
        }
        return output;
    }
}
