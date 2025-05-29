using UnityEngine;
using UnityEngine.Events;

namespace MyTools.Dialogue
{
    public class DialogueContainer : MonoBehaviour
    {
        [SerializeField] private bool LevelCompleteAtLastSession = false;
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private DialogueSystem[] dialogueSystem;


        private void Start()
        {
            if (dialogueSystem.Length > 0 || !dialogueManager)
            {
                try
                {
                   //dialogueManager = GameManager.Instance._DialogueManager;
                }
                catch
                {
                    dialogueManager = FindObjectOfType<DialogueManager>();
                }
            }

          
        }

        public void PlayDialogue(DialogueSession session, NPCAnimator npcAnimator, UnityAction onStart, UnityAction onFinish)
        {
           
            dialogueManager?.StartDialogue(dialogueSystem, session, npcAnimator, LevelCompleteAtLastSession, onStart, onFinish);
        }

    }
}