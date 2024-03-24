﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : InteractableObject
{
    public override void Pickup()
    {
        //Trigger the yes no prompt to confirm before executing sleep sequence
        UIManager.Instance.TriggerYesNoPrompt("؟ﻡﻮﻨﻟﺍ ﺪﻳﺮﺗ ﻞﻫ", GameStateManager.Instance.Sleep);
    }
}
