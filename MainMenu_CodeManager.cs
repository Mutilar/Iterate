using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_CodeManager : MonoBehaviour {
    public GameObject info;
    public Button[] buttons = new Button[5];
    public Button[] load_buttons = new Button[5];
    public Button[] settings_buttons = new Button[5];
    bool on_settings;
    bool on_loading;
    bool info_open;
    public Text title;
    public string title_text;
    public string loadLevel_text;
    bool loadLevel_tab;
    bool settings_tab;
    string settings_text;
    bool loading = true;
	// Use this for initialization
	void Start ()
    {
      
        info = GameObject.Find("INFO");
        info.SetActive(false);
        for (int i = 0; i < load_buttons.Length; i++)
        {
            load_buttons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < settings_buttons.Length; i++)
        {
            settings_buttons[i].gameObject.SetActive(false); 
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
        }
        //  TextAnimator.changeText(title.text, "", title, .75f, "scroll");
        //  title_text = title.text;
        //loadLevel_text = "<size=250>Load Class;</size>\n\nthing\nthing";
        // settings_text = "<size=250>Settings();</size>\n\nChangeFontSize(" + LevelManager.opposite_font_size + ");\nToggleVibrations(" + !LevelManager.vibrating + ");\nGetInfo();\nreturn;";
    }

    public bool UI_clickCheck(float xMin, float xMax, float yMin, float yMax, Vector2 mousePosition, Vector2 otherPosition)
    {
        if (mousePosition.x + xMin < otherPosition.x && mousePosition.x + xMax > otherPosition.x) if (mousePosition.y + yMin < otherPosition.y && mousePosition.y + yMax > otherPosition.y) return true;
        return false;
    }

    public void NewClass()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        if (!on_settings)
        {
            LevelManager.level = "NewClass";
            SceneManager.LoadScene("Coding");
        }
    }
    public void LoadClass()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        GameObject.Find("Title").transform.GetChild(0).gameObject.GetComponent<Text>().text = "Load();";
        for (int i = 0; i < load_buttons.Length; i++)
        {
            load_buttons[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
    }
    public void LoadReturn()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        GameObject.Find("Title").transform.GetChild(0).gameObject.GetComponent<Text>().text = "Iterate();";
        for (int i = 0; i < load_buttons.Length; i++)
        {
            load_buttons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
        }

    }
    public void Tutorial()
    {
    }
    public void LoadClass(string input)
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        LevelManager.level = input;
        SceneManager.LoadScene("Coding");
    }
    public void Settings()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        GameObject.Find("Title").transform.GetChild(0).gameObject.GetComponent<Text>().text = "Settings();";
        for (int i = 0; i < settings_buttons.Length; i++)
        {
            settings_buttons[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
    }
    public void FontSize()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        string switcher = LevelManager.font_size;
        LevelManager.font_size = LevelManager.opposite_font_size;
        LevelManager.opposite_font_size = switcher;
        settings_buttons[0].transform.GetChild(0).gameObject.GetComponent<Text>().text =  "ChangeFontSize(" + LevelManager.opposite_font_size + ");";
    }
    public void Vibrating()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        LevelManager.vibrating = !LevelManager.vibrating;
        settings_buttons[1].transform.GetChild(0).gameObject.GetComponent<Text>().text = "ToggleVibrations(" + !LevelManager.vibrating + ");";
    }
    public void GetInfo()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        info_open = true;
        info.SetActive(true);
        GameObject.Find("Title").transform.GetChild(0).gameObject.GetComponent<Text>().text = "GetInfo();";
        for (int i = 0; i < settings_buttons.Length; i++)
        {
            settings_buttons[i].gameObject.SetActive(false);
        }
    }
    public void SettingsReturn()
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        GameObject.Find("Title").transform.GetChild(0).gameObject.GetComponent<Text>().text = "Iterate();";
        for (int i = 0; i < settings_buttons.Length; i++)
        {
            settings_buttons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (info_open && Input.GetMouseButtonDown(0))
        {
            info_open = false;
           info.SetActive(false);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.SetActive(true);
            }
            if (LevelManager.vibrating) Vibration.Vibrate(10);
            GameObject.Find("Title").transform.GetChild(0).gameObject.GetComponent<Text>().text = "Iterate();";
        }
        if (false)
        {
            if (loading)
            {
                if (TextAnimator.areTasksRunning() == false) loading = false;
            }
            else
            {
                Vector2 mouse_pos = Input.mousePosition;
                string uncolored_text = title_text;
                if (loadLevel_tab == true) uncolored_text = loadLevel_text;
                if (settings_tab == true) uncolored_text = "<size=250>Settings();</size>\n\nChangeFontSize(" + LevelManager.opposite_font_size + ");\nToggleVibrations(" + !LevelManager.vibrating + ");\nGetInfo();\nreturn;";
                title.text = uncolored_text;
                for (int i = 0; i < 4; i++)
                {
                    if (UI_clickCheck(-600f, 600f, 0, 88f, mouse_pos, new Vector2(960, 595 - (105 * i))))
                    {
                        string[] splitting = uncolored_text.Split('\n');
                        string output = "";
                        splitting[i + 2] = "<color=#FF0000>" + splitting[i + 2] + "</color>";
                        for (int j = 0; j < splitting.Length; j++)
                        {
                            if (j == splitting.Length - 1) output += splitting[j];
                            else output += splitting[j] + "\n";
                        }
                        title.text = output;
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (UI_clickCheck(-600f, 600f, 0, 88f, mouse_pos, new Vector2(960, 595 - (105 * i))))
                        {
                            UI_clickOn(i);
                        }
                    }
                }
            }
        }
    }
    void UI_clickOn(int option)
    {
        if (LevelManager.vibrating) Vibration.Vibrate(10);
        switch (option)
        {
            case 0:
                if (settings_tab == false)
                {
                    LevelManager.level = "NewClass";
                    SceneManager.LoadScene("Coding");
                }
                else
                {
                    string switcher = LevelManager.font_size;
                    LevelManager.font_size = LevelManager.opposite_font_size;
                    LevelManager.opposite_font_size = switcher;
                    title.text = "<size=250>Settings();</size>\n\nChangeFontSize(" + LevelManager.opposite_font_size + ");\nToggleVibrations(" + !LevelManager.vibrating + ");\nGetInfo();\nreturn;";
                }
                    break;
            case 1:
                if (settings_tab == false)
                {
                    loadLevel_tab = true;
                    loading = true;
                    TextAnimator.changeText(loadLevel_text, title.text + "     ", title, 1f, "fade");
                }
                else
                {
                    LevelManager.vibrating = !LevelManager.vibrating;
                    title.text = "<size=250>Settings();</size>\n\nChangeFontSize(" + LevelManager.opposite_font_size + ");\nToggleVibrations(" + !LevelManager.vibrating + ");\nGetInfo();\nreturn;";
                }
                break;
            case 2:
                if (settings_tab == false)
                {
                    SceneManager.LoadScene("Coding");
                }
                else
                {
                    //contact information, fun facts, etc;
                }
                break;
            case 3:

                if (settings_tab == false)
                {
                    settings_tab = true;
                    loading = true;
                    TextAnimator.changeText("<size=250>Settings();</size>\n\nChangeFontSize(" + LevelManager.opposite_font_size + ");\nToggleVibrations(" + !LevelManager.vibrating + ");\nGetInfo();\nreturn;", title.text + "                                     ", title, 1f, "fade");
                }
                else
                {
                    settings_tab = false;
                    loading = true;
                    TextAnimator.changeText(title_text, title.text + "                                     ", title, 1f, "fade");
                }
                break;
        }
    }
}
