using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;
using System;

namespace MedTrainAI.Interaction
{
    public class HandTrackingManager : MonoBehaviour
    {
        [Header("Hand References")]
        public Transform leftHandAnchor;
        public Transform rightHandAnchor;
        public GameObject leftHandModel;
        public GameObject rightHandModel;
        
        [Header("Medical Tools")]
        public List<MedicalTool> availableTools = new List<MedicalTool>();
        public Transform toolSpawnPoint;
        public LayerMask interactableLayer = -1;
        
        [Header("Interaction Settings")]
        public float grabbingDistance = 0.1f;
        public float hapticIntensity = 0.5f;
        public float hapticDuration = 0.1f;
        
        [Header("Gesture Recognition")]
        public bool enableGestureRecognition = true;
        public float gestureConfidenceThreshold = 0.8f;
        
        private OVRHand leftOVRHand;
        private OVRHand rightOVRHand;
        private OVRSkeleton leftSkeleton;
        private OVRSkeleton rightSkeleton;
        
        private MedicalTool leftHandTool;
        private MedicalTool rightHandTool;
        
        private Dictionary<string, Action> gestureActions;
        private HandGestureRecognizer gestureRecognizer;
        
        public delegate void OnToolGrabbed(MedicalTool tool, HandType hand);
        public event OnToolGrabbed ToolGrabbed;
        
        public delegate void OnToolReleased(MedicalTool tool, HandType hand);
        public event OnToolReleased ToolReleased;
        
        public delegate void OnGestureDetected(string gestureName, HandType hand);
        public event OnGestureDetected GestureDetected;
        
        public enum HandType
        {
            Left,
            Right
        }
        
        private void Awake()
        {
            InitializeGestureActions();
            gestureRecognizer = GetComponent<HandGestureRecognizer>();
            if (gestureRecognizer == null)
            {
                gestureRecognizer = gameObject.AddComponent<HandGestureRecognizer>();
            }
        }
        
        public void Initialize(Transform leftHand, Transform rightHand)
        {
            leftHandAnchor = leftHand;
            rightHandAnchor = rightHand;
            
            SetupHandTracking();
            SetupMedicalTools();
        }
        
        private void SetupHandTracking()
        {
            // Get OVR hand components
            leftOVRHand = leftHandAnchor.GetComponentInChildren<OVRHand>();
            rightOVRHand = rightHandAnchor.GetComponentInChildren<OVRHand>();
            
            leftSkeleton = leftHandAnchor.GetComponentInChildren<OVRSkeleton>();
            rightSkeleton = rightHandAnchor.GetComponentInChildren<OVRSkeleton>();
            
            if (leftOVRHand == null || rightOVRHand == null)
            {
                Debug.LogError("OVRHand components not found! Make sure OVR prefabs are properly set up.");
                return;
            }
            
            // Enable hand tracking
            OVRManager.instance.isInsightPassthroughEnabled = true;
            
            Debug.Log("Hand tracking initialized successfully");
        }
        
        private void SetupMedicalTools()
        {
            // Initialize medical tools with proper grabbable components
            foreach (var tool in availableTools)
            {
                if (tool != null)
                {
                    SetupToolInteraction(tool);
                }
            }
        }
        
        private void SetupToolInteraction(MedicalTool tool)
        {
            // Add grabbable components if not present
            if (tool.GetComponent<HandGrabInteractable>() == null)
            {
                var grabInteractable = tool.gameObject.AddComponent<HandGrabInteractable>();
                
                // Setup grab points for medical tools
                CreateGrabPoints(tool, grabInteractable);
            }
            
            // Add collision detection
            if (tool.GetComponent<Collider>() == null)
            {
                var collider = tool.gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
            }
            
            // Set proper layer
            tool.gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
        
        private void CreateGrabPoints(MedicalTool tool, HandGrabInteractable grabInteractable)
        {
            // Create appropriate grab points based on tool type
            switch (tool.toolType)
            {
                case MedicalTool.ToolType.Stethoscope:
                    CreateStethoscopeGrabPoints(tool, grabInteractable);
                    break;
                case MedicalTool.ToolType.Syringe:
                    CreateSyringeGrabPoints(tool, grabInteractable);
                    break;
                case MedicalTool.ToolType.Scalpel:
                    CreateScalpelGrabPoints(tool, grabInteractable);
                    break;
                case MedicalTool.ToolType.Thermometer:
                    CreateThermometerGrabPoints(tool, grabInteractable);
                    break;
                default:
                    CreateDefaultGrabPoints(tool, grabInteractable);
                    break;
            }
        }
        
        private void CreateStethoscopeGrabPoints(MedicalTool tool, HandGrabInteractable grabInteractable)
        {
            // Main handle grab point
            GameObject grabPoint = new GameObject("StethoscopeGrab");
            grabPoint.transform.SetParent(tool.transform);
            grabPoint.transform.localPosition = Vector3.zero;
            grabPoint.transform.localRotation = Quaternion.identity;
            
            var handGrabPose = grabPoint.AddComponent<HandGrabPose>();
            handGrabPose.HandPose = CreateMedicalGrip();
        }
        
        private void CreateSyringeGrabPoints(MedicalTool tool, HandGrabInteractable grabInteractable)
        {
            // Syringe requires precise finger positioning
            GameObject grabPoint = new GameObject("SyringeGrab");
            grabPoint.transform.SetParent(tool.transform);
            grabPoint.transform.localPosition = new Vector3(0, 0, 0.05f); // Slightly forward
            
            var handGrabPose = grabPoint.AddComponent<HandGrabPose>();
            handGrabPose.HandPose = CreatePrecisionGrip();
        }
        
        private void CreateScalpelGrabPoints(MedicalTool tool, HandGrabInteractable grabInteractable)
        {
            // Scalpel requires surgical grip
            GameObject grabPoint = new GameObject("ScalpelGrab");
            grabPoint.transform.SetParent(tool.transform);
            grabPoint.transform.localPosition = Vector3.zero;
            
            var handGrabPose = grabPoint.AddComponent<HandGrabPose>();
            handGrabPose.HandPose = CreateSurgicalGrip();
        }
        
        private void CreateThermometerGrabPoints(MedicalTool tool, HandGrabInteractable grabInteractable)
        {
            // Thermometer uses delicate grip
            GameObject grabPoint = new GameObject("ThermometerGrab");
            grabPoint.transform.SetParent(tool.transform);
            grabPoint.transform.localPosition = Vector3.zero;
            
            var handGrabPose = grabPoint.AddComponent<HandGrabPose>();
            handGrabPose.HandPose = CreateDelicateGrip();
        }
        
        private void CreateDefaultGrabPoints(MedicalTool tool, HandGrabInteractable grabInteractable)
        {
            // Default medical tool grip
            GameObject grabPoint = new GameObject("DefaultGrab");
            grabPoint.transform.SetParent(tool.transform);
            grabPoint.transform.localPosition = Vector3.zero;
            
            var handGrabPose = grabPoint.AddComponent<HandGrabPose>();
            handGrabPose.HandPose = CreateMedicalGrip();
        }
        
        private HandPose CreateMedicalGrip()
        {
            // Create a proper medical instrument grip pose
            var handPose = ScriptableObject.CreateInstance<HandPose>();
            // Configure hand pose for medical instruments
            return handPose;
        }
        
        private HandPose CreatePrecisionGrip()
        {
            var handPose = ScriptableObject.CreateInstance<HandPose>();
            // Configure precision grip for syringes
            return handPose;
        }
        
        private HandPose CreateSurgicalGrip()
        {
            var handPose = ScriptableObject.CreateInstance<HandPose>();
            // Configure surgical grip for scalpels
            return handPose;
        }
        
        private HandPose CreateDelicateGrip()
        {
            var handPose = ScriptableObject.CreateInstance<HandPose>();
            // Configure delicate grip for thermometers
            return handPose;
        }
        
        private void Update()
        {
            UpdateHandTracking();
            
            if (enableGestureRecognition)
            {
                CheckForGestures();
            }
        }
        
        private void UpdateHandTracking()
        {
            // Update hand positions and check for interactions
            if (leftOVRHand != null && leftOVRHand.IsTracked)
            {
                CheckHandInteractions(HandType.Left);
            }
            
            if (rightOVRHand != null && rightOVRHand.IsTracked)
            {
                CheckHandInteractions(HandType.Right);
            }
        }
        
        private void CheckHandInteractions(HandType handType)
        {
            Transform handTransform = handType == HandType.Left ? leftHandAnchor : rightHandAnchor;
            OVRHand ovrHand = handType == HandType.Left ? leftOVRHand : rightOVRHand;
            
            // Check for grab gesture
            if (ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > 0.7f)
            {
                CheckForToolGrab(handType, handTransform);
            }
        }
        
        private void CheckForToolGrab(HandType handType, Transform handTransform)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(handTransform.position, grabbingDistance, interactableLayer);
            
            foreach (var collider in nearbyColliders)
            {
                MedicalTool tool = collider.GetComponent<MedicalTool>();
                if (tool != null && !tool.IsBeingHeld)
                {
                    GrabTool(tool, handType);
                    break;
                }
            }
        }
        
        public void GrabTool(MedicalTool tool, HandType handType)
        {
            if (handType == HandType.Left)
            {
                if (leftHandTool != null)
                    ReleaseTool(HandType.Left);
                leftHandTool = tool;
            }
            else
            {
                if (rightHandTool != null)
                    ReleaseTool(HandType.Right);
                rightHandTool = tool;
            }
            
            tool.OnGrabbed(handType == HandType.Left ? leftHandAnchor : rightHandAnchor);
            
            // Provide haptic feedback
            ProvideHapticFeedback(handType, hapticIntensity, hapticDuration);
            
            ToolGrabbed?.Invoke(tool, handType);
            
            Debug.Log($"Grabbed {tool.toolType} with {handType} hand");
        }
        
        public void ReleaseTool(HandType handType)
        {
            MedicalTool tool = handType == HandType.Left ? leftHandTool : rightHandTool;
            
            if (tool != null)
            {
                tool.OnReleased();
                
                if (handType == HandType.Left)
                    leftHandTool = null;
                else
                    rightHandTool = null;
                
                ToolReleased?.Invoke(tool, handType);
                
                Debug.Log($"Released {tool.toolType} from {handType} hand");
            }
        }
        
        private void CheckForGestures()
        {
            if (gestureRecognizer != null)
            {
                // Check left hand gestures
                if (leftOVRHand != null && leftOVRHand.IsTracked)
                {
                    string leftGesture = gestureRecognizer.RecognizeGesture(leftSkeleton);
                    if (!string.IsNullOrEmpty(leftGesture))
                    {
                        ProcessGesture(leftGesture, HandType.Left);
                    }
                }
                
                // Check right hand gestures
                if (rightOVRHand != null && rightOVRHand.IsTracked)
                {
                    string rightGesture = gestureRecognizer.RecognizeGesture(rightSkeleton);
                    if (!string.IsNullOrEmpty(rightGesture))
                    {
                        ProcessGesture(rightGesture, HandType.Right);
                    }
                }
            }
        }
        
        private void ProcessGesture(string gestureName, HandType handType)
        {
            if (gestureActions.ContainsKey(gestureName))
            {
                gestureActions[gestureName]?.Invoke();
                GestureDetected?.Invoke(gestureName, handType);
            }
            
            Debug.Log($"Detected {gestureName} gesture with {handType} hand");
        }
        
        private void InitializeGestureActions()
        {
            gestureActions = new Dictionary<string, Action>
            {
                { "point", () => OnPointGesture() },
                { "thumbsup", () => OnThumbsUpGesture() },
                { "peace", () => OnPeaceGesture() },
                { "fist", () => OnFistGesture() },
                { "openpalm", () => OnOpenPalmGesture() }
            };
        }
        
        private void OnPointGesture()
        {
            // Point gesture detected - could be used for indicating symptoms
            Debug.Log("Point gesture detected");
        }
        
        private void OnThumbsUpGesture()
        {
            // Thumbs up - positive feedback
            Debug.Log("Thumbs up gesture detected");
        }
        
        private void OnPeaceGesture()
        {
            // Peace sign - could indicate "two" or confirmation
            Debug.Log("Peace gesture detected");
        }
        
        private void OnFistGesture()
        {
            // Fist - could indicate pain or tension
            Debug.Log("Fist gesture detected");
        }
        
        private void OnOpenPalmGesture()
        {
            // Open palm - could indicate "stop" or "five"
            Debug.Log("Open palm gesture detected");
        }
        
        private void ProvideHapticFeedback(HandType handType, float intensity, float duration)
        {
            OVRInput.Controller controller = handType == HandType.Left ? 
                OVRInput.Controller.LHand : OVRInput.Controller.RHand;
            
            OVRInput.SetControllerVibration(intensity, intensity, controller);
            
            // Stop vibration after duration
            Invoke(nameof(StopHapticFeedback), duration);
        }
        
        private void StopHapticFeedback()
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LHand);
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RHand);
        }
        
        public MedicalTool GetHeldTool(HandType handType)
        {
            return handType == HandType.Left ? leftHandTool : rightHandTool;
        }
        
        public bool IsHandTracked(HandType handType)
        {
            if (handType == HandType.Left)
                return leftOVRHand != null && leftOVRHand.IsTracked;
            else
                return rightOVRHand != null && rightOVRHand.IsTracked;
        }
    }
} 