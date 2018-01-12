using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableObject<T>
{
    T value;
    string key;
    public VariableObject(string key)
    {
        this.key = key;
    }

    public void setValue(T value)
    {
        this.value = value;
    }
    public T getValue()
    {
        return value;
    }
    public string getKey()
    {
        return key;
    }
   

    
    

}
