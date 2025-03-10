using UnityEngine;

[CreateAssetMenu(menuName = "Dialouge/DialougeObject")]
public class DialougeObject : ScriptableObject
{
    [System.Serializable]
    public class Dialogue
    {
        public string speaker;
        [TextArea(3, 5)] public string text;
    }

    public Dialogue[] dialogues;
}
