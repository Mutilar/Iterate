using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;


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


    /* INTERPRETER OBJECT */
    public Interpreter Interpreter_Object;

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
    string[] list_of_operations = { " + ", " - ", " * ", " / ", " % "};
    string[] list_of_condensed_operations = { "=", "++", "--", "+=", "-=", "*=", "/=", "%=" };
    public List<VariableObject<bool>> booleans = new List<VariableObject<bool>>();
    public List<VariableObject<int>> integers = new List<VariableObject<int>>();
    public List<VariableObject<double>> doubles = new List<VariableObject<double>>();
    /*public List<VariableObject<char>> characters = new List<VariableObject<char>>();*/
    public List<VariableObject<string>> strings = new List<VariableObject<string>>();

    public Stack<TaskObject> tasks = new Stack<TaskObject>();

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
        Display_LoadScript(LevelManager.level);
        /* INITIALIZE INTERPRETER */
        Interpreter_Object = new Interpreter();
        Interpreter_Object.addLines(lines);
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
                    Compiler_compilation();
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
            Debug_text.text = Compiler_displayValues();
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
               // Display_pushText(lines, new int[1] { current_line });
                Compiler_compile(lines[current_line]);

                Interpreter_Object.current_line = current_line;
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
            if (editting_line_type == "IfLeftSide") Editor_setOptions(new string[] { "Operator", "Comparison" });
            else Editor_setOptions(new string[] { "Operator", "Finish" });

            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";

        }
        if (editting_line_type.Contains("Modify"))
        {
            if (editting_line_type.IndexOf("ForModify") == 0)
            {
                lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";
                Editor_setOptions(new string[] { "Operator", "Finish" });

            }
            else if (editting_line_type.Contains("For"))
            {
                lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";
                Editor_setOptions(new string[] { "Operator", "Modifier" });
            }
            else
            {
                if (editting_line_type == "ModifyString")
                {
                    lines[editting_line + 1] += input;
                }
                else lines[editting_line + 1] += input;
                Editor_setOptions(new string[] { "Operator", "Finish" });
            }
        }

        if (editting_line_type == "ForSetVariable")
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + input + ")";
            editting_line_type = "ForSetVariableModify" + editting_variable_type;

            Editor_setOptions(new string[] { "Operator", "Comparison" });

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
                    UI_setColor(UIElement_EdittingMenu.transform.GetChild(3).gameObject, new Color(1, 0, 0)); 
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
            case "Operator":
                if (editting_variable_type == "String") Editor_setOptions(new string[] { " + " });
                else Editor_setOptions(list_of_operations);
                break;
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

        for (int i = 0; i < list_of_operations.Length; i++)
        {
            if (selection == list_of_operations[i])
            {
                if (editting_line_type == "IfLeftSide" || editting_line_type == "IfRightSide" || editting_line_type == "Print" || editting_line_type.Contains("For"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";

                    Editor_setOptions(new string[] { "Variable", "Method", "String Literal", "Number" });

                    if (editting_line_type.Contains("Modify"))
                    {
                        Editor_findPossibleValuesForVariables();
                    }

                }
                else if (editting_line_type.Contains("Modify"))
                {
                    lines[editting_line + 1] += list_of_operations[i];
                    Editor_findPossibleValuesForVariables();
                }
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
            else Editor_setOptions(new string[] { "Operator", "Comparison" });
        }
        if (editting_line_type == "IfRightSide")
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";
            if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "Finish" });
            else Editor_setOptions(new string[] { "Operator", "Finish" });
        }
        if (editting_line_type == "Print")
        {
            lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";
            if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "Finish" });
            else Editor_setOptions(new string[] { "Operator", "Finish" });
        }
        if (editting_line_type.Contains("Modify"))
        {
            if (editting_line_type != "ModifyVariable")
            {
                if (editting_line_type.Contains("For"))
                {
                    lines[editting_line + 1] = lines[editting_line + 1].Substring(0, lines[editting_line + 1].Length - 1) + selection + ")";
                    Editor_setOptions(new string[] { "Operator", "Comparison" });
                }
                else
                {
                    lines[editting_line + 1] += selection;
                    if (editting_variable_type == "Boolean") Editor_setOptions(new string[] { "Finish" });
                    else Editor_setOptions(new string[] { "Operator", "Finish" });
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
    /* Substring assisting method */
    string Parser_splitStringBetween(string line, string side1, string side2)
    {
        if (side2 == ")")
        {
            string output = line.Substring(line.IndexOf(side1) + side1.Length);
            int scope_counter = 0; //for any "(", ignore the following ")"
            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] == '(')
                {
                    scope_counter++;
                }
                if (output[i] == ')')
                {
                    if (scope_counter == 0)
                    {
                        output = output.Remove(i);
                        return output;
                    }
                    scope_counter--;
                }
            }
        }
        else
        {
            string output = line.Substring(line.IndexOf(side1) + side1.Length);
            output = output.Remove(output.IndexOf(side2));
            return output;
        }
        return "BROKEN";
    }
    string Compiler_displayValues()
    {
        string output = "";
        if (booleans.Count > 0)
        {
            output += "Booleans:\n";
            for (int i = 0; i < booleans.Count; i++)
            {
                output += "\t" + booleans[i].getKey() + ": " + booleans[i].getValue() + "\n";
            }
        }
        if (integers.Count > 0)
        {
            output += "Integers:\n";
            for (int i = 0; i < integers.Count; i++)
            {
                output += "\t" + integers[i].getKey() + ": " + integers[i].getValue() + "\n";
            }
        }
        if (doubles.Count > 0)
        {
            output += "Doubles:\n";
            for (int i = 0; i < doubles.Count; i++)
            {
                output += "\t" + doubles[i].getKey() + ": " + doubles[i].getValue() + "\n";
            }
        }
      /*  if (characters.Count > 0)
        {
            output += "Characters:\n";
            for (int i = 0; i < characters.Count; i++)
            {
                output += "\t" + characters[i].getKey() + ": " + characters[i].getValue() + "\n";
            }
        }*/
        if (strings.Count > 0)
        {
            output += "Strings:\n";
            for (int i = 0; i < strings.Count; i++)
            {
                output += "\t" + strings[i].getKey() + ": " + strings[i].getValue() + "\n";
            }
        }
        return output;
    }
    /* I forget what this does */
    string Compiler_parseManipulator(string line)
    {
        string manipulation = line.Substring(line.IndexOf(" ") + 1);
        manipulation = manipulation.Remove(manipulation.IndexOf(" "));
        return manipulation;
    }
    string Compiler_parseManipulationValue(string line)
    {
        string manipulation = Compiler_parseManipulator(line);
        string value = line.Substring(line.IndexOf(manipulation) + manipulation.Length + 1);
        value = value.Remove(value.IndexOf(";"));
        return value;
    }
    /* Finds brackets, deals with scope */
    int Compiler_findNextInstanceOf(int starting_line, string bracket_type)
    {
        int ending_line = starting_line;
        while (lines[ending_line].IndexOf(bracket_type) == -1)
        {
            ending_line++;
        }
        return ending_line;
    }
    int Compiler_findMatchingBracket(int starting_line)
    {
        int ending_line = starting_line;
        int number_of_internal_brackets = 0;

        bool found = false;

        while (!found)
        {
            ending_line++;
            if (lines[ending_line].IndexOf("{") == 0)
            {
                number_of_internal_brackets++;
            }
            if (lines[ending_line].IndexOf("}") == 0 && number_of_internal_brackets != 0)
            {
                number_of_internal_brackets--;
            }
            if (lines[ending_line].IndexOf("}") == 0 && number_of_internal_brackets == 0)
            {
                found = true;
            }
            
        }
        return ending_line;
    }
    /* Creates variables with a given name and type */
    void Compiler_addVariable(string line)
    {
        string name = "";
        string value = "";
        if (Compiler_findVariable(Parser_splitStringBetween(line," "," ")) != "N/A")
        {
            print("modified" + Parser_splitStringBetween(line, " ", ";") + ";");
            Compiler_modifyVariable(Parser_splitStringBetween(line, " ", ";") + ";");//Compiler_evaluateExpression(Parser_splitStringBetween(line, " ", ";") + ";"));
        
        }
        else
        {
            for (int types_of_PDTs = 0; types_of_PDTs < list_of_PDTs.Length; types_of_PDTs++)
            {
                //DECLARING A NEW VARIABLE OF TYPE (list_of_PDTs[types_of_PDTs])
                if (line.IndexOf(list_of_PDTs[types_of_PDTs]) == 0)
                {
                    //Isolate name from line of code
                    name = Parser_splitStringBetween(line, list_of_PDTs[types_of_PDTs] + " ", " ");
                    value = Parser_splitStringBetween(line, "= ", ";");
                    value = Compiler_evaluateExpression(value);
                    switch (types_of_PDTs)
                    {
                        case 0: //Add new integer definition to list
                            Compiler_addBoolean(name, bool.Parse(value));
                            break;
                       /* case 1: //Add new character definition to list
                            Compiler_addCharacter(name, char.Parse(value.Substring(1,1)));
                            break;
                        */case 2: //Add new double definition to list
                            Compiler_addDouble(name, double.Parse(value));
                            break;
                        case 3: //Add new integer definition to list
                            Compiler_addInteger(name, int.Parse(value));
                            break;
                        case 4: //Add new string definition to list
                            Compiler_addString(name, value);
                            break;
                    }
                }
            }
        }
    }
    /* Modifies a given variable by a given amount */
    void Compiler_modifyVariable(string line)
    {
        for (int m = 0; m < booleans.Count; m++)
        {
            if (line.IndexOf(booleans[m].getKey() + " ") == 0)
            {
                bool boolean_value = bool.Parse(Parser_splitStringBetween(line, "= ", ";"));
                switch (Compiler_parseManipulator(line))
                {
                    case "=":
                        booleans[m].setValue(boolean_value);
                        break;
                }
            }
        }
      /*  for (int m = 0; m < characters.Count; m++)
        {
            if (line.IndexOf(characters[m].getKey() + " ") == 0)
            {
                char character_value = char.Parse(Parser_splitStringBetween(line, "= ", ";"));
                switch (Compiler_parseManipulator(line))
                {
                    case "=":
                        characters[m].setValue(character_value);
                        break;
                }
            }
        }*/
        for (int m = 0; m < doubles.Count; m++)
        {
            if (line.IndexOf(doubles[m].getKey() + "++;") == 0)
            {
                doubles[m].setValue(doubles[m].getValue() + 1);
            }
            else if (line.IndexOf(doubles[m].getKey() + "--;") == 0)
            {
                doubles[m].setValue(doubles[m].getValue() - 1);
            }
            else if (line.IndexOf(doubles[m].getKey() + " ") == 0)
            {
                double double_value = double.Parse(Compiler_evaluateExpression(Parser_splitStringBetween(line, "= ", ";")));
                switch (Compiler_parseManipulator(line))
                {
                    case "=":
                        doubles[m].setValue(double_value);
                        break;
                    case "+=":
                        doubles[m].setValue(doubles[m].getValue() + double_value);
                        break;
                    case "-=":
                        doubles[m].setValue(doubles[m].getValue() - double_value);
                        break;
                    case "*=":
                        doubles[m].setValue(doubles[m].getValue() * double_value);
                        break;
                    case "/=":
                        doubles[m].setValue(doubles[m].getValue() / double_value);
                        break;
                    case "%=":
                        doubles[m].setValue(doubles[m].getValue() % double_value);
                        break;
                }
            }
        }
        for (int m = 0; m < integers.Count; m++)
        {
            if (line.IndexOf(integers[m].getKey() + "++;") == 0)
            {
                integers[m].setValue(integers[m].getValue() + 1);
            }
            else if (line.IndexOf(integers[m].getKey() + "--;") == 0)
            {
                integers[m].setValue(integers[m].getValue() - 1);
            }
            else if (line.IndexOf(integers[m].getKey() + " ") == 0)
            {
                int int_value = int.Parse(Compiler_evaluateExpression(Parser_splitStringBetween(line, "= ", ";")));
                switch (Compiler_parseManipulator(line))
                {
                    case "=":
                        integers[m].setValue(int_value);
                        print("modified");
                        break;
                    case "+=":
                        integers[m].setValue(integers[m].getValue() + int_value);
                        break;
                    case "-=":
                        integers[m].setValue(integers[m].getValue() - int_value);
                        break;
                    case "*=":
                        integers[m].setValue(integers[m].getValue() * int_value);
                        break;
                    case "/=":
                        integers[m].setValue(integers[m].getValue() / int_value);
                        break;
                    case "%=":
                        integers[m].setValue(integers[m].getValue() % int_value);
                        break;
                }
            }
        }
        for (int m = 0; m < strings.Count; m++)
        {
            if (line.IndexOf(strings[m].getKey() + " ") == 0)
            {
                string string_value = Compiler_evaluateExpression(Parser_splitStringBetween(line, "= ", ";"));
                switch (Compiler_parseManipulator(line))
                {
                    case "=":
                        strings[m].setValue(string_value);
                        break;
                    case "+=":
                        strings[m].setValue(strings[m].getValue() + string_value);
                        break;
                }
            }
        }
    }
    /* Creating variables assisting methods*/
    void Compiler_addBoolean(string name, bool value)
    {
        VariableObject<bool> item = new VariableObject<bool>(name);
        item.setValue(value);
        booleans.Add(item);
    }
  /*  void Compiler_addCharacter(string name, char value)
    {
        VariableObject<char> item = new VariableObject<char>(name);
        //Isolate value of variable from line of code
        item.setValue(value);
        characters.Add(item);
        //Display
        for (int m = 0; m < characters.Count; m++) print(characters[m].getKey() + "->" + characters[m].getValue());
    }*/
    void Compiler_addDouble(string name, double value)
    {
        VariableObject<double> item = new VariableObject<double>(name);
        //Isolate value of variable from line of code
        item.setValue(value);
        doubles.Add(item);
        //Display
        for (int m = 0; m < doubles.Count; m++) print(doubles[m].getKey() + "->" + doubles[m].getValue());
    }
    void Compiler_addInteger(string name, int value)
    {
        VariableObject<int> item = new VariableObject<int>(name);
        //Isolate value of variable from line of code
        item.setValue(value);
        integers.Add(item);
    }
    void Compiler_addString(string name, string value)
    {
        VariableObject<string> item = new VariableObject<string>(name);
        //Isolate value of variable from line of code
        item.setValue(value);
        strings.Add(item);
        //Display
        for (int m = 0; m < strings.Count; m++) print(strings[m].getKey() + "->" + strings[m].getValue());
    }
    /* Finds variables of any type with a given name */
    string Compiler_findVariable(string name)
    {
        if (Compiler_getIndexOfBoolean(name) != -1) return "" + Compiler_findBoolean(name);
        //if (Compiler_getIndexOfCharacter(name) != -1) return "" + Compiler_findCharacter(name);
        if (Compiler_getIndexOfDouble(name) != -1) return "" + Compiler_findDouble(name);
        if (Compiler_getIndexOfInteger(name) != -1) return "" + Compiler_findInteger(name);
        if (Compiler_getIndexOfString(name) != -1) return Compiler_findString(name);
        return "N/A";
    }
    /* Variale assisting methods */
    int Compiler_getIndexOfBoolean(string name)
    {
        for (int m = 0; m < booleans.Count; m++)
        {
            if (name == booleans[m].getKey())
            {
                return m;
            }
        }
        return -1;
    }
    bool Compiler_findBoolean(string name)
    {
        return booleans[Compiler_getIndexOfBoolean(name)].getValue();
    }
   /* int Compiler_getIndexOfCharacter(string name)
    {
        for (int m = 0; m < characters.Count; m++)
        {
            if (name == characters[m].getKey())
            {
                return m;
            }
        }
        return -1;
    }*/
   /* char Compiler_findCharacter(string name)
    {
        return characters[Compiler_getIndexOfCharacter(name)].getValue();
    }*/
    int Compiler_getIndexOfDouble(string name)
    {
        for (int m = 0; m < doubles.Count; m++)
        {
            if (name == doubles[m].getKey())
            {
                return m;
            }
        }
        return -1;
    }
    double Compiler_findDouble(string name)
    {
        return doubles[Compiler_getIndexOfDouble(name)].getValue();
    }
    int Compiler_getIndexOfInteger(string name)
    {
        for (int m = 0; m < integers.Count; m++)
        {
            if (name == integers[m].getKey())
            {
                return m;
            }
        }
        return -1;
    }
    int Compiler_findInteger(string name)
    {
        return integers[Compiler_getIndexOfInteger(name)].getValue();
    }
    int Compiler_getIndexOfString(string name)
    {
        for (int m = 0; m < strings.Count; m++)
        {
            if (name == strings[m].getKey())
            {
                return m;
            }
        }
        return -1;
    }
    string Compiler_findString(string name)
    {
        return strings[Compiler_getIndexOfString(name)].getValue();
    }
    /* Compiles and checks boolean statements, returning true or false */
    bool Compiler_booleanEvaluation(string line)
    {
        if (line == "(true)") return true;
        string evaluator = "";
        for (int i = 0; i < list_of_evaluations.Length; i++)
        {
            if (line.Contains(" " + list_of_evaluations[i] + " "))
            {
                evaluator = list_of_evaluations[i];
            }
        }
        string left_side_of_statement = Parser_splitStringBetween(line, "(", evaluator); 
        string right_side_of_statement = Parser_splitStringBetween(line, evaluator + " ", ")");
        left_side_of_statement = Compiler_evaluateExpression(left_side_of_statement);
        right_side_of_statement = Compiler_evaluateExpression(right_side_of_statement);
        return Compiler_booleanEvaluation(left_side_of_statement, right_side_of_statement, evaluator);
    }
    bool Compiler_booleanEvaluation(string left_side_of_statement, string right_side_of_statement, string comparision)
    {
        bool output_bool = false;
        char output_char = ' ';
        double output_double = 0.0;
        if (bool.TryParse(left_side_of_statement, out output_bool) && bool.TryParse(right_side_of_statement, out output_bool))
        {
            switch (comparision)
            {
                case "==":
                    return (left_side_of_statement == right_side_of_statement);
                case "!=":
                    return (left_side_of_statement != right_side_of_statement);
            }
        }

        else if (double.TryParse(left_side_of_statement, out output_double) && double.TryParse(left_side_of_statement, out output_double))
        {
            switch (comparision)
            {
                case "==":
                    return (double.Parse(left_side_of_statement) == double.Parse(right_side_of_statement));
                case "!=":
                    return (double.Parse(left_side_of_statement) != double.Parse(right_side_of_statement));
                case ">":
                    return (double.Parse(left_side_of_statement) > double.Parse(right_side_of_statement));
                case "<":
                    return (double.Parse(left_side_of_statement) < double.Parse(right_side_of_statement));
                case ">=":
                    return (double.Parse(left_side_of_statement) >= double.Parse(right_side_of_statement));
                case "<=":
                    return (double.Parse(left_side_of_statement) <= double.Parse(right_side_of_statement));
            }
        }
        /*
        else if (int.TryParse("3", out output_int) && int.TryParse("3", out output_int))
        {
            print("int");
            switch (comparision)
            {
                case "==":
                    output_of_if = (int.Parse(left_side_of_statement) == int.Parse(right_side_of_statement));
                    break;
                case "!=":
                    output_of_if = (int.Parse(left_side_of_statement) != int.Parse(right_side_of_statement));
                    break;
                case ">":
                    output_of_if = (int.Parse(left_side_of_statement) > int.Parse(right_side_of_statement));
                    break;
                case "<":
                    output_of_if = (int.Parse(left_side_of_statement) < int.Parse(right_side_of_statement));
                    break;
                case ">=":
                    output_of_if = (int.Parse(left_side_of_statement) >= int.Parse(right_side_of_statement));
                    break;
                case "<=":
                    output_of_if = (int.Parse(left_side_of_statement) <= int.Parse(right_side_of_statement));
                    break;
            }
        }*/
        else
        {
            switch (comparision)
            {
                case "==":
                    return (left_side_of_statement[0] == right_side_of_statement[0]);
                case "!=":
                    return (left_side_of_statement[0] != right_side_of_statement[0]);
            }
        }
        return false;
    }
    /* Compiles statements such that 2 + 3 = 5 in all applicable areas */
    string Compiler_evaluateExpression(string expression)
    { 
        string[] parts = expression.Split(' ');
        string output = "";
       
        bool are_numbers = true;
        double parse_output = 0.0;
       
        for (int i = 0; i < parts.Length; i+=2)
        {
            if (parts[i] == "Math.random()")
            {
                parts[i] = Random.value + "";
            }
            if (Compiler_findVariable(parts[i]) != "N/A")
            {
                parts[i] = Compiler_findVariable(parts[i]);
            }
            if (parts[i] == expression)
            {
                return parts[i];
            }
            if (!(double.TryParse(parts[i], out parse_output)))
            {
                are_numbers = false;
                break;
            }
        }
        if (are_numbers)
        {
            //  print("numbers");
            double output_double = double.Parse(parts[0]);
            //  double[] parts_numbers = new double[parts.Length];
            for (int i = 1; i < parts.Length; i += 2)
            {
                switch (parts[i])
                {
                    case "+":
                        output_double += double.Parse(parts[i + 1]);
                        break;
                    case "-":
                        output_double -= double.Parse(parts[i + 1]);
                        break;
                    case "/":
                        output_double /= double.Parse(parts[i + 1]);
                        break;
                    case "*":
                        output_double *= double.Parse(parts[i + 1]);
                        break;
                    case "%":
                        output_double %= double.Parse(parts[i + 1]);
                        break;
                }
            }
            output = "" + output_double;
        }
        else
        {
            output = "";
            for (int i = 0; i < parts.Length; i += 2)
            {
                output += parts[i];
            }
            return output;

        }
        return output;


    }
    /* Task Objects, dealing with scope*/
    int Compiler_doEndOfTask(TaskObject current_task)
    {
        if (current_task.variable_modifier != "")
        {
            Compiler_modifyVariable(current_task.variable_modifier);
        }
        if (current_task.condition != "")
        {
            if (Compiler_booleanEvaluation(current_task.condition))
            {
                return current_task.jumpTo_line;
            }
            else
            {
                return current_task.ending_line;
            }
        }
        else
        {
            return current_task.starting_line;
        }
    }
    /* Compiling lines of code */
    public void Compiler_compilation()
    {
        if (compiling == false)
        {
            if (current_line == lines.Count)
            {
                Compiler_reset();
                
            }
        }
        else
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Destroy(this.transform.GetChild(i).gameObject);
            }
        }
        compiling = !compiling;
        
    }
    public void Compiler_toggleConsoleMode()
    {
        on_console = !on_console;
    }
    /* Resetting all compiler-related variables to prepare for another iteration */
    public void Compiler_reset()
    {
        compiling = false;
        Console_output = "Console:\n"; 
        // this.lines = new List<string>();
        tasks = new Stack<TaskObject>();
        //tasks.Push(new TaskObject(0, this.lines.Count, "", ""));
        current_line = 0;

        booleans = new List<VariableObject<bool>>();
        integers = new List<VariableObject<int>>();
        doubles = new List<VariableObject<double>>();
       // characters = new List<VariableObject<char>>();
        strings = new List<VariableObject<string>>();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
        current_line = 0;

    }
    /* Parsing all the lines of text*/
    public void Compiler_compile(List<string> lines_in)
    {
        for (current_line = 0; current_line < lines_in.Count; current_line++)
        {
            Compiler_compile(lines_in[current_line]);
        }
    }
    /* Parsing each line of text into code (a.k.a. where the magic happens) */
    public void Compiler_compile(string line)
    {
        // print(line);
        if (line.IndexOf("while (") == 0)
        {
            string condition = ("(" + Parser_splitStringBetween(line, "(", ")") + ")");
            //  print(condition);
            if (Compiler_booleanEvaluation(condition))
            {
                tasks.Push(new TaskObject(current_line, Compiler_findMatchingBracket(current_line), condition, ""));
            }
            else
            {
                current_line = Compiler_findNextInstanceOf(current_line, "}");
            }
        }
        if (line.IndexOf("for (") == 0)
        {
            Compiler_addVariable(Parser_splitStringBetween(line, "(", ";") + ";");
            string condition = ("(" + Parser_splitStringBetween(line, "; ", ";") + ")");
            if (Compiler_booleanEvaluation(condition))
            {
                tasks.Push(new TaskObject(current_line, Compiler_findMatchingBracket(current_line), condition, (Parser_splitStringBetween(line, Parser_splitStringBetween(line, "; ", ";"), ")") + ";").Substring(2)));
            }
            else
            {
                current_line = Compiler_findNextInstanceOf(current_line, "}");
            }
        }
        if (tasks.Count != 0)
        {
            if (current_line == tasks.Peek().ending_line)
            {
                current_line = Compiler_doEndOfTask(tasks.Peek());
                if (current_line == tasks.Peek().ending_line) tasks.Pop();
            }
        }

        if (line.IndexOf("if (") == 0)
        {
            next_else = false; //Used to know if an "else" is linked to an if/will be executed           
            if (!Compiler_booleanEvaluation(line))
            {
                //Jump past If Statement
                current_line = Compiler_findMatchingBracket(current_line);//Compiler_findNextInstanceOf(current_line, "}");
                //Enter next else if available
                next_else = true;
            }
        }
        if (line.IndexOf("else") == 0)
        {
            if (next_else == false)
            {
                current_line = Compiler_findMatchingBracket(current_line);
            }
            
        }

        //Variable statements
        for (int types_of_PDTs = 0; types_of_PDTs < list_of_PDTs.Length; types_of_PDTs++)
        {
            //DECLARING A NEW VARIABLE OF TYPE (list_of_PDTs[types_of_PDTs])
            if (line.IndexOf(list_of_PDTs[types_of_PDTs] + " ") == 0)
            {
                Compiler_addVariable(line);
            }
        }
        //Looks for a simple statement that is modifying variables
        Compiler_modifyVariable(line);

        if (line.IndexOf("System.out.print") == 0)
        {
            string output = Parser_splitStringBetween(line, "(", ")");
            print(output);
            output = Compiler_evaluateExpression(output);
            print(output);
            if (line.IndexOf("System.out.println") == 0)
            {
                Display_pushConsoleText(output + "\n");
            }
            else Display_pushConsoleText(output);
        }
        if (line.IndexOf("Iterate(") == 0)
        {
            //Iterate(Phone.Vibrate);
            if (line.Contains("Vibrate"))
            {
               
            }
            else if (line.Contains("Position"))
            {

                string type = Parser_splitStringBetween(line, "(", ",");
                type = type.Substring(1, type.Length - 2);
                string pos = Parser_splitStringBetween(line, ", new Position(", ")");

                string x = Compiler_evaluateExpression(pos.Remove(pos.IndexOf(",")));
                string y = Compiler_evaluateExpression(pos.Substring(pos.IndexOf(",")+1));
               
                Vector2 position = new Vector2(float.Parse(x), float.Parse(y));
                position /= 6;
                GameObject obj = Instantiate(Resources.Load(type), position, this.transform.rotation) as GameObject;
                obj.transform.SetParent(this.transform);
                   
                //obj.GetComponent<SpriteRenderer>().color = new Color(Random.value, Random.value, Random.value);
            }
            else
            {
                string output = Parser_splitStringBetween(line, "(", ")");
                output = output.Substring(1, output.Length - 2); 
                Instantiate(Resources.Load(output), this.transform.position, this.transform.rotation);
            }
        }
        if (line.IndexOf("ClearIterations();") == 0)
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Destroy(this.transform.GetChild(i).gameObject);
            }
        }
    }
    /* Adjust, colorize, and display all code text*/



    void Display_pushText()
    {
        Display_Text.GetComponent<TextMeshProUGUI>().text = Interpreter_Object.ToString();
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
        Compiler_reset();



        Interpreter_Object = new Interpreter();
        Interpreter_Object.addLines(this.lines);
        Display_pushText();




        Display_Text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }
    /* Back to main menu */
    public void Misc_ReturnToMain()
    {
        SceneManager.LoadScene("Menu");
    }  

}