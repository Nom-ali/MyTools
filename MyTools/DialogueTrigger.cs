using MyBox;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace RNA.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private SessionArray[] Sessions;
        [SerializeField] private DialogueContainer container;
        [SerializeField] private CallDialogue CallDialogue = CallDialogue.None;

        [ConditionalField(nameof(CallDialogue), false, CallDialogue.OnTriggerEnter)]
        [SerializeField] private string _CompareTag = "";
        [ConditionalField(nameof(CallDialogue), false, CallDialogue.OnTriggerEnter)]
        [SerializeField] private RigidbodyConstraints OnEnterRigidbodyConstraints = RigidbodyConstraints.None;
        [ConditionalField(nameof(CallDialogue), false, CallDialogue.OnTriggerEnter)]
        [SerializeField] private bool DisableGameobjectOnTrigger = true;
        [SerializeField] private bool DisablePlayerOnTrigger = true;
        [SerializeField] private UnityEvent OnStart;
        [SerializeField] private UnityEvent OnFinish;

        [SerializeField] private NPCAnimator npcAnimator;
        [ReadOnly, SerializeField]private int SessionCount = 0;

        public void Start()
        {
            StartCoroutine(StartDialogue());
        }

        private IEnumerator StartDialogue()
        {
            try
            {
                container ??= GetComponentInParent<DialogueContainer>();
                npcAnimator ??= GetComponentInParent<NPCAnimator>();
            }
            catch
            {
                Debug.LogError("Something missing");
            }

            if (CallDialogue.Equals(CallDialogue.AtStart))
            {
                int currentCount = SessionManager();
                container.PlayDialogue(Sessions[currentCount].Session, npcAnimator, () => OnStart?.Invoke(), () => OnFinish?.Invoke());
            }
            yield return null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_CompareTag))
            {
                //if (other.GetComponent<JUCharacterController>().IsDriving)
                //{
                //    //Debug.Break();
                //    return;
                //}

                int currentCount = SessionManager();
                container.PlayDialogue(Sessions[currentCount].Session, npcAnimator, () => OnStart?.Invoke(), () => OnFinish?.Invoke());

                if (!OnEnterRigidbodyConstraints.Equals(RigidbodyConstraints.None) && other.TryGetComponent(out Rigidbody rb)) 
                    rb.constraints = OnEnterRigidbodyConstraints;
                //if (DisablePlayerOnTrigger) GameManager.instance.EnableDisablePlayer(false);
                gameObject.SetActive(!DisableGameobjectOnTrigger);
            }
        }

        int SessionManager()
        {
            int currentCount = SessionCount;
            OnFinish.AddListener(() => 
            {
                Sessions[currentCount].Completed = true;
                SessionCount++;
            });
            return currentCount;
        }

        public void Reset()
        {
            for (int i = 0; i < Sessions.Length; i++)
                Sessions[i].Completed = false;
            SessionCount = 0;
        }
    }

}

