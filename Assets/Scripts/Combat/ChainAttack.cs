﻿using UnityEngine;

public class ChainAttack : MonoBehaviour
{
    public int numberOfChainAttacks = 0;
    public float chainAttackTimer = 0;
    public float chainAttackResetsIn = 1;

    public Font chainFont;
    public Texture chainFontTexture;

    private void Update()
    {
        if (chainAttackTimer < chainAttackResetsIn)
            chainAttackTimer += Time.deltaTime;
        else if (numberOfChainAttacks > 0)
            numberOfChainAttacks = 0;
    }

    public void Hit()
    {
        if (numberOfChainAttacks == 3)
            numberOfChainAttacks = 0;
        chainAttackTimer = 0;
        numberOfChainAttacks++;
        AddChainAttackNumberToScene();
    }

    public void Miss()
    {
        if (numberOfChainAttacks > 0)
            ShowComboBreak();

        numberOfChainAttacks = 0;
        chainAttackTimer = float.PositiveInfinity;
    }

    public void AddChainAttackNumberToScene()
    {
        GameObject number = new GameObject();
        number.name = "Chain Attack Number";
        TextMesh tm = number.AddComponent<TextMesh>();
        tm.text = numberOfChainAttacks.ToString();
        tm.font = chainFont;
        //tm.font.material.SetTexture("Font Texture", chainFontTexture);
        tm.color = Color.red;
        tm.fontSize = 24;
        tm.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        tm.transform.position = transform.position;
        number.AddComponent<FloatUpAndDestroy>();
    }

    public void ShowComboBreak()
    {
        GameObject number = new GameObject();
        number.name = "Chain Attack Break";
        TextMesh tm = number.AddComponent<TextMesh>();
        tm.text = "X";
        tm.font = chainFont;
        //tm.font.material.SetTexture("Font Texture", chainFontTexture);
        tm.fontSize = 24;
        tm.color = Color.red;
        tm.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        tm.transform.position = transform.position;
        FloatUpAndDestroy f = number.AddComponent<FloatUpAndDestroy>();
        f.floatGravityMultiplier = 0.5f;
        f.floatVelocity = 2;
    }
}