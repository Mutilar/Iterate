using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TextAnimator : MonoBehaviour {

    public static List<TextAnimatorTask> tasks = new List<TextAnimatorTask>();
	public static char[] possibleSymbols;

	// Use this for initialization
	void Start () {
        //string possibleSymbolsList = "abcdefghijklmnopqrstuvqxyzABCDEFGHIJKLMNOQRSTUVWXYZ11223344556677889900!@#$%^&*";
        string possibleSymbolsList = "1!2@3#4$5%6^7&8*9(0)-_=+QqWwEeRrTtYyUuIiOoPp[{]}\\|AaSsDdFfGgHhJjKkLl;:'\"ZzXxCcVvBbNnMm<,>.?/";//"abcdefghijklmnopqrstuvqxyzABCDEFGHIJKLMNOQRSTUVWXYZ11223344556677889900!@#$%^&*";
        possibleSymbols = possibleSymbolsList.ToCharArray();
	}
	
	// Update is called once per frame
	void Update () 
	{
        int tail_size = 10;
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].speed > Random.value)
            {
                if (tasks[i].animation_type == "scroll")
                {
                    if (!(tasks[i].currentText.IndexOf(tasks[i].targetText) == 0))
                    {
                        int length = tasks[i].currentText.Length;
                        if (tasks[i].targetText.Length < tasks[i].currentText.Length)
                        {
                            length = tasks[i].targetText.Length;
                        }
                        tasks[i].currentText += possibleSymbols[((int)(Random.value * possibleSymbols.Length))];
                        if (tasks[i].currentText.Length > tail_size)
                        {
                            tasks[i].currentText = tasks[i].currentText.Remove(tasks[i].currentText.Length - tail_size, 1);
                            tasks[i].currentText = tasks[i].currentText.Insert(tasks[i].currentText.Length - tail_size, tasks[i].targetText[tasks[i].currentText.Length - tail_size].ToString());
                        }
                        tasks[i].textObject.text = tasks[i].currentText.Substring(0, length);
                    }
                    else
                    {
                        tasks.RemoveAt(i);
                        break;
                    }
                }
                else if (tasks[i].animation_type == "fade")
                {
                    if (!(tasks[i].currentText.IndexOf(tasks[i].targetText) == 0))
                    {
                        for (int repeats = 0; repeats < 4; repeats++)
                        {
                            int random_index;
                            for (int repeats2 = 0; repeats2 < 2; repeats2++)
                            {
                                random_index = (int)(Random.value * tasks[i].targetText.Length);
                                if (tasks[i].currentText[random_index] != tasks[i].targetText[random_index])
                                {
                                    tasks[i].currentText = tasks[i].currentText.Insert(random_index, possibleSymbols[((int)(Random.value * possibleSymbols.Length))].ToString());
                                    tasks[i].currentText = tasks[i].currentText.Remove(random_index + 1, 1);
                                }
                            }
                            random_index = (int)(Random.value * tasks[i].targetText.Length);
                            tasks[i].currentText = tasks[i].currentText.Insert(random_index, tasks[i].targetText[random_index].ToString());
                            tasks[i].currentText = tasks[i].currentText.Remove(random_index + 1, 1);
                        }
                        tasks[i].textObject.text = tasks[i].currentText;
                    }
                    else
                    {
                        tasks.RemoveAt(i);
                        break;
                    }
                }
            }
        }
	}

	public static void changeText(string text, string current, Text obj, float speed, string style)
	{
        tasks.Add(new TextAnimatorTask(current, text, obj, speed, style));
	}
    public static bool areTasksRunning()
    {
        return tasks.Count != 0;
    }
}
