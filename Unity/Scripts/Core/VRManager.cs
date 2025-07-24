using UnityEngine;
using UnityEngine.XR;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using System.Collections;

namespace MedTrainAI.Core
{
    public class VRManager : MonoBehaviour
    {
        [Header("VR Systems")]
        public Camera vrCamera;
        public Transform leftHand;
        public Transform rightHand;
        public GameObject handTrackingPrefab;
        
        [Header("Medical Training")]
        public ScenarioManager scenarioManager;
        public PatientSimulator patientSimulator;
        public DialogueSystem dialogueSystem;
        
        [Header("Settings")]
        public bool enableHandTracking = true;
        public bool enableVoiceInteraction = true;
        public float interactionDistance = 2.0f;
        
        private bool isVRReady = false;
        private XRInputSubsystem inputSubsystem;
        private HandTrackingManager handTrackingManager;
        
        public static VRManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            StartCoroutine(InitializeVRSystems());
        }
        
        private IEnumerator InitializeVRSystems()
        {
            Debug.Log("Initializing VR Systems for Meta Quest 3S...");
            
            // Wait for XR to initialize
            yield return new WaitUntil(() => XRSettings.isDeviceActive);
            
            // Initialize hand tracking
            if (enableHandTracking)
            {
                InitializeHandTracking();
            }
            
            // Initialize medical training systems
            yield return StartCoroutine(InitializeMedicalSystems());
            
            isVRReady = true;
            Debug.Log("VR Systems initialized successfully!");
            
            // Start the first medical scenario
            if (scenarioManager != null)
            {
                scenarioManager.StartRandomScenario();
            }
        }
        
        private void InitializeHandTracking()
        {
            handTrackingManager = FindObjectOfType<HandTrackingManager>();
            if (handTrackingManager == null)
            {
                GameObject handTrackingObj = Instantiate(handTrackingPrefab);
                handTrackingManager = handTrackingObj.GetComponent<HandTrackingManager>();
            }
            
            handTrackingManager.Initialize(leftHand, rightHand);
            Debug.Log("Hand tracking initialized");
        }
        
        private IEnumerator InitializeMedicalSystems()
        {
            // Initialize AI systems
            if (dialogueSystem != null)
            {
                yield return StartCoroutine(dialogueSystem.Initialize());
            }
            
            if (patientSimulator != null)
            {
                yield return StartCoroutine(patientSimulator.Initialize());
            }
            
            if (scenarioManager != null)
            {
                yield return StartCoroutine(scenarioManager.Initialize());
            }
        }
        
        public bool IsVRReady()
        {
            return isVRReady;
        }
        
        public void RestartScenario()
        {
            if (scenarioManager != null && isVRReady)
            {
                scenarioManager.RestartCurrentScenario();
            }
        }
        
        public void NextScenario()
        {
            if (scenarioManager != null && isVRReady)
            {
                scenarioManager.StartRandomScenario();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Pause VR systems
                if (dialogueSystem != null)
                    dialogueSystem.PauseSystem();
            }
            else
            {
                // Resume VR systems
                if (dialogueSystem != null)
                    dialogueSystem.ResumeSystem();
            }
        }
    }
} 