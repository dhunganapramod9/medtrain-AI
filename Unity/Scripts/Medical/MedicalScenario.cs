using UnityEngine;
using System.Collections.Generic;

namespace MedTrainAI.Medical
{
    [CreateAssetMenu(fileName = "New Medical Scenario", menuName = "MedTrain AI/Medical Scenario")]
    public class MedicalScenario : ScriptableObject
    {
        [Header("Scenario Information")]
        public string title;
        public string description;
        [TextArea(3, 6)]
        public string patientBackground;
        [TextArea(2, 4)]
        public string symptoms;
        [TextArea(2, 4)]
        public string learningObjectives;
        
        [Header("Scenario Settings")]
        public ScenarioType scenarioType;
        public DifficultyLevel difficulty;
        public float estimatedDuration = 15f; // in minutes
        public bool requiresSpecialEquipment;
        
        [Header("Patient Information")]
        public PatientProfile patientProfile;
        public VitalSigns initialVitals;
        public List<string> allergies = new List<string>();
        public List<string> currentMedications = new List<string>();
        
        [Header("Equipment Requirements")]
        public List<RequiredEquipment> requiredEquipment = new List<RequiredEquipment>();
        
        [Header("Assessment Criteria")]
        public List<AssessmentCriteria> assessmentPoints = new List<AssessmentCriteria>();
        
        [Header("Scenario Progression")]
        public List<ScenarioPhase> phases = new List<ScenarioPhase>();
        
        [Header("Audio/Visual")]
        public AudioClip backgroundAudio;
        public Sprite scenarioIcon;
        
        public enum ScenarioType
        {
            Emergency,
            Surgery,
            Consultation,
            Examination,
            Diagnostics,
            Procedures,
            PatientCare
        }
        
        public enum DifficultyLevel
        {
            Beginner,
            Intermediate,
            Advanced,
            Expert
        }
        
        [System.Serializable]
        public class PatientProfile
        {
            public string name;
            public int age;
            public Gender gender;
            public string medicalHistory;
            public string chiefComplaint;
            public Personality personalityType;
            
            public enum Gender
            {
                Male,
                Female,
                Other
            }
            
            public enum Personality
            {
                Calm,
                Anxious,
                Cooperative,
                Difficult,
                Confused,
                Aggressive
            }
        }
        
        [System.Serializable]
        public class VitalSigns
        {
            [Range(0, 200)]
            public int heartRate = 72;
            [Range(0, 250)]
            public int systolicBP = 120;
            [Range(0, 150)]
            public int diastolicBP = 80;
            [Range(0, 60)]
            public int respiratoryRate = 16;
            [Range(90, 110)]
            public float temperature = 98.6f;
            [Range(70, 100)]
            public int oxygenSaturation = 98;
            [Range(0, 10)]
            public int painLevel = 0;
            
            public string GetVitalSignsString()
            {
                return $"HR: {heartRate}, BP: {systolicBP}/{diastolicBP}, RR: {respiratoryRate}, Temp: {temperature}°F, O2: {oxygenSaturation}%, Pain: {painLevel}/10";
            }
        }
        
        [System.Serializable]
        public class RequiredEquipment
        {
            public string equipmentName;
            public bool isEssential;
            public string purpose;
        }
        
        [System.Serializable]
        public class AssessmentCriteria
        {
            public string criteriaName;
            public string description;
            public int maxPoints;
            public bool isRequired;
        }
        
        [System.Serializable]
        public class ScenarioPhase
        {
            public string phaseName;
            public string description;
            public float estimatedDuration;
            public List<string> expectedActions = new List<string>();
            public List<string> possibleComplications = new List<string>();
            public VitalSigns targetVitals;
            public bool autoProgress;
        }
        
        public int GetTotalMaxScore()
        {
            int total = 0;
            foreach (var criteria in assessmentPoints)
            {
                total += criteria.maxPoints;
            }
            return total;
        }
        
        public bool HasRequiredEquipment(List<string> availableEquipment)
        {
            foreach (var equipment in requiredEquipment)
            {
                if (equipment.isEssential && !availableEquipment.Contains(equipment.equipmentName))
                {
                    return false;
                }
            }
            return true;
        }
        
        public ScenarioPhase GetPhase(int phaseIndex)
        {
            if (phaseIndex >= 0 && phaseIndex < phases.Count)
            {
                return phases[phaseIndex];
            }
            return null;
        }
        
        public int GetPhaseCount()
        {
            return phases.Count;
        }
    }
} 