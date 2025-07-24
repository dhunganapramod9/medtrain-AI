using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;

namespace MedTrainAI.AI
{
    [System.Serializable]
    public class ConvaiRequest
    {
        public string sessionId;
        public string text;
        public string characterId;
        public AudioConfig audioConfig;
        
        [System.Serializable]
        public class AudioConfig
        {
            public string sampleRate = "22050";
            public string encoding = "LINEAR16";
        }
    }
    
    [System.Serializable]
    public class ConvaiResponse
    {
        public string sessionId;
        public AudioResponse audioResponse;
        public string text;
        public ActionResponse[] actionResponses;
        
        [System.Serializable]
        public class AudioResponse
        {
            public string audioData; // Base64 encoded audio
            public string contentType;
        }
        
        [System.Serializable]
        public class ActionResponse
        {
            public string action;
            public string parameter;
        }
    }
    
    public class ConvaiIntegration : MonoBehaviour
    {
        [Header("Convai Configuration")]
        [SerializeField] private string apiKey = "";
        [SerializeField] private string characterId = ""; // Patient character ID
        [SerializeField] private string apiUrl = "https://api.convai.com/character/getResponse";
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip microphoneClip;
        [SerializeField] private string microphoneDevice;
        [SerializeField] private int sampleRate = 22050;
        [SerializeField] private float recordTime = 10f;
        
        [Header("Voice Recognition")]
        [SerializeField] private bool isRecording = false;
        [SerializeField] private bool enableVoiceActivation = true;
        [SerializeField] private float voiceThreshold = 0.02f;
        
        private string sessionId;
        private bool isProcessing = false;
        private Queue<AudioClip> audioQueue;
        private GPT4OIntegration gptIntegration;
        
        public delegate void OnVoiceResponse(string text, AudioClip audio);
        public event OnVoiceResponse VoiceResponseReceived;
        
        public delegate void OnPatientAction(string action, string parameter);
        public event OnPatientAction PatientActionTriggered;
        
        private void Start()
        {
            InitializeConvai();
            audioQueue = new Queue<AudioClip>();
            gptIntegration = FindObjectOfType<GPT4OIntegration>();
            
            // Load API credentials
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("CONVAI_API_KEY");
            }
            
            if (string.IsNullOrEmpty(characterId))
            {
                characterId = Environment.GetEnvironmentVariable("CONVAI_CHARACTER_ID");
            }
        }
        
        private void InitializeConvai()
        {
            sessionId = System.Guid.NewGuid().ToString();
            
            // Initialize microphone
            if (Microphone.devices.Length > 0)
            {
                microphoneDevice = Microphone.devices[0];
                Debug.Log($"Using microphone: {microphoneDevice}");
            }
            else
            {
                Debug.LogWarning("No microphone devices found!");
            }
        }
        
        private void Update()
        {
            // Process audio queue
            if (audioQueue.Count > 0 && !audioSource.isPlaying)
            {
                AudioClip nextClip = audioQueue.Dequeue();
                PlayAudioClip(nextClip);
            }
            
            // Voice activation detection
            if (enableVoiceActivation && !isRecording && !isProcessing)
            {
                CheckForVoiceActivation();
            }
        }
        
        public void StartListening()
        {
            if (isRecording || string.IsNullOrEmpty(microphoneDevice))
                return;
                
            Debug.Log("Starting voice recording...");
            isRecording = true;
            microphoneClip = Microphone.Start(microphoneDevice, false, (int)recordTime, sampleRate);
        }
        
        public void StopListening()
        {
            if (!isRecording)
                return;
                
            Debug.Log("Stopping voice recording...");
            Microphone.End(microphoneDevice);
            isRecording = false;
            
            if (microphoneClip != null)
            {
                ProcessVoiceInput(microphoneClip);
            }
        }
        
        private void CheckForVoiceActivation()
        {
            if (Microphone.IsRecording(microphoneDevice))
                return;
                
            // Start a brief recording to detect voice activity
            AudioClip testClip = Microphone.Start(microphoneDevice, false, 1, sampleRate);
            StartCoroutine(CheckVoiceLevel(testClip));
        }
        
        private IEnumerator CheckVoiceLevel(AudioClip clip)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (clip != null)
            {
                float[] samples = new float[clip.samples];
                clip.GetData(samples, 0);
                
                float level = 0f;
                for (int i = 0; i < samples.Length; i++)
                {
                    level += Mathf.Abs(samples[i]);
                }
                level /= samples.Length;
                
                if (level > voiceThreshold)
                {
                    Microphone.End(microphoneDevice);
                    StartListening();
                }
                else
                {
                    Microphone.End(microphoneDevice);
                }
            }
        }
        
        private void ProcessVoiceInput(AudioClip audioClip)
        {
            if (isProcessing)
                return;
                
            StartCoroutine(SendVoiceToConvai(audioClip));
        }
        
        private IEnumerator SendVoiceToConvai(AudioClip audioClip)
        {
            isProcessing = true;
            
            // Convert audio to base64
            string audioData = ConvertAudioToBase64(audioClip);
            
            ConvaiRequest request = new ConvaiRequest
            {
                sessionId = sessionId,
                characterId = characterId,
                audioConfig = new ConvaiRequest.AudioConfig()
            };
            
            // If we have GPT integration, get enhanced context
            if (gptIntegration != null)
            {
                // This would be set based on current medical scenario context
                request.text = "Process this medical training interaction with current scenario context";
            }
            
            string jsonData = JsonConvert.SerializeObject(request);
            
            using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("CONVAI-API-KEY", apiKey);
                
                yield return webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseText = webRequest.downloadHandler.text;
                    ConvaiResponse response = JsonConvert.DeserializeObject<ConvaiResponse>(responseText);
                    
                    ProcessConvaiResponse(response);
                }
                else
                {
                    Debug.LogError($"Convai API Error: {webRequest.error}");
                }
            }
            
            isProcessing = false;
        }
        
        private void ProcessConvaiResponse(ConvaiResponse response)
        {
            // Process text response
            if (!string.IsNullOrEmpty(response.text))
            {
                Debug.Log($"Patient says: {response.text}");
                
                // Send to GPT for enhanced medical context if needed
                if (gptIntegration != null)
                {
                    gptIntegration.SendMessage($"Patient response: {response.text}", (gptResponse) =>
                    {
                        // Process GPT-enhanced response
                        Debug.Log($"Enhanced medical context: {gptResponse}");
                    });
                }
            }
            
            // Process audio response
            if (response.audioResponse != null && !string.IsNullOrEmpty(response.audioResponse.audioData))
            {
                AudioClip responseAudio = ConvertBase64ToAudio(response.audioResponse.audioData);
                audioQueue.Enqueue(responseAudio);
                
                VoiceResponseReceived?.Invoke(response.text, responseAudio);
            }
            
            // Process action responses (patient movements, vital changes, etc.)
            if (response.actionResponses != null)
            {
                foreach (var action in response.actionResponses)
                {
                    PatientActionTriggered?.Invoke(action.action, action.parameter);
                    ProcessPatientAction(action.action, action.parameter);
                }
            }
        }
        
        private void ProcessPatientAction(string action, string parameter)
        {
            switch (action.ToLower())
            {
                case "setvitals":
                    // Update patient vital signs
                    var patientSim = FindObjectOfType<PatientSimulator>();
                    if (patientSim != null)
                    {
                        patientSim.UpdateVitalsFromVoice(parameter);
                    }
                    break;
                    
                case "showpain":
                    // Trigger pain animation
                    var patientAnimator = FindObjectOfType<PatientAnimator>();
                    if (patientAnimator != null)
                    {
                        patientAnimator.ShowPainReaction(parameter);
                    }
                    break;
                    
                case "requesthelp":
                    // Patient is requesting help
                    Debug.Log($"Patient requesting help: {parameter}");
                    break;
                    
                default:
                    Debug.Log($"Unknown patient action: {action} with parameter: {parameter}");
                    break;
            }
        }
        
        private string ConvertAudioToBase64(AudioClip audioClip)
        {
            float[] samples = new float[audioClip.samples];
            audioClip.GetData(samples, 0);
            
            // Convert float samples to 16-bit PCM
            byte[] pcmData = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = (short)(samples[i] * 32767f);
                pcmData[i * 2] = (byte)(sample & 0xFF);
                pcmData[i * 2 + 1] = (byte)(sample >> 8);
            }
            
            return Convert.ToBase64String(pcmData);
        }
        
        private AudioClip ConvertBase64ToAudio(string base64AudioData)
        {
            byte[] audioBytes = Convert.FromBase64String(base64AudioData);
            
            // Convert bytes back to float array
            float[] samples = new float[audioBytes.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = (short)(audioBytes[i * 2] | (audioBytes[i * 2 + 1] << 8));
                samples[i] = sample / 32767f;
            }
            
            AudioClip clip = AudioClip.Create("ConvaiResponse", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            
            return clip;
        }
        
        private void PlayAudioClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
        
        public void SetCharacter(string newCharacterId)
        {
            characterId = newCharacterId;
            sessionId = System.Guid.NewGuid().ToString(); // Reset session
        }
        
        public void SetVoiceActivation(bool enabled)
        {
            enableVoiceActivation = enabled;
        }
    }
} 