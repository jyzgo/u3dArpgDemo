using System;
using System.Collections.Generic;
using UnityEngine;

public class UIFightScore : MonoBehaviour
{
    public UILabel fsValue;

    private int _fs;
    public int FS
    {
        set
        {
            _fs = value;
            fsValue.text = _fs.ToString();
        }
        get
        {
            return _fs;
        }
    }
}
