using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnimatorTask {
    public string currentText;
    public string targetText;
    public Text textObject;
    public int index;

    public float speed;
    public string animation_type;

    public TextAnimatorTask(string currentText, string targetText, Text textObject, float speed, string animation_type)
    {
        index = 0;
        this.currentText = currentText;
        this.targetText = targetText;
        this.textObject = textObject;
        this.speed = speed;
        this.animation_type = animation_type;
    }
}
