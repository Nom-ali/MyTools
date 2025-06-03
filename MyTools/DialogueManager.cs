using DG.Tweening;
using MyBox;
using RNA.TextWritter;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RNA.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [Separator("********** Interfac **********")]
        [SerializeField] private GameObject DialoguePanel = null;
        [SerializeField] private TextMeshProUGUI DialogueText = null;
        [SerializeField] private Button NextButton= null;
        [SerializeField] private Button SkipButton= null;
        [SerializeField] private NPCAnimator npcAnimator = null;
        [ReadOnly, SerializeField] private bool NextBtnClicked = false;


        private void Awake()
        {
            DialoguePanel.gameObject.SetActive(false);
        }

        private void Start()
        {
            //add listener
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(() =>
            {
                NextBtnClicked = true;
            });

            //add listener
            //SkipButton?.onClick.RemoveAllListeners();
            //SkipButton?.onClick.AddListener(() =>
            //{

            //});
        }

        public void StartDialogue(DialogueSystem[] dialogueSystem, DialogueSession session, NPCAnimator _npcAnimator, bool levelComplete, UnityAction onStart, UnityAction onFinish)
        {
            if(_npcAnimator) npcAnimator = _npcAnimator;
            StartCoroutine(_StartDialogue(dialogueSystem, session, levelComplete, onStart, onFinish));
        }

        public IEnumerator _StartDialogue(DialogueSystem[] dialogueSystem, DialogueSession session, bool levelComplete,  UnityAction onStart, UnityAction onFinish)
        {
            //Debug.Log("Session: " + session);

            // Find the Session index from array
            int SessionIndex = System.Array.FindIndex(dialogueSystem, item => item.dialogueSession == session);
            if (SessionIndex < 0) yield break;

            onStart?.Invoke();

            //GameManager.instance.EnableSettings(false);
            // Turn On canvas dialogue panel
            DialoguePanel.SetActive(true);

            var sesion = dialogueSystem[SessionIndex];
            for (int i = 0; i < sesion.dialogues.Length; i++)
            {
                bool animationCompleted = false;
                //DialogueText.text = "";
                Dialogues dialogue = sesion.dialogues[i];

                //enable buttons
                NextButton?.gameObject.SetActive(sesion.enableButtons);
                //SkipButton?.gameObject.SetActive(sesion.enableButtons);

                if (!dialogue.dialoguePlayer.Equals(DialoguePlayer.Player))
                    npcAnimator?.PlayRandAnimation();
                 else
                    npcAnimator?.ResetAnimator();

                //animating text
                DialogueText.WriteLetter(dialogue.dialogue, 0.03f)
                    .SetIndividualDelay(true)
                    .SetPresetMessage(dialogue.dialoguePlayer.ToString() + ": \n      ")
                    .OnComplete(() => animationCompleted = true)
                    .Start();
                //Debug.Log("Session Btn: " + sesion.enableButtons);
                if (!sesion.enableButtons)
                {
                    yield return new WaitUntil(() => animationCompleted);
                    animationCompleted = false;
                    yield return new WaitForSeconds(2f);
                    continue;
                }
                else
                {
                    yield return new WaitUntil(() => animationCompleted || NextBtnClicked);
                }

                if (animationCompleted)
                {
                    yield return new WaitUntil(() => NextBtnClicked);
                    NextBtnClicked = false;
                    continue;
                }
                if(NextBtnClicked)
                {
                    DialogueText.Kill();
                    DialogueText.text = dialogue.dialoguePlayer.ToString() + ": \n      " + dialogue.dialogue;
                    NextBtnClicked = false;
                    yield return new WaitUntil(() => NextBtnClicked);
                    NextBtnClicked = false;
                }
            }

            // Turn off canvas dialogue panel
            DialoguePanel.SetActive(false);
            //GameManager.Instance.EnableSettings(true);
            sesion.OnComplete?.Invoke();


            if (levelComplete)
            {
                DialogueSession nSession = (DialogueSession)(dialogueSystem.Length);
                Debug.Log("Session: " + nSession);
                if (nSession == session)
                {
                    //GameManager.Instance.MissionComplete();
                }
            }

            onFinish?.Invoke();
            yield return null;
        }
    }

    [System.Serializable]
    public enum DialogueSession
    {
        None, Session_01, Session_02, Session_03, Session_04, Session_05
    }

    [System.Serializable]
    public struct SessionArray
    {
        public bool Completed;
        public DialogueSession Session;
    }

    [System.Serializable]
    public enum CallDialogue
    {
        None, AtStart, OnTriggerEnter
    }

    [System.Serializable]
    public enum DialoguePlayer
    {
        None, Player, Boss, NPC, Jhon, Wick, Gang_Member, 
    }

    [System.Serializable]

    public struct Dialogues
    {
        public DialoguePlayer dialoguePlayer;
        [TextArea] public string dialogue;
    }

    [System.Serializable]
    public struct DialogueSystem
    {
        public DialogueSession dialogueSession;
        public bool enableButtons;
        public Dialogues[] dialogues;
        [Space]
        public UnityEvent OnComplete;
    }
}