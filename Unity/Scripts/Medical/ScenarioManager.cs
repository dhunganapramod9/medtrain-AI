using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MedTrainAI.AI;

namespace MedTrainAI.Medical
{
    public class ScenarioManager : MonoBehaviour
    {
        [Header("Scenario Database")]
        public List<MedicalScenario> availableScenarios = new List<MedicalScenario>();
        public MedicalScenario currentScenario;
        
        [Header("Scenario State")]
        public int currentPhaseIndex = 0;
        public bool scenarioActive = false;
        public float scenarioStartTime;
        public Dictionary<string, int> currentScore = new Dictionary<string, int>();
        
        [Header("Components")]
        public PatientSimulator patientSimulator;
        public HospitalEnvironment hospitalEnvironment;
        public GPT4OIntegration gptIntegration;
        
        [Header("UI References")]
        public ScenarioUI scenarioUI;
        public VitalSignsMonitor vitalSignsMonitor;
        
        public delegate void OnScenarioEvent(string eventType, object data);
        public event OnScenarioEvent ScenarioEvent;
        
        public delegate void OnPhaseChange(int oldPhase, int newPhase);
        public event OnPhaseChange PhaseChanged;
        
        public delegate void OnScenarioComplete(int finalScore, int maxScore);
        public event OnScenarioComplete ScenarioCompleted;
        
        private void Start()
        {
            LoadAvailableScenarios();
        }
        
        public IEnumerator Initialize()
        {
            Debug.Log("Initializing Scenario Manager...");
            
            // Load scenarios from Resources folder
            LoadAvailableScenarios();
            
            // Setup UI connections
            if (scenarioUI != null)
            {
                scenarioUI.OnScenarioSelected += StartScenario;
                scenarioUI.OnScenarioRestart += RestartCurrentScenario;
            }
            
            yield return null;
            Debug.Log("Scenario Manager initialized");
        }
        
        private void LoadAvailableScenarios()
        {
            // Load all medical scenarios from Resources
            MedicalScenario[] scenarios = Resources.LoadAll<MedicalScenario>("Scenarios");
            availableScenarios.Clear();
            availableScenarios.AddRange(scenarios);
            
            Debug.Log($"Loaded {availableScenarios.Count} medical scenarios");
        }
        
        public void StartScenario(MedicalScenario scenario)
        {
            if (scenarioActive)
            {
                EndCurrentScenario();
            }
            
            currentScenario = scenario;
            currentPhaseIndex = 0;
            scenarioActive = true;
            scenarioStartTime = Time.time;
            
            // Initialize scoring
            InitializeScoring();
            
            // Setup patient
            if (patientSimulator != null)
            {
                patientSimulator.LoadPatientProfile(scenario.patientProfile);
                patientSimulator.SetVitalSigns(scenario.initialVitals);
            }
            
            // Setup environment
            if (hospitalEnvironment != null)
            {
                hospitalEnvironment.SetupForScenario(scenario);
            }
            
            // Update UI
            if (scenarioUI != null)
            {
                scenarioUI.DisplayScenarioInfo(scenario);
            }
            
            // Start first phase
            StartPhase(0);
            
            // Generate dynamic content with GPT-4o
            if (gptIntegration != null)
            {
                string contextPrompt = $"Starting medical scenario: {scenario.title}. Patient: {scenario.patientProfile.name}, {scenario.patientProfile.age} years old, presenting with {scenario.patientProfile.chiefComplaint}. Provide an opening scenario description.";
                gptIntegration.SendMessage(contextPrompt, OnGPTScenarioStart);
            }
            
            ScenarioEvent?.Invoke("ScenarioStarted", scenario);
            
            Debug.Log($"Started scenario: {scenario.title}");
        }
        
        public void StartRandomScenario()
        {
            if (availableScenarios.Count > 0)
            {
                MedicalScenario randomScenario = availableScenarios[Random.Range(0, availableScenarios.Count)];
                StartScenario(randomScenario);
            }
        }
        
        public void StartPhase(int phaseIndex)
        {
            if (currentScenario == null || phaseIndex >= currentScenario.phases.Count)
                return;
                
            int oldPhase = currentPhaseIndex;
            currentPhaseIndex = phaseIndex;
            
            MedicalScenario.ScenarioPhase phase = currentScenario.phases[phaseIndex];
            
            // Update patient vitals for this phase
            if (patientSimulator != null && phase.targetVitals != null)
            {
                patientSimulator.TransitionToVitals(phase.targetVitals, phase.estimatedDuration);
            }
            
            // Update UI
            if (scenarioUI != null)
            {
                scenarioUI.DisplayPhaseInfo(phase, phaseIndex + 1, currentScenario.phases.Count);
            }
            
            PhaseChanged?.Invoke(oldPhase, phaseIndex);
            ScenarioEvent?.Invoke("PhaseStarted", phase);
            
            // Auto-progress if specified
            if (phase.autoProgress && phase.estimatedDuration > 0)
            {
                StartCoroutine(AutoProgressPhase(phase.estimatedDuration));
            }
            
            Debug.Log($"Started phase {phaseIndex + 1}: {phase.phaseName}");
        }
        
        private IEnumerator AutoProgressPhase(float duration)
        {
            yield return new WaitForSeconds(duration * 60f); // Convert minutes to seconds
            
            if (scenarioActive && currentPhaseIndex < currentScenario.phases.Count - 1)
            {
                StartPhase(currentPhaseIndex + 1);
            }
            else if (currentPhaseIndex >= currentScenario.phases.Count - 1)
            {
                CompleteScenario();
            }
        }
        
        public void NextPhase()
        {
            if (currentPhaseIndex < currentScenario.phases.Count - 1)
            {
                StartPhase(currentPhaseIndex + 1);
            }
            else
            {
                CompleteScenario();
            }
        }
        
        public void CompleteScenario()
        {
            if (!scenarioActive || currentScenario == null)
                return;
                
            scenarioActive = false;
            
            // Calculate final score
            int finalScore = CalculateFinalScore();
            int maxScore = currentScenario.GetTotalMaxScore();
            
            // Record completion time
            float completionTime = Time.time - scenarioStartTime;
            
            // Update UI
            if (scenarioUI != null)
            {
                scenarioUI.DisplayScenarioResults(finalScore, maxScore, completionTime);
            }
            
            ScenarioCompleted?.Invoke(finalScore, maxScore);
            ScenarioEvent?.Invoke("ScenarioCompleted", new { score = finalScore, maxScore = maxScore, time = completionTime });
            
            Debug.Log($"Scenario completed! Score: {finalScore}/{maxScore} in {completionTime:F1} seconds");
        }
        
        public void RestartCurrentScenario()
        {
            if (currentScenario != null)
            {
                StartScenario(currentScenario);
            }
        }
        
        public void EndCurrentScenario()
        {
            if (scenarioActive)
            {
                scenarioActive = false;
                currentScenario = null;
                currentPhaseIndex = 0;
                
                // Reset patient
                if (patientSimulator != null)
                {
                    patientSimulator.ResetPatient();
                }
                
                ScenarioEvent?.Invoke("ScenarioEnded", null);
            }
        }
        
        private void InitializeScoring()
        {
            currentScore.Clear();
            
            if (currentScenario != null)
            {
                foreach (var criteria in currentScenario.assessmentPoints)
                {
                    currentScore[criteria.criteriaName] = 0;
                }
            }
        }
        
        public void AwardPoints(string criteriaName, int points)
        {
            if (currentScore.ContainsKey(criteriaName))
            {
                currentScore[criteriaName] = Mathf.Min(currentScore[criteriaName] + points, 
                    GetMaxPointsForCriteria(criteriaName));
                
                ScenarioEvent?.Invoke("PointsAwarded", new { criteria = criteriaName, points = points });
                
                Debug.Log($"Awarded {points} points for {criteriaName}");
            }
        }
        
        public void DeductPoints(string criteriaName, int points)
        {
            if (currentScore.ContainsKey(criteriaName))
            {
                currentScore[criteriaName] = Mathf.Max(currentScore[criteriaName] - points, 0);
                
                ScenarioEvent?.Invoke("PointsDeducted", new { criteria = criteriaName, points = points });
                
                Debug.Log($"Deducted {points} points from {criteriaName}");
            }
        }
        
        private int GetMaxPointsForCriteria(string criteriaName)
        {
            if (currentScenario != null)
            {
                foreach (var criteria in currentScenario.assessmentPoints)
                {
                    if (criteria.criteriaName == criteriaName)
                    {
                        return criteria.maxPoints;
                    }
                }
            }
            return 0;
        }
        
        private int CalculateFinalScore()
        {
            int total = 0;
            foreach (var score in currentScore.Values)
            {
                total += score;
            }
            return total;
        }
        
        private void OnGPTScenarioStart(string response)
        {
            // Display GPT-generated scenario introduction
            if (scenarioUI != null)
            {
                scenarioUI.DisplayGPTContent(response);
            }
            
            Debug.Log($"GPT Scenario Introduction: {response}");
        }
        
        public void ProcessAction(string actionName, object actionData = null)
        {
            if (!scenarioActive || currentScenario == null)
                return;
                
            // Use GPT to evaluate the action
            if (gptIntegration != null)
            {
                string actionPrompt = $"Medical student performed action: {actionName} during {currentScenario.title} scenario. Patient has {currentScenario.patientProfile.chiefComplaint}. Evaluate this action and provide feedback.";
                gptIntegration.SendMessage(actionPrompt, (response) => OnActionEvaluated(actionName, response));
            }
            
            // Check if action matches expected actions for current phase
            if (currentPhaseIndex < currentScenario.phases.Count)
            {
                var currentPhase = currentScenario.phases[currentPhaseIndex];
                if (currentPhase.expectedActions.Contains(actionName))
                {
                    AwardPoints("Correct Procedures", 10);
                }
            }
            
            ScenarioEvent?.Invoke("ActionPerformed", new { action = actionName, data = actionData });
        }
        
        private void OnActionEvaluated(string actionName, string evaluation)
        {
            Debug.Log($"Action {actionName} evaluated: {evaluation}");
            
            if (scenarioUI != null)
            {
                scenarioUI.DisplayActionFeedback(actionName, evaluation);
            }
        }
        
        public MedicalScenario GetCurrentScenario()
        {
            return currentScenario;
        }
        
        public bool IsScenarioActive()
        {
            return scenarioActive;
        }
        
        public int GetCurrentPhase()
        {
            return currentPhaseIndex;
        }
        
        public float GetElapsedTime()
        {
            return scenarioActive ? Time.time - scenarioStartTime : 0f;
        }
        
        public Dictionary<string, int> GetCurrentScore()
        {
            return new Dictionary<string, int>(currentScore);
        }
    }
} 