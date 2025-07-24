using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace MedTrainAI.AI
{
    [System.Serializable]
    public class GPTMessage
    {
        public string role;
        public string content;
        
        public GPTMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
    
    [System.Serializable]
    public class GPTRequest
    {
        public string model = "gpt-4o";
        public List<GPTMessage> messages;
        public float temperature = 0.7f;
        public int max_tokens = 1000;
        
        public GPTRequest()
        {
            messages = new List<GPTMessage>();
        }
    }
    
    [System.Serializable]
    public class GPTResponse
    {
        public Choice[] choices;
        
        [System.Serializable]
        public class Choice
        {
            public GPTMessage message;
        }
    }
    
    public class GPT4OIntegration : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] private string apiKey = ""; // Set in inspector or from environment
        [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";
        
        [Header("Medical Training Context")]
        [TextArea(3, 10)]
        [SerializeField] private string systemPrompt = @"You are an AI medical trainer in a VR environment. You're helping medical students practice clinical scenarios. 
        Provide realistic patient responses, medical guidance, and adapt scenarios based on student actions. 
        Keep responses conversational and educational. Focus on proper medical procedures, patient safety, and clinical reasoning.
        Always maintain professional medical accuracy while being engaging for VR interaction.";
        
        private List<GPTMessage> conversationHistory;
        private bool isProcessing = false;
        
        public delegate void OnResponseReceived(string response);
        public event OnResponseReceived ResponseReceived;
        
        public delegate void OnScenarioGenerated(MedicalScenario scenario);
        public event OnScenarioGenerated ScenarioGenerated;
        
        private void Start()
        {
            conversationHistory = new List<GPTMessage>();
            conversationHistory.Add(new GPTMessage("system", systemPrompt));
            
            // Load API key from environment if not set
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            }
        }
        
        public void SendMessage(string userInput, Action<string> callback = null)
        {
            if (isProcessing)
            {
                Debug.LogWarning("GPT request already in progress");
                return;
            }
            
            StartCoroutine(ProcessMessage(userInput, callback));
        }
        
        public void GenerateMedicalScenario(string scenarioType, string difficulty, Action<MedicalScenario> callback = null)
        {
            string prompt = $@"Generate a realistic medical scenario for VR training:
            Type: {scenarioType}
            Difficulty: {difficulty}
            
            Include:
            - Patient background and symptoms
            - Initial vital signs
            - Required equipment and procedures
            - Learning objectives
            - Potential complications
            - Assessment criteria
            
            Format as a structured medical scenario suitable for VR simulation.";
            
            StartCoroutine(ProcessScenarioGeneration(prompt, callback));
        }
        
        private IEnumerator ProcessMessage(string userInput, Action<string> callback)
        {
            isProcessing = true;
            
            // Add user message to conversation
            conversationHistory.Add(new GPTMessage("user", userInput));
            
            GPTRequest request = new GPTRequest();
            request.messages = new List<GPTMessage>(conversationHistory);
            
            string jsonData = JsonConvert.SerializeObject(request);
            
            using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                
                yield return webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseText = webRequest.downloadHandler.text;
                    GPTResponse response = JsonConvert.DeserializeObject<GPTResponse>(responseText);
                    
                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string aiResponse = response.choices[0].message.content;
                        
                        // Add AI response to conversation history
                        conversationHistory.Add(new GPTMessage("assistant", aiResponse));
                        
                        // Limit conversation history to prevent token overflow
                        if (conversationHistory.Count > 20)
                        {
                            conversationHistory.RemoveRange(1, 2); // Remove oldest user/assistant pair
                        }
                        
                        callback?.Invoke(aiResponse);
                        ResponseReceived?.Invoke(aiResponse);
                        
                        Debug.Log($"GPT-4o Response: {aiResponse}");
                    }
                }
                else
                {
                    Debug.LogError($"GPT-4o API Error: {webRequest.error}");
                    callback?.Invoke("I'm having trouble connecting right now. Please try again.");
                }
            }
            
            isProcessing = false;
        }
        
        private IEnumerator ProcessScenarioGeneration(string prompt, Action<MedicalScenario> callback)
        {
            isProcessing = true;
            
            GPTRequest request = new GPTRequest();
            request.messages.Add(new GPTMessage("system", systemPrompt));
            request.messages.Add(new GPTMessage("user", prompt));
            request.temperature = 0.8f; // More creative for scenario generation
            request.max_tokens = 1500;
            
            string jsonData = JsonConvert.SerializeObject(request);
            
            using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                
                yield return webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseText = webRequest.downloadHandler.text;
                    GPTResponse response = JsonConvert.DeserializeObject<GPTResponse>(responseText);
                    
                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string scenarioText = response.choices[0].message.content;
                        MedicalScenario scenario = ParseScenarioFromText(scenarioText);
                        
                        callback?.Invoke(scenario);
                        ScenarioGenerated?.Invoke(scenario);
                        
                        Debug.Log($"Generated Medical Scenario: {scenario.title}");
                    }
                }
                else
                {
                    Debug.LogError($"Scenario Generation Error: {webRequest.error}");
                }
            }
            
            isProcessing = false;
        }
        
        private MedicalScenario ParseScenarioFromText(string scenarioText)
        {
            // Parse the GPT response into a structured medical scenario
            MedicalScenario scenario = ScriptableObject.CreateInstance<MedicalScenario>();
            
            // Basic parsing - you can make this more sophisticated
            scenario.title = ExtractSection(scenarioText, "Title:", "Patient:");
            scenario.patientBackground = ExtractSection(scenarioText, "Patient:", "Symptoms:");
            scenario.symptoms = ExtractSection(scenarioText, "Symptoms:", "Vital Signs:");
            scenario.learningObjectives = ExtractSection(scenarioText, "Learning Objectives:", "Equipment:");
            
            return scenario;
        }
        
        private string ExtractSection(string text, string startMarker, string endMarker)
        {
            int startIndex = text.IndexOf(startMarker);
            if (startIndex == -1) return "";
            
            startIndex += startMarker.Length;
            int endIndex = text.IndexOf(endMarker, startIndex);
            if (endIndex == -1) endIndex = text.Length;
            
            return text.Substring(startIndex, endIndex - startIndex).Trim();
        }
        
        public void ClearConversationHistory()
        {
            conversationHistory.Clear();
            conversationHistory.Add(new GPTMessage("system", systemPrompt));
        }
        
        public void SetCustomSystemPrompt(string newPrompt)
        {
            systemPrompt = newPrompt;
            ClearConversationHistory();
        }
    }
} 