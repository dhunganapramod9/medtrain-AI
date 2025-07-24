# MedTrain AI - Unity VR Medical Training System

**2nd Place Winner - HackPrinceton 2024**

A comprehensive VR medical training platform for Meta Quest 3S featuring GPT-4o integration, Convai voice interactions, and advanced hand tracking for realistic medical simulations.

## Project Overview

MedTrain AI is an immersive VR medical training application that combines cutting-edge AI with haptic feedback and spatial workflows to create realistic medical learning experiences. The system enables voice-based, LLM-guided medical simulations with adaptive dialogue branching and real-time scenario generation.

### Key Features

- **Meta Quest 3S Integration**: Full hand tracking and spatial mapping
- **GPT-4o AI Integration**: Dynamic scenario generation and medical guidance
- **Convai Voice API**: Natural voice interactions with AI patients
- **Medical Tool Simulation**: Realistic stethoscope, syringe, scalpel interactions
- **Adaptive Dialogue System**: Context-aware branching conversations
- **Real-time Patient Simulation**: Dynamic vital signs and patient responses
- **Hospital Environment**: Fully immersive medical facility with spatial workflows

## Getting Started

### Prerequisites

- Unity 2022.3.0f1 or later
- Meta Quest SDK
- Oculus Integration package
- Meta XR SDK
- Newtonsoft.Json package

### API Keys Required

```bash
# Environment Variables
OPENAI_API_KEY=your_gpt4o_api_key
CONVAI_API_KEY=your_convai_api_key
CONVAI_CHARACTER_ID=your_patient_character_id
```

### Installation

1. **Clone the Repository**

   ```bash
   git clone https://github.com/your-repo/medtrain-ai.git
   cd medtrain-ai
   ```

2. **Open in Unity**

   - Open Unity Hub
   - Add project from disk
   - Select the Unity folder

3. **Install Dependencies**

   - Window → Package Manager
   - Install Meta XR SDK
   - Install Oculus Integration
   - Install Newtonsoft.Json

4. **Configure Build Settings**

   - File → Build Settings
   - Switch to Android platform
   - Set Texture Compression to ASTC
   - Configure XR settings for Quest

5. **Setup Scene**
   - Open `Scenes/MainTrainingScene`
   - Configure VRManager prefab
   - Set API keys in inspector or environment variables

## Project Structure

```
Unity/Scripts/
├── Core/
│   └── VRManager.cs              # Main VR system coordinator
├── AI/
│   ├── GPT4OIntegration.cs       # OpenAI GPT-4o API integration
│   ├── ConvaiIntegration.cs      # Convai voice interaction system
│   └── DialogueSystem.cs         # Adaptive dialogue branching
├── Medical/
│   ├── MedicalScenario.cs        # Scenario definition system
│   ├── ScenarioManager.cs        # Scenario execution and scoring
│   ├── PatientSimulator.cs       # AI-driven patient behavior
│   └── MedicalTool.cs           # Medical instrument interactions
├── Interaction/
│   └── HandTrackingManager.cs    # Meta Quest hand tracking
├── Environment/
│   └── HospitalEnvironment.cs    # Hospital room and equipment setup
└── Utils/
    └── HandGestureRecognizer.cs  # Medical gesture recognition
```

## Core Components

### VRManager

Central coordinator for all VR systems, initializes hand tracking, AI integration, and medical simulations.

```csharp
// Initialize VR systems
VRManager.Instance.IsVRReady();
VRManager.Instance.RestartScenario();
```

### GPT4OIntegration

Handles real-time medical scenario generation and educational guidance.

```csharp
// Generate medical scenario
gptIntegration.GenerateMedicalScenario("Emergency", "Intermediate", callback);

// Send medical query
gptIntegration.SendMessage("Patient shows signs of distress", response => {
    // Handle AI guidance
});
```

### ConvaiIntegration

Manages voice-based patient interactions with natural language processing.

```csharp
// Start voice interaction
convaiIntegration.StartListening();

// Handle patient responses
convaiIntegration.VoiceResponseReceived += (text, audio) => {
    // Process patient voice response
};
```

### HandTrackingManager

Provides precise hand tracking for medical tool manipulation.

```csharp
// Check hand tracking status
bool isTracked = handTrackingManager.IsHandTracked(HandType.Right);

// Get held medical tool
MedicalTool tool = handTrackingManager.GetHeldTool(HandType.Left);
```

## Medical Scenarios

### Creating Scenarios

Medical scenarios are defined using ScriptableObjects:

```csharp
[CreateAssetMenu(fileName = "New Medical Scenario", menuName = "MedTrain AI/Medical Scenario")]
public class MedicalScenario : ScriptableObject
{
    public string title;
    public ScenarioType scenarioType;
    public PatientProfile patientProfile;
    public VitalSigns initialVitals;
    // ... additional properties
}
```

### Scenario Types

- **Emergency**: Critical care situations
- **Surgery**: Surgical procedures and techniques
- **Consultation**: Patient interviews and examinations
- **Diagnostics**: Medical testing and analysis
- **Procedures**: Specific medical interventions

### Assessment System

Scenarios include built-in assessment criteria:

```csharp
// Award points for correct procedures
scenarioManager.AwardPoints("Correct Diagnosis", 25);

// Deduct points for errors
scenarioManager.DeductPoints("Patient Safety", 10);
```

## 🛠Medical Tools

### Supported Instruments

- **Stethoscope**: Heart and lung sound detection
- **Syringe**: Injection procedures
- **Scalpel**: Surgical techniques
- **Thermometer**: Temperature measurement
- **Blood Pressure Cuff**: Vital sign monitoring
- **Otoscope**: Ear examination
- **Reflex Hammer**: Neurological testing

### Tool Usage

```csharp
// Use medical tool
bool success = medicalTool.UseTool(targetPosition, patientObject);

// Check tool-specific actions
if (tool.GetToolType() == MedicalTool.ToolType.Stethoscope)
{
    // Handle stethoscope-specific logic
}
```

## Hand Gesture Recognition

### Medical Gestures

The system recognizes medical-specific hand gestures:

- **Injection Grip**: Proper syringe holding technique
- **Stethoscope Grip**: Correct stethoscope positioning
- **Surgical Grip**: Precision instrument handling
- **Palpation**: Physical examination techniques
- **CPR Position**: Emergency response positioning

### Custom Gestures

```csharp
// Add custom medical gesture
var customGesture = new GesturePattern
{
    name = "blood_pressure_cuff",
    confidenceThreshold = 0.9f,
    isMedicalGesture = true
};
gestureRecognizer.AddCustomGesture(customGesture);
```

## Environment System

### Hospital Rooms

- **Emergency Room**: High-intensity scenarios
- **Operating Room**: Surgical procedures
- **ICU**: Critical care monitoring
- **Patient Room**: Standard care situations
- **Examination Room**: Diagnostic procedures

### Spatial Workflows

The system uses Quest 3S spatial mapping for:

- Room boundary detection
- Equipment placement optimization
- Patient positioning
- Safety zone establishment

## Voice Integration

### Patient Voices

Convai integration provides:

- Natural language patient responses
- Emotion-appropriate voice modulation
- Medical condition-specific speech patterns
- Dynamic conversation adaptation

### Voice Commands

```csharp
// Process voice input
convaiIntegration.ProcessVoiceInput(audioClip);

// Handle patient actions from voice
convaiIntegration.PatientActionTriggered += (action, parameter) => {
    switch(action) {
        case "setvitals":
            patient.UpdateVitalsFromVoice(parameter);
            break;
        case "showpain":
            patient.ShowPainReaction(parameter);
            break;
    }
};
```

## Performance Optimization

### Quest 3S Specific

- Optimized rendering pipeline for mobile VR
- Efficient hand tracking algorithms
- Reduced draw calls for medical instruments
- Level-of-detail (LOD) for patient models

### AI Response Optimization

- Response caching for common scenarios
- Asynchronous API calls
- Context window management
- Token usage optimization

## Testing & Validation

### Medical Accuracy

All medical procedures and scenarios are validated by:

- Licensed medical professionals
- Medical education standards
- Clinical practice guidelines
- Evidence-based protocols

### VR Comfort

- Motion sickness prevention
- Proper IPD adjustment
- Comfortable interaction distances
- Eye strain reduction

## Security & Privacy

### Data Handling

- No PHI (Protected Health Information) storage
- Encrypted API communications
- Local processing where possible
- HIPAA-compliant design principles

### User Safety

- Guardian boundary enforcement
- Safe interaction zones
- Emergency exit procedures
- Comfort break reminders

## Deployment

### Build Configuration

```bash
# Build for Quest 3S
Unity Build Settings:
- Platform: Android
- Architecture: ARM64
- Graphics API: Vulkan
- Scripting Backend: IL2CPP
```

### APK Installation

```bash
adb install MedTrainAI.apk
```



## Awards & Recognition

- **2nd Place - HackPrinceton 2024**
- Featured medical VR innovation
- Advanced AI integration award

##  Support

For technical support or medical validation questions:

- Email: support@medtrain-ai.com
- Documentation: https://docs.medtrain-ai.com
- Community: https://discord.gg/medtrain-ai

## Acknowledgments

- Princeton University HackPrinceton organizers
- Medical advisory board
- Meta Quest development team
- OpenAI and Convai API teams
- Unity VR development community

---

**Built with ❤️ for medical education and VR innovation**
