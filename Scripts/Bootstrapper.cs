﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrapper : MonoBehaviour {

	void Start()
    {
        if (ApplicationManager.instance)
        {
            ApplicationManager.instance.LoadMainMenu();
        }
    }
}
