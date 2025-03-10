using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MenuOfNPCDialogue", menuName ="NPCwitch Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
    public float typeSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch;
    
}
