﻿using UnityEngine;

public class Perk : MonoBehaviour
{
    public enum PerkType
    {
        AXE_FIRE,
        HAT_DTVAMPIRISM
    }
    public PerkType type;

    [HideInInspector]
    public string name;
    [HideInInspector]
    public bool unlocked;

    public void CheckStatus()
    {
        switch (type)
        {
            case PerkType.AXE_FIRE:
                name = "Axe_Fire";
                unlocked = GlobalSettings.axe_fire_unlocked;
                break;
            case PerkType.HAT_DTVAMPIRISM:
                name = "Hat_DTVampirism";
                unlocked = GlobalSettings.hat_dtVampirism_unlocked;
                break;
        }

        if (!unlocked)
            gameObject.SetActive(false);
    }

    public void OnCollision(GameObject other)
    {
        // Give player the perk
    }

}
