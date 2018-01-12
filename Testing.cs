using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Testing : MonoBehaviour {
    GameObject Display_Text;
    string raw_text;
    int line;
    //50 font == 40 pixels
    //50 font == 39.765 pixels (technically)
    //100 font == 79.5

	// Use this for initialization
	void Start ()
    {
        Display_Text = GameObject.Find("Display_Text");
        raw_text = Display_Text.GetComponent<TextMeshProUGUI>().text;
    }

	
	// Update is called once per frame
	void Update ()
    {
        string[] lines = raw_text.Split('\n');
        //print("length" + lines.Length);
        line =  (int)((Screen.height - Input.mousePosition.y) / (39.765f * Display_Text.transform.localScale.y));
        if (line >= 0 && line < lines.Length)
        {
            //lines[line] = "<color=\"red\">" + lines[line] + "</color>";
            lines[line] = "<color=\"red\"><link=\"red\">" + lines[line] + "</link></color>";
        }
        string output = "";
        for (int i = 0; i < lines.Length; i++)
        {
           // print(lines[i]);
            output += lines[i] + '\n';
        }
        Display_Text.GetComponent<TextMeshProUGUI>().text = output;
    }
}
