using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSTMPControllerIcons
// stores string references for inserting sprites into TMPro UI objects
// created 25/7/25
// modified 25/7/25

[CreateAssetMenu(fileName = "ControllerIcons", menuName = "SO/Global/ControllerIcons", order = 20)]
public class CSTMPControllerIcons : ScriptableObject
{
    [Tooltip("If font size should be override for icons")]public int fontOverride;
    [Header("D-Pad")]
    public Sprite DPad;
    public Sprite DPU;
    public Sprite DPR;
    public Sprite DPD;
    public Sprite DPL;
    [Header("Main buttons")]
    public Sprite butN;
    public Sprite butE;
    public Sprite butS;
    public Sprite butW;
    [Header("Left")]
    public Sprite leftStick;
    public Sprite leftTrack;
    public Sprite left1Bump;
    public Sprite left2Trig;
    public Sprite left3Stick;
    public Sprite left4;
    public Sprite left5;
    [Header("Right")]
    public Sprite rightStick;
    public Sprite rightTrack;
    public Sprite right1Bump;
    public Sprite right2Trig;
    public Sprite right3Stick;
    public Sprite right4;
    public Sprite right5;
    [Header("Other")]
    public Sprite start;
    public Sprite select;
    // convert sprites to strings
    public string DPadString { get => CSUtils.SpriteAssetToSprite(DPad.name, fontOverride); }
    public string DPUString { get => CSUtils.SpriteAssetToSprite(DPU.name, fontOverride); }
    public string DPRString { get => CSUtils.SpriteAssetToSprite(DPR.name, fontOverride); }
    public string DPDString { get => CSUtils.SpriteAssetToSprite(DPD.name, fontOverride); }
    public string DPLString { get => CSUtils.SpriteAssetToSprite(DPL.name, fontOverride); }
    public string butNString { get => CSUtils.SpriteAssetToSprite(butN.name, fontOverride); }
    public string butEString { get => CSUtils.SpriteAssetToSprite(butE.name, fontOverride); }
    public string butSString { get => CSUtils.SpriteAssetToSprite(butS.name, fontOverride); }
    public string butWString { get => CSUtils.SpriteAssetToSprite(butW.name, fontOverride); }
    public string leftStickString { get => CSUtils.SpriteAssetToSprite(leftStick.name, fontOverride); }
    public string leftTrackString { get => CSUtils.SpriteAssetToSprite(leftTrack.name, fontOverride); }
    public string left1BumpString { get => CSUtils.SpriteAssetToSprite(left1Bump.name, fontOverride); }
    public string left2TrigString { get => CSUtils.SpriteAssetToSprite(left2Trig.name, fontOverride); }
    public string left3StickString { get => CSUtils.SpriteAssetToSprite(left3Stick.name, fontOverride); }
    public string left4String { get => CSUtils.SpriteAssetToSprite(left4.name, fontOverride); }
    public string left5String { get => CSUtils.SpriteAssetToSprite(left5.name, fontOverride); }
    public string rightStickString { get => CSUtils.SpriteAssetToSprite(rightStick.name, fontOverride); }
    public string rightTrackString { get => CSUtils.SpriteAssetToSprite(rightTrack.name, fontOverride); }
    public string right1BumpString { get => CSUtils.SpriteAssetToSprite(right1Bump.name, fontOverride); }
    public string right2TrigString { get => CSUtils.SpriteAssetToSprite(right2Trig.name, fontOverride); }
    public string right3StickString { get => CSUtils.SpriteAssetToSprite(right3Stick.name, fontOverride); }
    public string right4String { get => CSUtils.SpriteAssetToSprite(right4.name, fontOverride); }
    public string right5String { get => CSUtils.SpriteAssetToSprite(right5.name, fontOverride); }
    public string startString { get => CSUtils.SpriteAssetToSprite(start.name, fontOverride); }
    public string selectString { get => CSUtils.SpriteAssetToSprite(select.name, fontOverride); }
}
