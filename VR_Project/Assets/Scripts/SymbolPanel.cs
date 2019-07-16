using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SymbolPanel : MonoBehaviour
{
    public int startNumber;
    private int lastNumber = -1;
    public bool editable = true;
    public GameObject[] bits;
    // Start is called before the first frame update
    void Start()
    {
        setNumber(startNumber);
        for (int i = 0; i < bits.Length; i++)
        {
            bits[i].GetComponent<SymbolBit>().editable = editable;
            bits[i].GetComponent<SymbolBit>().bitIndex = i;
        }
    }

    public void registerCallback(Action callback)
    {
        foreach(var bit in bits)
            bit.GetComponent<SymbolBit>().registerCallback(callback);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int getNumber()
    {
        int res = 0;
        foreach (var bit in bits)
            res += bit.GetComponent<SymbolBit>().getNumber();
        return res;
    }

    public void setNumber(int number)
    {
        if(number == lastNumber)
            return;

        Debug.Log("setting symbol to number: " + number);
        foreach (var bit in bits)
            bit.GetComponent<SymbolBit>().setNumber(number);

        lastNumber = number;
    }
}
