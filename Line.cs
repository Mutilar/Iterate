using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line
{
    string line;

    int jumpTo_line; //line to go to after this line

    public bool comment = false;

    public string current_command = "";

    public string initialization = "";
    public string condition = "";
    public string modifier = "";


    public Line (string line)
    {
        this.line = line;

        if (line.IndexOf("//") == 0) comment = true;

        if (line.IndexOf("for (") == 0)
        {
            initialization = "";
            condition = "";
            modifier = "";
        }
        if (line.IndexOf("while (") == 0)
        {
            condition = "";
        }
        if (line.IndexOf("if (") == 0)
        {
            condition = "";
        }
        if (line.IndexOf("else") == 0)
        {
            if (line.IndexOf("else if (") == 0)
            {

            }
            else
            {

            }
        }
    }

    public override string ToString()
    {
        return line;
    }
}
