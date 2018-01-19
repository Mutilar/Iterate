using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;


public class CodeManager : MonoBehaviour
{
    /*** TODO:::     
        when deleting: give option to edit: precoded other options
        Curr:
        David: write up 10-20 example projects/exercises (#, title, 1 sentence desc, 2-4 comments)
        Editting:
        Brian: Clean up editor mode, add pretty lines/animations
    ***/

    Vector2 old_position;
    bool not_over_UI;
    float tap_duration = 0;


    /* MAIN TEXT DISPLAY */
    public GameObject Display_Text;



    
                    //public float text_height = 62.75f; // 62.75 for 75, 41.75 for 50,
                    public float text_tab_width = 105f;
    
    public Scrollbar UIElement_CompilationSpeed;
    /* Options Menu */
    public GameObject UIElement_OptionsMenu;
    bool UI_optionsMenuOpen;


    public Dropdown UIElement_LoadList;
    /* Compiler Menu */
    public GameObject UIElement_CompilationMenu;
    /* Console variables*/
    string Console_output = "Console:\n";
    bool on_console = true;
    public Text Output_text;
    public Text Debug_text;

    
    bool allowed_to_edit;
    bool editting_locked = true;
    public GameObject UIElement_EdittingLocker;
    public GameObject UIElement_EdittingMenu;
    bool editting_mode;
    int editting_tabAmount;
    int editting_line;
    string editting_line_type;
    string editting_variable_type;
    bool tapping_over_same_line;
    bool entering_string_literal;
    bool entering_string;

    public GameObject UIElement_InputField;

    public List<string> integers_within_editting_scope = new List<string>();
    public List<string> doubles_within_editting_scope = new List<string>();
    public List<string> booleans_within_editting_scope = new List<string>();
    public List<string> strings_within_editting_scope = new List<string>();
    string[] available_variable_types;

    public List<string> lines;

    string[] list_of_PDTs = { "boolean", "char", "double", "int", "String" };
    string[] list_of_evaluations = { "==", ">", "<", "<=", ">=", "!=" };
    //string[] list_of_operations = { " + ", " - ", " * ", " / ", " % "};
    string[] list_of_condensed_operations = { "=", "++", "--", "+=", "-=", "*=", "/=", "%=" };
    //public List<VariableObject<bool>> booleans = new List<VariableObject<bool>>();
    //public List<VariableObject<int>> integers = new List<VariableObject<int>>();
    //public List<VariableObject<double>> doubles = new List<VariableObject<double>>();
    /*public List<VariableObject<char>> characters = new List<VariableObject<char>>();*/
   // public List<VariableObject<string>> strings = new List<VariableObject<string>>();

    bool compiling = false;
    int current_line = -1; //points to current line of List<string> lines is being read
    
    bool next_else = false; //Used for if/else logic
    string loop_condition;
    string loop_variable_modifer;

    float time_delayer; //counter for compiling speed

    /* Initializing code (see compiler_reset as main initializer) */
    void Start()
    {
        /* LOAD CLASS FROM MAIN MENU */
        lines = new List<string>();
        //Display_LoadScript(LevelManager.level);
        /* INITIALIZE INTERPRETER */
        Display_pushText();
    }
    /* All code that requires running per frame (animation calls, compiling, button presses */
    void Update()
    {
        float text_height = 39.765f * Display_Text.transform.localScale.y;
      
        

        allowed_to_edit = true;
        if (UIElement_InputField.activeSelf == true) allowed_to_edit = false;
        //font size 50 = 41.75 pixels
        if (Input.GetMouseButtonDown(0))
        {
            old_position = Input.mousePosition;
            not_over_UI = true;
            if (UI_clickCheck(-1000, 200, -200, 0, old_position, UIElement_CompilationMenu.transform.position))
            {
                not_over_UI = false;
                if (UI_clickCheck(0, 200, -200, 0, old_position, UIElement_CompilationMenu.transform.position))
                {
                    UI_setColor(UIElement_CompilationMenu, new Color(1, 0, 0));
                    UI_setColor(UIElement_CompilationMenu.transform.GetChild(1).gameObject, new Color(1, 0, 0));
                }
            }
            /*Editting Locker*/
            if (UI_clickCheck(0, 200, -200, 0, old_position, UIElement_EdittingLocker.transform.position)) { not_over_UI = false; UI_setColor(UIElement_EdittingLocker, new Color(1, 0, 0)); Editor_toggleLock(); }
            /*Options Menu*/
            if (UI_clickCheck(0, 1000, 0, 200, old_position, UIElement_OptionsMenu.transform.position))
            { not_over_UI = false;
                if (UI_clickCheck(0, 200, 0, 200, old_position, UIElement_OptionsMenu.transform.position))
                {
                    UI_setColor(UIElement_OptionsMenu, new Color(1, 0, 0));
                    UI_setColor(UIElement_OptionsMenu.transform.GetChild(0).gameObject, new Color(1, 0, 0));
                    Display_OptionsMenu();
                }
            }
            if (editting_mode)
            {
                for (int i = 0; i < UIElement_EdittingMenu.transform.childCount; i++)
                {
                    if (UIElement_EdittingMenu.transform.GetChild(i).gameObject.activeSelf)
                    {
                        print(UIElement_EdittingMenu.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Text>().text);
                        if (UI_clickCheck(-562.5f, 0f, -50f, 50f, old_position, UIElement_EdittingMenu.transform.GetChild(i).transform.position))
                        {
                            not_over_UI = false;
                            Editor_selectOption(UIElement_EdittingMenu.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Text>().text);
                            //Display_pushText(lines, new int[0]);
                            //resize main thingy
                            print(lines[editting_line + 1]);
                            break;
                        }
                    }
                }
            }
            tapping_over_same_line = true;
        }
        if (Input.GetMouseButton(0))
        {
            if (tapping_over_same_line == true && allowed_to_edit && not_over_UI && !editting_mode && !UI_optionsMenuOpen && !compiling)
            {
                int line = (int)((Screen.height - Input.mousePosition.y + Display_Text.GetComponent<RectTransform>().anchoredPosition.y) / text_height);

                int old_pos = (int)((Screen.height - old_position.y) / text_height + 1);
                int new_pos = (int)((Screen.height - Input.mousePosition.y) / text_height + 1);
                if (old_pos != new_pos)
                {
                    tapping_over_same_line = false;
                }
                if (tap_duration > .25f)
                {
                    int line_of_main_method = 0;
                    while (lines[line_of_main_method].Contains("public static void") == false)
                    {
                        line_of_main_method++;
                    }
                    if (line > ++line_of_main_method && line < lines.Count - 1)
                    {
                        if (lines[line].Contains("}") == false && lines[line].Contains("{") == false)
                        {

                           // if (!compiling) Display_pushText(lines, new int[1] { line });
                        }
                        if (lines[line].IndexOf("if") == 0 || lines[line].IndexOf("while") == 0 || lines[line].IndexOf("for") == 0)
                        {
                            int matching_bracket = -1;
                            int line_of_bracket = 0;
                            for (int i = line; i < lines.Count; i++)
                            {
                                if (lines[i].IndexOf("{") == 0) matching_bracket++;
                                if (lines[i].IndexOf("}") == 0)
                                {
                                    if (matching_bracket == 0)
                                    {
                                        line_of_bracket = i;
                                        break;
                                    }
                                    matching_bracket--;
                                }
                            }
                            //Display_pushText(lines, new int[3] { line, line+1, line_of_bracket});
                        }
                    }
                    else
                    {
                        if (lines[line].Contains("public class"))
                        {
                            //Display_pushText(lines, new int[1] { line });
                        }
                    }
                }
            }
            else
            {
                //  Display_pushText(lines, new int[1] { current_line });//
               // if (!compiling)  Display_pushText(lines, new int[0]); ////////
            }
            tap_duration += Time.deltaTime;
            Vector2 input_position = Input.mousePosition;
            if (!editting_mode && not_over_UI)
            {
                UI_moveRectTransform(Display_Text.gameObject, input_position.x - old_position.x, input_position.y - old_position.y);
                /* Keeping scrolling within bounds */
                if (Display_Text.GetComponent<RectTransform>().anchoredPosition.x > 0) UI_setRectTransform(Display_Text, 0, Display_Text.GetComponent<RectTransform>().anchoredPosition.y);
                if (Display_Text.GetComponent<RectTransform>().anchoredPosition.y > text_height * lines.Count - 1080) UI_setRectTransform(Display_Text.gameObject, Display_Text.GetComponent<RectTransform>().anchoredPosition.x, text_height * lines.Count - 1080);
                if (Display_Text.GetComponent<RectTransform>().anchoredPosition.y < 0) UI_setRectTransform(Display_Text, Display_Text.GetComponent<RectTransform>().anchoredPosition.x, 0);
            }
            old_position = input_position;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (tap_duration < .25f && tapping_over_same_line)
            {
                //short tap
                if (allowed_to_edit && not_over_UI && !editting_mode && !UI_optionsMenuOpen && !compiling)
                {
                    int line = (int)((Screen.height - Input.mousePosition.y + Display_Text.GetComponent<RectTransform>().anchoredPosition.y) / text_height + 1);
                    int line_of_main_method = 0;
                    while (lines[line_of_main_method].Contains("public static void") == false)
                    {
                        line_of_main_method++;
                    }
                    if (line > ++line_of_main_method && line < lines.Count - 1)
                    {
                        if (!editting_locked)
                        {
                            UI_setColor(UIElement_EdittingLocker, new Color(1, 0, 0));
                        }
                        else
                        {
                            print(lines[line]);
                            if (lines[line - 1].IndexOf("for") == 0 || lines[line - 1].IndexOf("if") == 0 || lines[line - 1].IndexOf("while") == 0)
                            {
                                line ++;
                            }
                            Editor_start(line);
                        }
                    }
                }
               // Display_pushText(lines, new int[0]);
            }
            if (tap_duration > .25f && tapping_over_same_line)
            {
                if (allowed_to_edit && not_over_UI && !editting_mode && !UI_optionsMenuOpen && !compiling)
                {
                    int line = (int)((Screen.height - Input.mousePosition.y + Display_Text.GetComponent<RectTransform>().anchoredPosition.y) / text_height);
                    print(lines[line]);
                    if (lines[line].Contains("public class"))
                    {
                        if (!editting_locked)
                        {
                            UI_setColor(UIElement_EdittingLocker, new Color(1, 0, 0));
                          //  Display_pushText(lines, new int[0]);
                        }
                        else
                        {
                            //UIElement_EdittingMenu.SetActive(true);
                            Editor_setOptions(new string[] { "Enter the class's name:" });
                            editting_line_type = "SetClassName";
                        }
                    }
                    int line_of_main_method = 0;
                    while (lines[line_of_main_method].Contains("public static void") == false)
                    {
                        line_of_main_method++;
                    }
                    if (line > ++line_of_main_method && line < lines.Count - 1)
                    {
                        if (!editting_locked)
                        {
                            UI_setColor(UIElement_EdittingLocker, new Color(1, 0, 0));
                           // Display_pushText(lines, new int[0]);
                        }
                        else
                        {
                            if (lines[line].IndexOf("if") == 0 || lines[line].IndexOf("while") == 0 || lines[line].IndexOf("for") == 0)
                            {
                                lines.RemoveAt(line);
                                lines.RemoveAt(line);
                                int matching_bracket = 0;
                                for (int i = line; i < lines.Count; i++)
                                {
                                    if (lines[i].IndexOf("{") == 0)
                                    {
                                        matching_bracket++;
                                    }
                                    if (lines[i].IndexOf("}") == 0)
                                    {
                                        if (matching_bracket == 0)
                                        {
                                            lines.RemoveAt(i);
                                            break;
                                        }
                                        matching_bracket--;
                                    }
                                }
                            }
                            else if (lines[line].Contains("}") == false && lines[line].Contains("{") == false)
                            {
                                lines.RemoveAt(line);
                            }
                        }
                    }

                }
            }
            tap_duration = 0;
        }

        if (compiling)
        {
            UIElement_CompilationMenu.GetComponent<Image>().sprite = (Resources.Load("Sprites/Pause") as GameObject).GetComponent<SpriteRenderer>().sprite;
            UIElement_CompilationMenu.transform.GetChild(0).gameObject.SetActive(true);
            UIElement_CompilationMenu.transform.GetChild(1).gameObject.SetActive(true);
            UI_setTowardsRectTransform(UIElement_CompilationMenu, -800, 25);
            //Console stuff
            Output_text.text = Console_output;
            int height_of_console = Output_text.text.Split('\n').Length-1;
            if (Debug_text.text.Split('\n').Length - 1 > height_of_console) height_of_console = Debug_text.text.Split('\n').Length - 1;
            GameObject.Find("UIElement_Console").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameObject.Find("UIElement_Console").GetComponent<RectTransform>().anchoredPosition.x, height_of_console * 38f - 20);
            //Compiling timer
            time_delayer += Time.deltaTime;
            if (current_line == -1) current_line++;
            if (time_delayer >= Mathf.Pow(UIElement_CompilationSpeed.value*2, 2) && current_line < lines.Count)
            {
                while (lines[current_line].Contains("//"))
                {
                    current_line++;
                }
               // Display_pushText(lines, new int[1] { current_line });\

                Display_pushText();

                current_line++;
                time_delayer = 0;
            }
        }
        else
        {
            UIElement_CompilationMenu.GetComponent<Image>().sprite = (Resources.Load("Sprites/Start") as GameObject).GetComponent<SpriteRenderer>().sprite;
            UIElement_CompilationMenu.transform.GetChild(0).gameObject.SetActive(false);
            UIElement_CompilationMenu.transform.GetChild(1).gameObject.SetActive(false);
            UI_setTowardsRectTransform(UIElement_CompilationMenu, 0, 25);
        }
        if (UI_optionsMenuOpen)
        {
            UIElement_OptionsMenu.GetComponent<Image>().sprite = (Resources.Load("Sprites/SettingsOpen") as GameObject).GetComponent<SpriteRenderer>().sprite;
            UIElement_OptionsMenu.transform.GetChild(0).gameObject.SetActive(true);
            UI_setTowardsRectTransform(UIElement_OptionsMenu, -800, -25);
        }
        else
        {
            UIElement_OptionsMenu.GetComponent<Image>().sprite = (Resources.Load("Sprites/SettingsClose") as GameObject).GetComponent<SpriteRenderer>().sprite;
            UIElement_OptionsMenu.transform.GetChild(0).gameObject.SetActive(false);
            UI_setTowardsRectTransform(UIElement_OptionsMenu, 0, -25);
        }
        if (editting_mode)
        {
            UI_setTowardsRectTransform(Display_Text.gameObject, ((editting_tabAmount) * -1 * text_tab_width) - 100f, Display_Text.GetComponent<RectTransform>().anchoredPosition.y);
            UIElement_EdittingMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(lines[editting_line + 1].Length * 120f + 300f, 410f);
        }
        else
        {
           // UI_setTowardsRectTransform(Display_text.gameObject, 0, Display_text.GetComponent<RectTransform>().anchoredPosition.y);
        }


        UI_resetColor(UIElement_CompilationMenu, .025f);
        UI_resetColor(UIElement_CompilationMenu.transform.GetChild(1).gameObject, .025f);
        UI_resetColor(UIElement_OptionsMenu, .025f);
        UI_resetColor(UIElement_OptionsMenu.transform.GetChild(0).gameObject, .025f);
        UI_resetColor(UIElement_EdittingLocker, .025f);
        UI_resetColor(UIElement_EdittingMenu.transform.GetChild(3).gameObject, .025f);
        //UIElement_EdittingMenu.transform.GetChild(3).gameObject
    }
    /* Check for button press (manual button) */
    public bool UI_clickCheck(float xMin, float xMax, float yMin, float yMax, Vector2 mousePosition, Vector2 otherPosition)
    {
        if (mousePosition.x + xMin < otherPosition.x && mousePosition.x + xMax > otherPosition.x) if (mousePosition.y + yMin < otherPosition.y && mousePosition.y + yMax > otherPosition.y) return true;
        return false;
    }
    /* UI animation assisting methods */
    public void UI_moveRectTransform(GameObject item, float x, float y)
    {
        Vector2 pos = item.GetComponent<RectTransform>().anchoredPosition;
        item.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x + x, pos.y + y);
    }
    public void UI_setRectTransform(GameObject item, float x, float y)
    {
        item.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }
    public void UI_setTowardsRectTransform(GameObject item, float targetX, float targetY)
    {
        Vector2 pos = item.GetComponent<RectTransform>().anchoredPosition;
        if (Mathf.Abs(pos.x - targetX) < .1f && Mathf.Abs(pos.y - targetY) < .1f)
        {
            if (!(pos.x == targetX && pos.y == targetY)) UI_setRectTransform(item, targetX, targetY);
        }
        else
        {
            float style_smoothness = 10f;
            item.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x - (pos.x - targetX) / style_smoothness, pos.y - (pos.y - targetY) / style_smoothness);
        }
    }
    /* UI color animation assiting methods */
    public void UI_setColor(GameObject item, Color color)
    {
        item.GetComponent<Image>().color = color;
    }
    public void UI_resetColor(GameObject item, float animation_speed)
    {
        Color current = item.GetComponent<Image>().color;
        if (current.r < 1) current.r += animation_speed;
        if (current.r > 1) current.r = 1;
        if (current.g < 1) current.g += animation_speed;
        if (current.g > 1) current.g = 1;
        if (current.b < 1) current.b += animation_speed;
        if (current.b > 1) current.b = 1;
        item.GetComponent<Image>().color = current;
    }
    /* Editor initializing method */
    public void Editor_start(int line)
    {
        editting_line = line;
        editting_mode = true;
        editting_tabAmount = 0;

        for (int i = 0; i < line; i++)
        {
            if (lines[i].IndexOf("{") == 0)
            {
                editting_tabAmount++;
            }
            if (lines[i].IndexOf("}") == 0)
            {
                editting_tabAmount--;
            }
        }
        print(editting_tabAmount);
        lines.Insert(line, "");
        lines.Insert(line, " ");
        lines.Insert(line, "");

        UIElement_EdittingMenu.SetActive(true);
        UI_setRectTransform(UIElement_EdittingMenu, -5, -Screen.height / 2);//(line + 1) * -41.75f + 12f);
        //UI_setRectTransform(Display_Text.gameObject,0, (line + 1) * text_height - 511);
        editting_line_type = "";
        Editor_findVariablesWithinScope(line);
        //old:: Editor_setOptions(new string[] { "Keyword", "Method", "Variable" });
        Editor_setOptions(new string[] { "Create Variable", "Modify Variable", "Flow Control", "Method" });
    }
    /* Editor closing method */
    public void Editor_end()
    {
        editting_line = -1;
        editting_mode = false;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] == "")
            {
                lines.RemoveAt(i);
            }
        }
        editting_line_type = "";
        UIElement_EdittingMenu.SetActive(false);
    }
    /* Sketchy code: does typing/entering of code into program from user input */
    public void Editor_findVariablesWithinScope(int line)
    {
        booleans_within_editting_scope = new List<string>();
        integers_within_editting_scope = new List<string>();
        doubles_within_editting_scope = new List<string>();
        strings_within_editting_scope = new List<string>();
        for (int i = 0; i < line; i++)
        {
            for (int types_of_PDTs = 0; types_of_PDTs < list_of_PDTs.Length; types_of_PDTs++)
            {
                if (lines[i].Contains(list_of_PDTs[types_of_PDTs] + " "))
                {
                    if (lines[i].Contains("for (" + list_of_PDTs[types_of_PDTs] + " ") || lines[i].IndexOf(list_of_PDTs[types_of_PDTs] + " ") == 0)
                    {
                        string name = lines[i].Substring(lines[i].IndexOf(list_of_PDTs[types_of_PDTs] + " ") + list_of_PDTs[types_of_PDTs].Length + 1);
                        if (name.IndexOf(" ") < name.IndexOf(";")) name = name.Remove(name.IndexOf(" "));
                        else name = name.Remove(name.IndexOf(";"));
                        switch (types_of_PDTs)
                        {
                            case 0: //Add new integer definition to list
                                booleans_within_editting_scope.Add(name);
                                break;
                            case 2: //Add new double definition to list
                                doubles_within_editting_scope.Add(name);
                                break;
                            case 3: //Add new integer definition to list
                                integers_within_editting_scope.Add(name);
                                break;
                            case 4: //Add new string definition to list
                                strings_within_editting_scope.Add(name);
                                break;
                        }
                    }
                }
            }
        }
        List<string> availables = new List<string>();
        if (integers_within_editting_scope.Count != 0) availables.Add("Integer");
        if (doubles_within_editting_scope.Count != 0) availables.Add("Double");
        if (booleans_within_editting_scope.Count != 0) availables.Add("Boolean");
        if (strings_within_editting_scope.Count != 0) availables.Add("String");
        available_variable_types = availables.ToArray();
    }
    public void Editor_setOptions(string[] options)
    {
        Editor_setOptionPositions(options);
        entering_string_literal = false;
        entering_string = false;
        for (int i = 0; i < options.Length; i++)
        {
            if (i == 0)
            {
                if (options[i] == "Enter a name:" || options[i] == "Enter a string:" || options[i] == "Enter the class's name:")
                {
                    entering_string = true;
                    if (options[i] == "Enter a string:") entering_string_literal = true;
                    UIElement_InputField.SetActive(true);
                    UIElement_InputField.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
                    UIElement_InputField.GetComponent<InputField>().placeholder.GetComponent<Text>().text = options[i];
                    UIElement_EdittingMenu.transform.GetChild(i).gameObject.SetActive(false);
                }
                else if (options[i] == "Enter a number:")
                {
                    UIElement_InputField.SetActive(true);
                    if (editting_variable_type == "Integer") UIElement_InputField.GetComponent<InputField>().contentType = InputField.ContentType.IntegerNumber;
                    else if (editting_variable_type == "Double") UIElement_InputField.GetComponent<InputField>().contentType = InputField.ContentType.DecimalNumber;
                    UIElement_InputField.GetComponent<InputField>().placeholder.GetComponent<Text>().text = options[i];
                    UIElement_EdittingMenu.transform.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    UIElement_InputField.SetActive(false);
                    Editor_setOption(i, options[i]);
                    UIElement_EdittingMenu.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
            else
            {
                Editor_setOption(i, options[i]);
                UIElement_EdittingMenu.transform.GetChild(i).gameObject.SetActive(true);

            }
        }
        for (int i = options.Length; i < UIElement_EdittingMenu.transform.childCount; i++)
        {
            UIElement_EdittingMenu.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void Editor_setOption(int option, string text)
    {
        UIElement_EdittingMenu.transform.GetChild(option).GetChild(0).gameObject.GetComponent<Text>().text = text;
    }
    public void Editor_finishTyping()
    {
        string input = UIElement_InputField.GetComponent<InputField>().text;
        if (entering_string)
        {
            input = input.Replace(' ', '_');

            if (entering_string_literal)
            {
                input = "\"" + input + "\"";
            }
        }
        //UIElement_EdittingMenu.transform.GetChild(0).GetChild(1).gameObject.GetComponent<InputField>().text;
        UIElement_InputField.GetComponent<InputField>().text = "";
        UIElement_InputField.SetActive(false);
            //UIElement_EdittingMenu.transform.GetChild(0).GetChild(1).gameObject.GetComponent<InputField>().text = "";
        if (editting_line_type == "SetClassName")
        {
            int line = 0;
            while (lines[line].Contains("public class") == false)
            {
                line++;
            }
            lines[line] = "public class " + input;
           // Display_pushText(lines, new int[0]);

        }
        if (editting_line_type.Contains("rint"))
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";
            Editor_setOptions(new string[] { "Finish" });
        }
        if (editting_line_type.Contains("If"))
        {
            if (editting_line_type == "IfLeftSide") Editor_setOptions(new string[] { "Comparison" });
            else Editor_setOptions(new string[] { "Finish" });

            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";

        }
        if (editting_line_type.Contains("Modify"))
        {
            if (editting_line_type.IndexOf("ForModify") == 0)
            {
                lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";
                Editor_setOptions(new string[] { "Finish" });

            }
            else if (editting_line_type.Contains("For"))
            {
                lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";
                Editor_setOptions(new string[] { "Modifier" });
            }
            else
            {
                if (editting_line_type == "ModifyString")
                {
                    lines[editting_line + 1] += input;
                }
                else lines[editting_line + 1] += input;
                Editor_setOptions(new string[] { "Finish" });
            }
        }

        if (editting_line_type == "ForSetVariable")
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";
            editting_line_type = "ForSetVariableModify" + editting_variable_type;

            Editor_setOptions(new string[] { "Comparison" });

        }

        string[] options;
        if (editting_line_type == "CreateInteger" || (editting_line_type == "ForNameVariable" && editting_variable_type == "Integer"))
        {
            if (editting_line_type == "ForNameVariable")
            {
                lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + " = )";
                editting_line_type = "ForSetVariable";
                integers_within_editting_scope.Add(input);
            }
            else
            {
                lines[editting_line + 1] += input + " = ";
                editting_line_type = "ModifyInteger";
            }
            if (integers_within_editting_scope.Count != 0)
            {
                options = new string[integers_within_editting_scope.Count + 1];
                options[0] = "Number";
                //options[1] = "Method";
                for (int i = 0; i < integers_within_editting_scope.Count; i++)
                {
                    options[i+1] = integers_within_editting_scope[i];
                }
                Editor_setOptions(options);
            }
            else Editor_setOptions(new string[] { "Number" });
        }
        if (editting_line_type == "CreateDouble" || (editting_line_type == "ForNameVariable" && editting_variable_type == "Double"))
        {
            if (editting_line_type == "ForNameVariable")
            {
                lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + " = )";
                editting_line_type = "ForSetVariable";
                doubles_within_editting_scope.Add(input);
            }
            else
            {
                lines[editting_line + 1] += input + " = ";
                editting_line_type = "ModifyDouble";
            }
            if (doubles_within_editting_scope.Count != 0)
            {
                options = new string[doubles_within_editting_scope.Count + 2];
                options[0] = "Number";
                options[1] = "Method";
                for (int i = 0; i < doubles_within_editting_scope.Count; i++)
                {
                    options[i + 2] = doubles_within_editting_scope[i];
                }
                Editor_setOptions(options);
            }
            else Editor_setOptions(new string[] {"Method", "Number" });
        }
        if (editting_line_type == "CreateBoolean")
        {
            editting_line_type = "ModifyBoolean";
            lines[editting_line + 1] += input + " = ";

            if (booleans_within_editting_scope.Count != 0) Editor_setOptions(new string[] { "Variable", "True", "False" });
            else Editor_setOptions(new string[] { "True", "False" });
        }
        if (editting_line_type == "CreateString")
        {
            editting_line_type = "ModifyString";
            lines[editting_line + 1] += input + " = ";

            if (strings_within_editting_scope.Count != 0) Editor_setOptions(new string[] { "Variable", "String Literal" });
            else Editor_setOptions(new string[] { "String Literal" });
        }
       // Display_pushText(lines, new int[0]);

    }
    public void Editor_selectOption(string selection)
    {

        switch (selection)
        {
            case "Flow Control":
                if (editting_line_type == "") { Editor_setOptions(new string[] { "If", "Else", "While", "For" }); editting_line_type = selection; }
                break;
            case "Method":
                if (editting_line_type == "")
                {
                    //voids
                    editting_line_type = selection; lines[editting_line + 1] = selection;
                    Editor_setOptions(new string[] { "System", "Iterate", });
                }
                else
                {
                    //numbers (iterate == temperature probe)
                    Editor_setOptions(new string[] { "Math", "Iterate", });
                }
                break;
            case "Create Variable":
                editting_line_type = "CreateVariable";
                Editor_setOptions(new string[] { "Integer", "Double", "Boolean", "String" });
                break;
            case "Modify Variable":
                editting_line_type = "ModifyVariable";
                if (available_variable_types.Length > 0) Editor_setOptions(available_variable_types);
                else
                {
                    UI_setColor(UIElement_EdittingMenu.transform.GetChild(1).gameObject, new Color(1, 0, 0)); 
                    print("no variables");
                }
                break;




            case "Variable":
                if (editting_line_type == "") { Editor_setOptions(available_variable_types); editting_line_type = "ModifyVariable"; }
                if (editting_line_type == "Keyword") { Editor_setOptions(new string[] { "Integer", "Double", "Boolean", "String" }); editting_line_type = "CreateVariable"; }
                if (editting_line_type == "If") { Editor_setOptions(available_variable_types); editting_line_type = "IfLeftSide";}
                if (editting_line_type == "IfLeftSide") { Editor_setOptions(available_variable_types); editting_line_type = "IfLeftSide"; }
                if (editting_line_type == "IfRightSide") { Editor_setOptions(available_variable_types); editting_line_type = "IfRightSide"; }
                if (editting_line_type == "Print") { Editor_setOptions(available_variable_types); }
                break;
            case "Conditional":
                if (editting_line_type == "Keyword") { editting_line_type = selection; lines[editting_line + 1] = selection; }
                Editor_setOptions(new string[] { "If", "Else" });
                break;
            case "Loop":
                if (editting_line_type == "Keyword") editting_line_type = selection;
                Editor_setOptions(new string[] { "While Loop", "For Loop" });
                break;
            case "System":
                lines[editting_line + 1] = selection;
                Editor_setOptions(new string[] { "Print", "Print Line" });
                break;
            case "Iterate":
                lines[editting_line + 1] = selection;
                Editor_setOptions(new string[] { "Iterate();", "ClearIterations();" } );
                break;
            case "Iterate();":
                lines[editting_line + 1] = selection;
              //  Editor_setOptions(new string[] { "Iterate(Phone.Vibrate);" });
                break;
            case "ClearIterations();":
                lines[editting_line + 1] = selection;
                Editor_setOptions(new string[] { "Finish" });
                break;
            case "Math":
                Editor_setOptions(new string[] { "Random Value"});
                break;
            case "Random Value":
                if (editting_line_type.Contains("If") || editting_line_type.Contains("Print"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + "Math.random())";
                }
                else
                {
                    lines[editting_line + 1] += "Math.random()";
                }
                Editor_setOptions(new string[] { "Finish" });
                break;
            case "Number":
                Editor_setOptions(new string[] { "Enter a number:" });
                break;
            case "String Literal":
                Editor_setOptions(new string[] { "Enter a string:" });
                break;
            case "Integer":
                if (editting_line_type == "CreateVariable") { Editor_setOptions(new string[] { "Enter a name:" }); lines[editting_line + 1] = "int "; editting_line_type = "CreateInteger"; }
                else if (editting_line_type == "For") { Editor_setOptions(new string[] { "Enter a name:" }); lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + "int )"; editting_line_type = "ForNameVariable"; }
                else Editor_setOptions(integers_within_editting_scope.ToArray());
                editting_variable_type = "Integer";
                break;
            case "Double":
                if (editting_line_type == "CreateVariable") { Editor_setOptions(new string[] { "Enter a name:" }); lines[editting_line + 1] = "double "; editting_line_type = "CreateDouble"; }
                else if (editting_line_type == "For") { Editor_setOptions(new string[] { "Enter a name:" }); lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + "double )"; editting_line_type = "ForNameVariable"; }
                else Editor_setOptions(doubles_within_editting_scope.ToArray());
                editting_variable_type = "Double";
                break;
            case "Boolean":
                if (editting_line_type == "CreateVariable") { Editor_setOptions(new string[] { "Enter a name:" }); lines[editting_line + 1] = "boolean "; editting_line_type = "CreateBoolean"; }
                else Editor_setOptions(booleans_within_editting_scope.ToArray());
                editting_variable_type = "Boolean";
                break;
            case "String":
                if (editting_line_type == "CreateVariable") { Editor_setOptions(new string[] { "Enter a name:" }); lines[editting_line + 1] = "String "; editting_line_type = "CreateString"; }
                else Editor_setOptions(strings_within_editting_scope.ToArray());
                editting_variable_type = "String";
                break;
            case "True":
                if (editting_line_type.Contains("If"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + "true)";
                }
                else lines[editting_line + 1] += "true";
                Editor_setOptions(new string[] { "Finish" });
                break;
            case "False":
                if (editting_line_type.Contains("If"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + "false)";
                }
                else lines[editting_line + 1] += "false";
                Editor_setOptions(new string[] { "Finish" });
                break;
            case "If":
                editting_line_type = "If";
                lines[editting_line + 1] = "if ()";
                Editor_setOptions(new string[] { "Variable", "Method", "String Literal", "Number" });
                break;
            case "While":
                editting_line_type = "If";
                Editor_setOptions(new string[] { "Variable", "Method", "String Literal", "Number" });
                lines[editting_line + 1] = "while ()";
                break;
            case "For":
                editting_line_type = "For";
                Editor_setOptions(new string[] { "Integer", "Double" });
                lines[editting_line + 1] = "for ()";
                break;
            //case "Operator":
            //    if (editting_variable_type == "String") Editor_setOptions(new string[] { " + " });
            //    else Editor_setOptions(list_of_operations);
            //    break;
            case "Comparison":
                if (editting_variable_type == "Boolean" || editting_variable_type == "String") Editor_setOptions(new string[] { "==", "!=" });
                else if (editting_line_type.Contains("ForSet"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + "; )";
                    editting_line_type = "ForComparisonModify" + editting_variable_type;
                    Editor_findPossibleValuesForVariables();
                }
                else Editor_setOptions(list_of_evaluations);
                break;
            case "Print":
                editting_line_type = "Print";
                lines[editting_line + 1] = "System.out.print()";
                Editor_setOptions(new string[] { "Variable", "String Literal", "Number" });
                break;
            case "Print Line":
                editting_line_type = "Print";
                lines[editting_line + 1] = "System.out.println()";
                Editor_setOptions(new string[] { "Variable", "String Literal", "Number" });
                break;
            case "Modifier":
                lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + "; )";
                editting_line_type = "ForModify" + editting_variable_type;
                if (editting_variable_type == "Integer")
                {
                    Editor_setOptions(integers_within_editting_scope.ToArray());
                }
                if (editting_variable_type == "Double")
                {
                    Editor_setOptions(doubles_within_editting_scope.ToArray());
                }
                break;
            case "Finish":
                if (editting_line_type.IndexOf("Modify") == 0 || editting_line_type == "Print") lines[editting_line + 1] += ";";
                if (editting_line_type.Contains("If") || editting_line_type.Contains("For") || editting_line_type.Contains("While"))
                {    lines.Insert(editting_line + 2, "{"); lines.Insert(editting_line + 3, "}"); }
                Editor_end();
                break;
        }

        for (int i = 0; i < integers_within_editting_scope.Count; i++)
        {
            if (selection == integers_within_editting_scope[i])
            {
                Editor_usedAVariable(selection);
            }
        }
        for (int i = 0; i < doubles_within_editting_scope.Count; i++)
        {
            if (selection == doubles_within_editting_scope[i])
            {
                Editor_usedAVariable(selection);
            }
        }
        for (int i = 0; i < booleans_within_editting_scope.Count; i++)
        {
            if (selection == booleans_within_editting_scope[i])
            {
                Editor_usedAVariable(selection);
            }
        }
        for (int i = 0; i < strings_within_editting_scope.Count; i++)
        {
            if (selection == strings_within_editting_scope[i])
            {
                Editor_usedAVariable(selection);
            }
        }
        for (int i = 0; i < list_of_evaluations.Length; i++)
        {
            if (selection == list_of_evaluations[i])
            {
                if (editting_line_type == "IfLeftSide")
                {
                    editting_line_type = "IfRightSide";
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + " " + selection + " )";
                    if (editting_variable_type == "Boolean")
                    {
                        if (booleans_within_editting_scope.Count != 0) Editor_setOptions(new string[] { "Variable", "True", "False" });
                        else Editor_setOptions(new string[] { "True", "False" });
                    }
                    else if (editting_variable_type == "String")
                    {
                        if (strings_within_editting_scope.Count != 0) Editor_setOptions(new string[] { "Variable", "String Literal" });
                        else Editor_setOptions(new string[] { "String Literal" });
                    } 
                    else Editor_setOptions(new string[] { "Variable", "Method", "Number" });
                }
                if (editting_line_type.Contains("For"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + " " + selection + " )";
                    Editor_setOptions(new string[] { "Variable", "Method", "Number" });

                }
            }
        }
        for (int i = 0; i < list_of_condensed_operations.Length; i++)
        {
            if (selection == list_of_condensed_operations[i])
            {
                if (editting_line_type.Contains("For"))
                {
                    if (selection == "++" || selection == "--")
                    {
                        lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1)  + selection + ")";
                        Editor_setOptions(new string[] { "Finish" });
                    }
                    else
                    {
                        lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + " " + selection + " )";

                        Editor_findPossibleValuesForVariables();
                    }
                }
                else
                {
                    if (selection == "++" || selection == "--")
                    {
                        lines[editting_line + 1] += selection;
                        Editor_setOptions(new string[] { "Finish" });
                    }
                    else
                    {   
                        lines[editting_line + 1] += " " + selection + " ";
                        Editor_findPossibleValuesForVariables();
                    }
                }
            }
        }
       
       
    }
    public void Editor_findPossibleValuesForVariables()
    {
        string[] options;
        if (editting_line_type.Contains("ModifyInteger"))
        {
            if (integers_within_editting_scope.Count != 0)
            {
                options = new string[integers_within_editting_scope.Count + 1];
                options[0] = "Number";
                //options[1] = "Method";
                for (int j = 0; j < integers_within_editting_scope.Count; j++)
                {
                    options[j + 1] = integers_within_editting_scope[j];
                }
                Editor_setOptions(options);
            }
            else Editor_setOptions(new string[] { "Number" });
        }
        if (editting_line_type.Contains("ModifyDouble"))
        {
            if (doubles_within_editting_scope.Count != 0)
            {
                options = new string[doubles_within_editting_scope.Count + 2];
                options[0] = "Number";
                options[1] = "Method";
                for (int j = 0; j < doubles_within_editting_scope.Count; j++)
                {
                    options[j + 2] = doubles_within_editting_scope[j];
                }
                Editor_setOptions(options);
            }
            else Editor_setOptions(new string[] { "Method", "Number" });
        }
        if (editting_line_type == "ModifyBoolean")
        {
            if (booleans_within_editting_scope.Count != 0) Editor_setOptions(new string[] { "Variable", "True", "False" });
            else Editor_setOptions(new string[] { "True", "False" });
        }
        if (editting_line_type == "ModifyString")
        {
            if (strings_within_editting_scope.Count != 0) Editor_setOptions(new string[] { "Variable", "String Literal" });
            else Editor_setOptions(new string[] { "String Literal" });
        }
    }
    public void Editor_usedAVariable(string selection)
    {

        if (editting_line_type == "IfLeftSide")
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";
            print(editting_variable_type);
            if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "Comparison" });
            else Editor_setOptions(new string[] { "Comparison" });
        }
        if (editting_line_type == "IfRightSide")
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";
            if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "Finish" });
            else Editor_setOptions(new string[] { "Finish" });
        }
        if (editting_line_type == "Print")
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";
            if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "Finish" });
            else Editor_setOptions(new string[] {"Finish" });
        }
        if (editting_line_type.Contains("Modify"))
        {
            if (editting_line_type != "ModifyVariable")
            {
                if (editting_line_type.Contains("For"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";
                    Editor_setOptions(new string[] { "Comparison" });
                }
                else
                {
                    lines[editting_line + 1] += selection;
                    if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "Finish" });
                    else Editor_setOptions(new string[] { "Finish" });
                }
            }
            //this one is below cuz it would otherwise trigger both loops ;)
            if (editting_line_type == "ModifyVariable")
            {
                editting_line_type = "Modify" + editting_variable_type;
                lines[editting_line + 1] = selection;
                if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "=" });
                else if (editting_variable_type == "String") Editor_setOptions(new string[] { "=", "+=" });
                else Editor_setOptions(list_of_condensed_operations);
                print("yes");
            }
        }
        if (editting_line_type.Contains("ForModify"))
        {
            Editor_setOptions(list_of_condensed_operations);
        }
    }
    public void Editor_setOptionPositions(string[] options)
    {
        int spacing = 220;
        int space = spacing - spacing * options.Length % 2, count = 0;
        for (int i = options.Length / 2; i >= -options.Length / 2 + (options.Length + 1) % 2; i--)
        {
            if (options.Length % 2 == 1)   UIElement_EdittingMenu.transform.GetChild(count).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, i * (spacing*2) - space + spacing);
            else UIElement_EdittingMenu.transform.GetChild(count).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, i * (spacing*2) - space);
            count++;
        }
    }
    /* Lock to prevent accidental editting of code */
    public void Editor_toggleLock()
    {
        editting_locked = !editting_locked;
        UIElement_EdittingLocker.GetComponent<Image>().sprite = (Resources.Load("Sprites/editting" + editting_locked) as GameObject).GetComponent<SpriteRenderer>().sprite;
    }
   



    void Display_pushText()
    {
       // Display_Text.GetComponent<TextMeshProUGUI>().text = Interpreter_Object.ToString();
    }

    public void Display_pushConsoleText(string text)
    {
        Console_output += text;
        if (Console_output.Split('\n').Length >= 8)
        {
            Console_output = Console_output.Substring(Console_output.IndexOf('\n') + 1);
            Console_output = Console_output.Substring(Console_output.IndexOf('\n') + 1);
            Console_output = Console_output.Substring(Console_output.IndexOf('\n') + 1);
            Console_output = "Console:\n...\n" + Console_output;
        }
    }
    
    /* Display UI */
    public void Display_OptionsMenu()
    {
        UI_optionsMenuOpen = !UI_optionsMenuOpen;
    }
    public void Display_LoadScript(string name)
    {
        // print(UIElement_OptionsMenu.transform.GetChild(0).gameObject.GetComponent<Dropdown>().value);
        if (name == "") name = UIElement_LoadList.options[UIElement_LoadList.value].text;
        if (name == "NewClass")
        {
            //CODE FOR ASKING FOR NAME OF CLASS
            Editor_setOptions(new string[] { "Enter the class's name:" });
            editting_line_type = "SetClassName";
        }
        print(name);
        TextAsset textFile = Resources.Load(name) as TextAsset;
        string[] lines = textFile.text.Split('\n');
 
        this.lines = new List<string>();
        for (int i = 0; i < lines.Length; i++)
        {
            this.lines.Add(lines[i]);             
        }

       
        Display_pushText();




        Display_Text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }
    /* Back to main menu */
    public void Misc_ReturnToMain()
    {
        SceneManager.LoadScene("Menu");
    }  

}