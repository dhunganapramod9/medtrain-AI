using UnityEngine;
using MedTrainAI.Interaction;

namespace MedTrainAI.Medical
{
    public class MedicalTool : MonoBehaviour
    {
        [Header("Tool Information")]
        public ToolType toolType;
        public string toolName;
        [TextArea(2, 4)]
        public string description;
        public Sprite toolIcon;
        
        [Header("Interaction")]
        public bool requiresPrecision = false;
        public float usageDistance = 0.5f;
        public LayerMask targetLayers = -1;
        
        [Header("Audio/Visual")]
        public AudioClip[] usageSounds;
        public ParticleSystem usageEffect;
        public GameObject highlightEffect;
        
        [Header("Tool State")]
        public bool isBeingHeld = false;
        public bool isInUse = false;
        public Transform holdingHand;
        
        private Rigidbody toolRigidbody;
        private Collider toolCollider;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Transform originalParent;
        
        public enum ToolType
        {
            Stethoscope,
            Syringe,
            Scalpel,
            Thermometer,
            BloodPressureCuff,
            Otoscope,
            Reflex_Hammer,
            Bandage,
            Forceps,
            Scissors
        }
        
        public delegate void OnToolEvent(MedicalTool tool, string eventType, object data);
        public event OnToolEvent ToolEvent;
        
        private void Awake()
        {
            toolRigidbody = GetComponent<Rigidbody>();
            toolCollider = GetComponent<Collider>();
            
            // Store original transform
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalParent = transform.parent;
        }
        
        private void Start()
        {
            SetupToolSpecifics();
        }
        
        private void SetupToolSpecifics()
        {
            // Configure tool-specific settings
            switch (toolType)
            {
                case ToolType.Stethoscope:
                    SetupStethoscope();
                    break;
                case ToolType.Syringe:
                    SetupSyringe();
                    break;
                case ToolType.Scalpel:
                    SetupScalpel();
                    break;
                case ToolType.Thermometer:
                    SetupThermometer();
                    break;
                case ToolType.BloodPressureCuff:
                    SetupBloodPressureCuff();
                    break;
                default:
                    SetupGenericTool();
                    break;
            }
        }
        
        private void SetupStethoscope()
        {
            requiresPrecision = true;
            usageDistance = 0.1f;
            targetLayers = LayerMask.GetMask("Patient");
        }
        
        private void SetupSyringe()
        {
            requiresPrecision = true;
            usageDistance = 0.05f;
            targetLayers = LayerMask.GetMask("Patient", "InjectionSite");
        }
        
        private void SetupScalpel()
        {
            requiresPrecision = true;
            usageDistance = 0.02f;
            targetLayers = LayerMask.GetMask("Patient", "SurgicalSite");
        }
        
        private void SetupThermometer()
        {
            requiresPrecision = false;
            usageDistance = 0.15f;
            targetLayers = LayerMask.GetMask("Patient");
        }
        
        private void SetupBloodPressureCuff()
        {
            requiresPrecision = false;
            usageDistance = 0.3f;
            targetLayers = LayerMask.GetMask("Patient", "Arm");
        }
        
        private void SetupGenericTool()
        {
            requiresPrecision = false;
            usageDistance = 0.2f;
            targetLayers = LayerMask.GetMask("Patient");
        }
        
        public void OnGrabbed(Transform hand)
        {
            isBeingHeld = true;
            holdingHand = hand;
            
            // Attach to hand
            transform.SetParent(hand);
            
            // Disable physics while held
            if (toolRigidbody != null)
            {
                toolRigidbody.isKinematic = true;
            }
            
            // Show highlight effect
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(true);
            }
            
            ToolEvent?.Invoke(this, "Grabbed", hand);
            
            Debug.Log($"{toolName} grabbed by {hand.name}");
        }
        
        public void OnReleased()
        {
            isBeingHeld = false;
            isInUse = false;
            holdingHand = null;
            
            // Detach from hand
            transform.SetParent(originalParent);
            
            // Re-enable physics
            if (toolRigidbody != null)
            {
                toolRigidbody.isKinematic = false;
            }
            
            // Hide highlight effect
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }
            
            ToolEvent?.Invoke(this, "Released", null);
            
            Debug.Log($"{toolName} released");
        }
        
        public bool TryUseTool(Vector3 targetPosition, Transform targetObject = null)
        {
            if (!isBeingHeld || isInUse)
                return false;
                
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            if (distance <= usageDistance)
            {
                return UseTool(targetPosition, targetObject);
            }
            
            return false;
        }
        
        public bool UseTool(Vector3 targetPosition, Transform targetObject = null)
        {
            if (!isBeingHeld)
                return false;
                
            isInUse = true;
            
            // Perform tool-specific actions
            bool success = PerformToolAction(targetPosition, targetObject);
            
            if (success)
            {
                // Play usage sound
                PlayUsageSound();
                
                // Show usage effect
                if (usageEffect != null)
                {
                    usageEffect.transform.position = targetPosition;
                    usageEffect.Play();
                }
                
                ToolEvent?.Invoke(this, "Used", new { position = targetPosition, target = targetObject });
            }
            
            isInUse = false;
            
            return success;
        }
        
        private bool PerformToolAction(Vector3 targetPosition, Transform targetObject)
        {
            switch (toolType)
            {
                case ToolType.Stethoscope:
                    return UseStethoscope(targetPosition, targetObject);
                case ToolType.Syringe:
                    return UseSyringe(targetPosition, targetObject);
                case ToolType.Scalpel:
                    return UseScalpel(targetPosition, targetObject);
                case ToolType.Thermometer:
                    return UseThermometer(targetPosition, targetObject);
                case ToolType.BloodPressureCuff:
                    return UseBloodPressureCuff(targetPosition, targetObject);
                default:
                    return UseGenericTool(targetPosition, targetObject);
            }
        }
        
        private bool UseStethoscope(Vector3 targetPosition, Transform targetObject)
        {
            // Check if targeting appropriate body part
            PatientSimulator patient = targetObject?.GetComponent<PatientSimulator>();
            if (patient != null)
            {
                // Get heart and lung sounds
                var vitals = patient.GetCurrentVitals();
                string soundDescription = GetHeartLungSounds(vitals);
                
                // Notify scenario manager of correct usage
                var scenarioManager = FindObjectOfType<ScenarioManager>();
                if (scenarioManager != null)
                {
                    scenarioManager.ProcessAction("StethoscopeUsed", new { target = targetObject.name, sounds = soundDescription });
                }
                
                Debug.Log($"Stethoscope: {soundDescription}");
                return true;
            }
            
            return false;
        }
        
        private bool UseSyringe(Vector3 targetPosition, Transform targetObject)
        {
            // Check for proper injection technique
            PatientSimulator patient = targetObject?.GetComponent<PatientSimulator>();
            if (patient != null)
            {
                // Simulate injection
                var scenarioManager = FindObjectOfType<ScenarioManager>();
                if (scenarioManager != null)
                {
                    scenarioManager.ProcessAction("InjectionGiven", new { site = targetObject.name });
                }
                
                Debug.Log("Injection administered");
                return true;
            }
            
            return false;
        }
        
        private bool UseScalpel(Vector3 targetPosition, Transform targetObject)
        {
            // Check for proper surgical technique
            var scenarioManager = FindObjectOfType<ScenarioManager>();
            if (scenarioManager != null)
            {
                scenarioManager.ProcessAction("IncisionMade", new { position = targetPosition });
            }
            
            Debug.Log("Surgical incision made");
            return true;
        }
        
        private bool UseThermometer(Vector3 targetPosition, Transform targetObject)
        {
            PatientSimulator patient = targetObject?.GetComponent<PatientSimulator>();
            if (patient != null)
            {
                var vitals = patient.GetCurrentVitals();
                
                var scenarioManager = FindObjectOfType<ScenarioManager>();
                if (scenarioManager != null)
                {
                    scenarioManager.ProcessAction("TemperatureTaken", new { temperature = vitals.temperature });
                }
                
                Debug.Log($"Temperature reading: {vitals.temperature}°F");
                return true;
            }
            
            return false;
        }
        
        private bool UseBloodPressureCuff(Vector3 targetPosition, Transform targetObject)
        {
            PatientSimulator patient = targetObject?.GetComponent<PatientSimulator>();
            if (patient != null)
            {
                var vitals = patient.GetCurrentVitals();
                
                var scenarioManager = FindObjectOfType<ScenarioManager>();
                if (scenarioManager != null)
                {
                    scenarioManager.ProcessAction("BloodPressureTaken", 
                        new { systolic = vitals.systolicBP, diastolic = vitals.diastolicBP });
                }
                
                Debug.Log($"Blood pressure: {vitals.systolicBP}/{vitals.diastolicBP} mmHg");
                return true;
            }
            
            return false;
        }
        
        private bool UseGenericTool(Vector3 targetPosition, Transform targetObject)
        {
            var scenarioManager = FindObjectOfType<ScenarioManager>();
            if (scenarioManager != null)
            {
                scenarioManager.ProcessAction($"{toolType}Used", new { target = targetObject?.name ?? "unknown" });
            }
            
            Debug.Log($"{toolName} used");
            return true;
        }
        
        private string GetHeartLungSounds(MedicalScenario.VitalSigns vitals)
        {
            string heartSounds = "Normal heart sounds";
            string lungSounds = "Clear lung sounds";
            
            // Generate realistic sounds based on vitals
            if (vitals.heartRate > 100)
                heartSounds = "Rapid heart sounds (tachycardia)";
            else if (vitals.heartRate < 60)
                heartSounds = "Slow heart sounds (bradycardia)";
                
            if (vitals.respiratoryRate > 20)
                lungSounds = "Rapid breathing sounds";
            else if (vitals.respiratoryRate < 12)
                lungSounds = "Slow breathing sounds";
                
            if (vitals.oxygenSaturation < 90)
                lungSounds += ", possible congestion";
                
            return $"{heartSounds}. {lungSounds}";
        }
        
        private void PlayUsageSound()
        {
            if (usageSounds.Length > 0)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();
                    
                AudioClip soundClip = usageSounds[Random.Range(0, usageSounds.Length)];
                audioSource.PlayOneShot(soundClip);
            }
        }
        
        public void ReturnToOriginalPosition()
        {
            if (!isBeingHeld)
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                transform.SetParent(originalParent);
                
                if (toolRigidbody != null)
                {
                    toolRigidbody.velocity = Vector3.zero;
                    toolRigidbody.angularVelocity = Vector3.zero;
                }
            }
        }
        
        public bool IsBeingHeld => isBeingHeld;
        public bool IsInUse => isInUse;
        public ToolType GetToolType() => toolType;
        public string GetToolName() => toolName;
    }
} 