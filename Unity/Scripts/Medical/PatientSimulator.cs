using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MedTrainAI.AI;

namespace MedTrainAI.Medical
{
    public class PatientSimulator : MonoBehaviour
    {
        [Header("Patient Model")]
        public Animator patientAnimator;
        public Transform patientHead;
        public Transform patientBody;
        public SkinnedMeshRenderer patientSkin;
        
        [Header("Vital Signs")]
        public MedicalScenario.VitalSigns currentVitals;
        public VitalSignsMonitor vitalSignsDisplay;
        public float vitalUpdateInterval = 2f;
        
        [Header("Patient State")]
        public MedicalScenario.PatientProfile currentProfile;
        public PatientState currentState;
        public float painLevel = 0f;
        public float consciousnessLevel = 1f;
        
        [Header("Audio")]
        public AudioSource patientVoice;
        public AudioClip[] painSounds;
        public AudioClip[] breathingSounds;
        public AudioClip[] heartbeatSounds;
        
        [Header("Visual Effects")]
        public ParticleSystem breathingEffect;
        public Renderer[] symptomRenderers;
        public Material normalSkin;
        public Material paleSkin;
        public Material flushedSkin;
        
        private ConvaiIntegration convaiIntegration;
        private GPT4OIntegration gptIntegration;
        private Coroutine vitalSignsCoroutine;
        private Coroutine symptomsCoroutine;
        
        public enum PatientState
        {
            Stable,
            Deteriorating,
            Critical,
            Improving,
            Unconscious
        }
        
        public delegate void OnVitalSignsChanged(MedicalScenario.VitalSigns newVitals);
        public event OnVitalSignsChanged VitalSignsChanged;
        
        public delegate void OnPatientResponse(string response, bool isVoice);
        public event OnPatientResponse PatientResponded;
        
        private void Start()
        {
            InitializePatient();
        }
        
        public IEnumerator Initialize()
        {
            Debug.Log("Initializing Patient Simulator...");
            
            convaiIntegration = FindObjectOfType<ConvaiIntegration>();
            gptIntegration = FindObjectOfType<GPT4OIntegration>();
            
            // Subscribe to voice interactions
            if (convaiIntegration != null)
            {
                convaiIntegration.VoiceResponseReceived += OnVoiceResponse;
                convaiIntegration.PatientActionTriggered += OnPatientAction;
            }
            
            InitializePatient();
            
            yield return null;
            Debug.Log("Patient Simulator initialized");
        }
        
        private void InitializePatient()
        {
            // Set default vitals
            currentVitals = new MedicalScenario.VitalSigns();
            currentState = PatientState.Stable;
            
            // Start vital signs monitoring
            if (vitalSignsCoroutine != null)
                StopCoroutine(vitalSignsCoroutine);
            vitalSignsCoroutine = StartCoroutine(MonitorVitalSigns());
            
            // Start symptom simulation
            if (symptomsCoroutine != null)
                StopCoroutine(symptomsCoroutine);
            symptomsCoroutine = StartCoroutine(SimulateSymptoms());
        }
        
        public void LoadPatientProfile(MedicalScenario.PatientProfile profile)
        {
            currentProfile = profile;
            
            // Configure patient appearance based on profile
            ConfigurePatientAppearance();
            
            // Set initial animations based on personality
            SetPatientBehavior(profile.personalityType);
            
            // Configure Convai character if available
            if (convaiIntegration != null)
            {
                // You would set the character ID based on the patient profile
                string characterId = GetCharacterIdForProfile(profile);
                convaiIntegration.SetCharacter(characterId);
            }
            
            Debug.Log($"Loaded patient profile: {profile.name}, {profile.age} years old");
        }
        
        private void ConfigurePatientAppearance()
        {
            if (currentProfile == null || patientAnimator == null)
                return;
                
            // Set gender-appropriate animations and appearance
            patientAnimator.SetBool("IsMale", currentProfile.gender == MedicalScenario.PatientProfile.Gender.Male);
            patientAnimator.SetInteger("Age", currentProfile.age);
            
            // Adjust skin color based on symptoms or condition
            UpdateSkinAppearance();
        }
        
        private void SetPatientBehavior(MedicalScenario.PatientProfile.Personality personality)
        {
            if (patientAnimator == null)
                return;
                
            // Set animation parameters based on personality
            switch (personality)
            {
                case MedicalScenario.PatientProfile.Personality.Anxious:
                    patientAnimator.SetBool("IsAnxious", true);
                    patientAnimator.SetFloat("Restlessness", 0.8f);
                    break;
                case MedicalScenario.PatientProfile.Personality.Calm:
                    patientAnimator.SetBool("IsCalm", true);
                    patientAnimator.SetFloat("Restlessness", 0.2f);
                    break;
                case MedicalScenario.PatientProfile.Personality.Aggressive:
                    patientAnimator.SetBool("IsAgitated", true);
                    patientAnimator.SetFloat("Restlessness", 1.0f);
                    break;
                case MedicalScenario.PatientProfile.Personality.Confused:
                    patientAnimator.SetBool("IsConfused", true);
                    patientAnimator.SetFloat("Awareness", 0.5f);
                    break;
            }
        }
        
        public void SetVitalSigns(MedicalScenario.VitalSigns vitals)
        {
            currentVitals = vitals;
            UpdateVitalSignsEffects();
            VitalSignsChanged?.Invoke(currentVitals);
            
            if (vitalSignsDisplay != null)
            {
                vitalSignsDisplay.UpdateDisplay(currentVitals);
            }
        }
        
        public void TransitionToVitals(MedicalScenario.VitalSigns targetVitals, float transitionTime)
        {
            StartCoroutine(TransitionVitalsCoroutine(targetVitals, transitionTime * 60f));
        }
        
        private IEnumerator TransitionVitalsCoroutine(MedicalScenario.VitalSigns targetVitals, float duration)
        {
            MedicalScenario.VitalSigns startVitals = new MedicalScenario.VitalSigns
            {
                heartRate = currentVitals.heartRate,
                systolicBP = currentVitals.systolicBP,
                diastolicBP = currentVitals.diastolicBP,
                respiratoryRate = currentVitals.respiratoryRate,
                temperature = currentVitals.temperature,
                oxygenSaturation = currentVitals.oxygenSaturation,
                painLevel = currentVitals.painLevel
            };
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                
                currentVitals.heartRate = Mathf.RoundToInt(Mathf.Lerp(startVitals.heartRate, targetVitals.heartRate, t));
                currentVitals.systolicBP = Mathf.RoundToInt(Mathf.Lerp(startVitals.systolicBP, targetVitals.systolicBP, t));
                currentVitals.diastolicBP = Mathf.RoundToInt(Mathf.Lerp(startVitals.diastolicBP, targetVitals.diastolicBP, t));
                currentVitals.respiratoryRate = Mathf.RoundToInt(Mathf.Lerp(startVitals.respiratoryRate, targetVitals.respiratoryRate, t));
                currentVitals.temperature = Mathf.Lerp(startVitals.temperature, targetVitals.temperature, t);
                currentVitals.oxygenSaturation = Mathf.RoundToInt(Mathf.Lerp(startVitals.oxygenSaturation, targetVitals.oxygenSaturation, t));
                currentVitals.painLevel = Mathf.RoundToInt(Mathf.Lerp(startVitals.painLevel, targetVitals.painLevel, t));
                
                UpdateVitalSignsEffects();
                VitalSignsChanged?.Invoke(currentVitals);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            currentVitals = targetVitals;
            UpdateVitalSignsEffects();
            VitalSignsChanged?.Invoke(currentVitals);
        }
        
        private IEnumerator MonitorVitalSigns()
        {
            while (true)
            {
                yield return new WaitForSeconds(vitalUpdateInterval);
                
                // Add realistic variations to vital signs
                AddVitalSignsVariation();
                
                // Update patient state based on vitals
                UpdatePatientState();
                
                // Update displays
                if (vitalSignsDisplay != null)
                {
                    vitalSignsDisplay.UpdateDisplay(currentVitals);
                }
                
                VitalSignsChanged?.Invoke(currentVitals);
            }
        }
        
        private void AddVitalSignsVariation()
        {
            // Add small random variations to make vitals realistic
            currentVitals.heartRate += Random.Range(-2, 3);
            currentVitals.respiratoryRate += Random.Range(-1, 2);
            currentVitals.temperature += Random.Range(-0.1f, 0.1f);
            
            // Clamp values to realistic ranges
            currentVitals.heartRate = Mathf.Clamp(currentVitals.heartRate, 30, 200);
            currentVitals.respiratoryRate = Mathf.Clamp(currentVitals.respiratoryRate, 8, 40);
            currentVitals.temperature = Mathf.Clamp(currentVitals.temperature, 95f, 110f);
        }
        
        private void UpdatePatientState()
        {
            // Determine patient state based on vital signs
            bool criticalHR = currentVitals.heartRate < 50 || currentVitals.heartRate > 140;
            bool criticalBP = currentVitals.systolicBP < 90 || currentVitals.systolicBP > 180;
            bool criticalO2 = currentVitals.oxygenSaturation < 85;
            bool highPain = currentVitals.painLevel > 7;
            
            if (criticalHR || criticalBP || criticalO2)
            {
                currentState = PatientState.Critical;
            }
            else if (currentVitals.heartRate > 100 || currentVitals.painLevel > 5)
            {
                currentState = PatientState.Deteriorating;
            }
            else if (currentVitals.heartRate < 70 && currentVitals.painLevel < 3)
            {
                currentState = PatientState.Improving;
            }
            else
            {
                currentState = PatientState.Stable;
            }
            
            // Update animations based on state
            UpdateAnimationState();
        }
        
        private void UpdateAnimationState()
        {
            if (patientAnimator == null)
                return;
                
            patientAnimator.SetFloat("HeartRate", currentVitals.heartRate);
            patientAnimator.SetFloat("RespiratoryRate", currentVitals.respiratoryRate);
            patientAnimator.SetFloat("PainLevel", currentVitals.painLevel / 10f);
            patientAnimator.SetFloat("ConsciousnessLevel", consciousnessLevel);
            
            // Set state animations
            patientAnimator.SetBool("IsCritical", currentState == PatientState.Critical);
            patientAnimator.SetBool("IsUnconscious", currentState == PatientState.Unconscious);
        }
        
        private void UpdateVitalSignsEffects()
        {
            // Update breathing effects
            if (breathingEffect != null)
            {
                var emission = breathingEffect.emission;
                emission.rateOverTime = currentVitals.respiratoryRate / 4f;
            }
            
            // Update skin appearance
            UpdateSkinAppearance();
            
            // Play appropriate sounds
            PlayVitalSoundsEffects();
        }
        
        private void UpdateSkinAppearance()
        {
            if (patientSkin == null)
                return;
                
            Material skinMaterial = normalSkin;
            
            // Change skin color based on oxygen saturation and other factors
            if (currentVitals.oxygenSaturation < 90)
            {
                skinMaterial = paleSkin; // Cyanotic
            }
            else if (currentVitals.temperature > 100f || currentVitals.heartRate > 120)
            {
                skinMaterial = flushedSkin; // Feverish/flushed
            }
            
            patientSkin.material = skinMaterial;
        }
        
        private void PlayVitalSoundsEffects()
        {
            // Play breathing sounds based on respiratory rate
            if (breathingSounds.Length > 0 && patientVoice != null)
            {
                if (currentVitals.respiratoryRate > 25) // Rapid breathing
                {
                    if (!patientVoice.isPlaying)
                    {
                        patientVoice.clip = breathingSounds[1]; // Labored breathing
                        patientVoice.Play();
                    }
                }
            }
        }
        
        private IEnumerator SimulateSymptoms()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(10f, 30f));
                
                // Randomly trigger symptoms based on patient condition
                if (currentProfile != null && Random.Range(0f, 1f) < 0.3f)
                {
                    TriggerSymptom();
                }
            }
        }
        
        private void TriggerSymptom()
        {
            if (currentProfile == null || patientAnimator == null)
                return;
                
            // Trigger random symptoms based on pain level and condition
            if (currentVitals.painLevel > 5)
            {
                // Show pain reaction
                patientAnimator.SetTrigger("ShowPain");
                PlayPainSound();
                
                // Generate patient response through GPT
                if (gptIntegration != null)
                {
                    string painPrompt = $"Patient {currentProfile.name} is experiencing pain level {currentVitals.painLevel}/10. Generate a realistic patient response describing their pain.";
                    gptIntegration.SendMessage(painPrompt, OnPainResponse);
                }
            }
        }
        
        private void PlayPainSound()
        {
            if (painSounds.Length > 0 && patientVoice != null)
            {
                AudioClip painClip = painSounds[Random.Range(0, painSounds.Length)];
                patientVoice.PlayOneShot(painClip);
            }
        }
        
        public void RespondToInteraction(string interaction)
        {
            // Generate patient response using GPT
            if (gptIntegration != null && currentProfile != null)
            {
                string responsePrompt = $"Medical student interacts with patient {currentProfile.name} ({currentProfile.personalityType} personality) by {interaction}. Patient has {currentProfile.chiefComplaint}. Generate a realistic patient response.";
                gptIntegration.SendMessage(responsePrompt, (response) => OnPatientInteractionResponse(interaction, response));
            }
        }
        
        public void UpdateVitalsFromVoice(string vitalChanges)
        {
            // Parse vocal commands to update vitals
            // Example: "heart rate 120, blood pressure 140/90"
            Debug.Log($"Updating vitals from voice: {vitalChanges}");
            
            // Simple parsing - you can make this more sophisticated
            if (vitalChanges.ToLower().Contains("heart rate"))
            {
                // Extract heart rate value
            }
            if (vitalChanges.ToLower().Contains("blood pressure"))
            {
                // Extract blood pressure values
            }
        }
        
        private void OnVoiceResponse(string text, AudioClip audio)
        {
            PatientResponded?.Invoke(text, true);
            Debug.Log($"Patient voice response: {text}");
        }
        
        private void OnPatientAction(string action, string parameter)
        {
            Debug.Log($"Patient action triggered: {action} - {parameter}");
        }
        
        private void OnPainResponse(string response)
        {
            PatientResponded?.Invoke(response, false);
            Debug.Log($"Patient pain response: {response}");
        }
        
        private void OnPatientInteractionResponse(string interaction, string response)
        {
            PatientResponded?.Invoke(response, false);
            Debug.Log($"Patient response to {interaction}: {response}");
        }
        
        private string GetCharacterIdForProfile(MedicalScenario.PatientProfile profile)
        {
            // Return appropriate Convai character ID based on patient profile
            // This would be configured based on your Convai characters
            return "default_patient_character_id";
        }
        
        public void ResetPatient()
        {
            currentProfile = null;
            currentVitals = new MedicalScenario.VitalSigns();
            currentState = PatientState.Stable;
            painLevel = 0f;
            consciousnessLevel = 1f;
            
            if (patientAnimator != null)
            {
                patientAnimator.Rebind();
            }
            
            Debug.Log("Patient reset to default state");
        }
        
        public MedicalScenario.VitalSigns GetCurrentVitals()
        {
            return currentVitals;
        }
        
        public PatientState GetCurrentState()
        {
            return currentState;
        }
    }
} 