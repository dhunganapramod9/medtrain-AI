using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MedTrainAI.AI
{
    public class DialogueSystem : MonoBehaviour
    {
        [Header("Dialogue Configuration")]
        public List<DialogueTree> availableDialogueTrees = new List<DialogueTree>();
        public DialogueTree currentDialogue;
        public DialogueNode currentNode;
        
        [Header("AI Integration")]
        public GPT4OIntegration gptIntegration;
        public ConvaiIntegration convaiIntegration;
        
        [Header("Response Settings")]
        public float responseDelay = 1f;
        public bool enableDynamicBranching = true;
        public bool enableContextMemory = true;
        
        private Dictionary<string, object> conversationContext = new Dictionary<string, object>();
        private List<DialogueEntry> conversationHistory = new List<DialogueEntry>();
        private bool isProcessingDialogue = false;
        
        public delegate void OnDialogueEvent(string eventType, DialogueNode node, string response);
        public event OnDialogueEvent DialogueEvent;
        
        [System.Serializable]
        public class DialogueTree
        {
            public string treeName;
            public string description;
            public DialogueNode rootNode;
            public List<DialogueNode> allNodes = new List<DialogueNode>();
        }
        
        [System.Serializable]
        public class DialogueNode
        {
            public string nodeId;
            public string nodeText;
            public DialogueType dialogueType;
            public List<DialogueChoice> choices = new List<DialogueChoice>();
            public List<DialogueCondition> conditions = new List<DialogueCondition>();
            public List<DialogueAction> actions = new List<DialogueAction>();
            public string gptPrompt; // For dynamic content generation
            public bool isDynamic = false;
            
            public enum DialogueType
            {
                Statement,
                Question,
                Choice,
                Action,
                Conditional,
                GPTGenerated
            }
        }
        
        [System.Serializable]
        public class DialogueChoice
        {
            public string choiceText;
            public string targetNodeId;
            public List<DialogueCondition> requirements = new List<DialogueCondition>();
            public int priority = 0;
        }
        
        [System.Serializable]
        public class DialogueCondition
        {
            public string conditionKey;
            public ComparisonType comparison;
            public string expectedValue;
            
            public enum ComparisonType
            {
                Equals,
                NotEquals,
                GreaterThan,
                LessThan,
                Contains,
                Exists
            }
        }
        
        [System.Serializable]
        public class DialogueAction
        {
            public string actionType;
            public string actionParameter;
            public string targetKey;
            public string actionValue;
        }
        
        [System.Serializable]
        public class DialogueEntry
        {
            public string speaker;
            public string text;
            public float timestamp;
            public DialogueNode.DialogueType type;
        }
        
        private void Start()
        {
            InitializeDialogueSystem();
        }
        
        public IEnumerator Initialize()
        {
            Debug.Log("Initializing Dialogue System...");
            
            InitializeDialogueSystem();
            
            // Connect to AI systems
            if (gptIntegration != null)
            {
                gptIntegration.ResponseReceived += OnGPTResponse;
            }
            
            if (convaiIntegration != null)
            {
                convaiIntegration.VoiceResponseReceived += OnVoiceResponse;
            }
            
            yield return null;
            Debug.Log("Dialogue System initialized");
        }
        
        private void InitializeDialogueSystem()
        {
            // Load dialogue trees from resources
            LoadDialogueTrees();
            
            // Initialize conversation context
            SetupConversationContext();
        }
        
        private void LoadDialogueTrees()
        {
            // Load dialogue trees from ScriptableObjects or JSON files
            DialogueTree[] trees = Resources.LoadAll<DialogueTree>("Dialogues");
            availableDialogueTrees.Clear();
            availableDialogueTrees.AddRange(trees);
            
            Debug.Log($"Loaded {availableDialogueTrees.Count} dialogue trees");
        }
        
        private void SetupConversationContext()
        {
            conversationContext.Clear();
            conversationContext["patient_name"] = "Unknown";
            conversationContext["scenario_type"] = "General";
            conversationContext["conversation_stage"] = "Introduction";
            conversationContext["user_competency"] = "Beginner";
        }
        
        public void StartDialogue(string dialogueTreeName, Dictionary<string, object> initialContext = null)
        {
            DialogueTree targetTree = FindDialogueTree(dialogueTreeName);
            if (targetTree != null)
            {
                StartDialogue(targetTree, initialContext);
            }
            else
            {
                Debug.LogWarning($"Dialogue tree not found: {dialogueTreeName}");
            }
        }
        
        public void StartDialogue(DialogueTree dialogueTree, Dictionary<string, object> initialContext = null)
        {
            currentDialogue = dialogueTree;
            currentNode = dialogueTree.rootNode;
            
            // Update context with initial values
            if (initialContext != null)
            {
                foreach (var kvp in initialContext)
                {
                    conversationContext[kvp.Key] = kvp.Value;
                }
            }
            
            // Clear conversation history
            conversationHistory.Clear();
            
            // Start the dialogue
            ProcessCurrentNode();
            
            Debug.Log($"Started dialogue: {dialogueTree.treeName}");
        }
        
        private void ProcessCurrentNode()
        {
            if (currentNode == null || isProcessingDialogue)
                return;
                
            isProcessingDialogue = true;
            
            // Check node conditions
            if (!CheckNodeConditions(currentNode))
            {
                // Find alternative node or end dialogue
                FindAlternativeNode();
                return;
            }
            
            // Execute node actions
            ExecuteNodeActions(currentNode);
            
            // Process different node types
            switch (currentNode.dialogueType)
            {
                case DialogueNode.DialogueType.Statement:
                    ProcessStatement();
                    break;
                case DialogueNode.DialogueType.Question:
                    ProcessQuestion();
                    break;
                case DialogueNode.DialogueType.Choice:
                    ProcessChoice();
                    break;
                case DialogueNode.DialogueType.GPTGenerated:
                    ProcessGPTNode();
                    break;
                default:
                    ProcessGenericNode();
                    break;
            }
        }
        
        private void ProcessStatement()
        {
            string processedText = ProcessTextWithContext(currentNode.nodeText);
            
            // Add to conversation history
            AddToConversationHistory("System", processedText, currentNode.dialogueType);
            
            // Trigger dialogue event
            DialogueEvent?.Invoke("Statement", currentNode, processedText);
            
            // Auto-advance after delay
            StartCoroutine(AutoAdvanceDialogue());
        }
        
        private void ProcessQuestion()
        {
            string processedText = ProcessTextWithContext(currentNode.nodeText);
            
            AddToConversationHistory("System", processedText, currentNode.dialogueType);
            DialogueEvent?.Invoke("Question", currentNode, processedText);
            
            // Wait for user response
            isProcessingDialogue = false;
        }
        
        private void ProcessChoice()
        {
            string processedText = ProcessTextWithContext(currentNode.nodeText);
            
            AddToConversationHistory("System", processedText, currentNode.dialogueType);
            
            // Filter available choices based on conditions
            List<DialogueChoice> availableChoices = GetAvailableChoices(currentNode);
            
            DialogueEvent?.Invoke("Choice", currentNode, processedText);
            
            // Present choices to user
            PresentChoices(availableChoices);
            
            isProcessingDialogue = false;
        }
        
        private void ProcessGPTNode()
        {
            if (gptIntegration != null && !string.IsNullOrEmpty(currentNode.gptPrompt))
            {
                string contextualPrompt = BuildContextualPrompt(currentNode.gptPrompt);
                gptIntegration.SendMessage(contextualPrompt, OnGPTNodeResponse);
            }
            else
            {
                // Fallback to static text
                ProcessStatement();
            }
        }
        
        private void ProcessGenericNode()
        {
            ProcessStatement();
        }
        
        private string BuildContextualPrompt(string basePrompt)
        {
            string contextPrompt = basePrompt;
            
            // Add conversation context
            contextPrompt += $"\n\nContext:";
            foreach (var kvp in conversationContext)
            {
                contextPrompt += $"\n- {kvp.Key}: {kvp.Value}";
            }
            
            // Add recent conversation history
            if (conversationHistory.Count > 0)
            {
                contextPrompt += $"\n\nRecent conversation:";
                int startIndex = Mathf.Max(0, conversationHistory.Count - 3);
                for (int i = startIndex; i < conversationHistory.Count; i++)
                {
                    var entry = conversationHistory[i];
                    contextPrompt += $"\n{entry.speaker}: {entry.text}";
                }
            }
            
            return contextPrompt;
        }
        
        private void OnGPTNodeResponse(string response)
        {
            AddToConversationHistory("System", response, DialogueNode.DialogueType.GPTGenerated);
            DialogueEvent?.Invoke("GPTResponse", currentNode, response);
            
            // Use voice synthesis if available
            if (convaiIntegration != null)
            {
                // Convert to speech through Convai
                // This would depend on your Convai integration setup
            }
            
            StartCoroutine(AutoAdvanceDialogue());
        }
        
        private void OnGPTResponse(string response)
        {
            // Handle general GPT responses
            ProcessUserResponse(response);
        }
        
        private void OnVoiceResponse(string text, AudioClip audio)
        {
            // Handle voice responses from Convai
            AddToConversationHistory("Patient", text, DialogueNode.DialogueType.Statement);
            ProcessUserResponse(text);
        }
        
        public void ProcessUserResponse(string userInput)
        {
            if (isProcessingDialogue)
                return;
                
            AddToConversationHistory("User", userInput, DialogueNode.DialogueType.Statement);
            
            // Update conversation context based on user input
            UpdateContextFromUserInput(userInput);
            
            if (enableDynamicBranching && gptIntegration != null)
            {
                // Use GPT to determine next dialogue action
                string analysisPrompt = $"User said: '{userInput}'. Current dialogue context: {GetContextSummary()}. Determine appropriate response and next dialogue action.";
                gptIntegration.SendMessage(analysisPrompt, OnUserResponseAnalysis);
            }
            else
            {
                // Use predefined logic
                AdvanceDialogueBasedOnInput(userInput);
            }
        }
        
        private void OnUserResponseAnalysis(string analysis)
        {
            // Parse GPT analysis and determine next node
            AdvanceDialogueBasedOnAnalysis(analysis);
        }
        
        private void AdvanceDialogueBasedOnInput(string userInput)
        {
            if (currentNode.choices.Count > 0)
            {
                // Find matching choice
                DialogueChoice selectedChoice = FindBestMatchingChoice(userInput);
                if (selectedChoice != null)
                {
                    MoveToNode(selectedChoice.targetNodeId);
                }
            }
            else
            {
                // Auto-advance to next node
                AdvanceToNextNode();
            }
        }
        
        private void AdvanceDialogueBasedOnAnalysis(string analysis)
        {
            // Simple analysis parsing - you can make this more sophisticated
            if (analysis.ToLower().Contains("advance") || analysis.ToLower().Contains("continue"))
            {
                AdvanceToNextNode();
            }
            else if (analysis.ToLower().Contains("repeat") || analysis.ToLower().Contains("clarify"))
            {
                // Repeat current node
                ProcessCurrentNode();
            }
            else
            {
                // Generate dynamic response
                GenerateDynamicResponse(analysis);
            }
        }
        
        private void GenerateDynamicResponse(string analysis)
        {
            if (gptIntegration != null)
            {
                string responsePrompt = $"Generate appropriate dialogue response based on: {analysis}. Keep it medical and educational.";
                gptIntegration.SendMessage(responsePrompt, (response) =>
                {
                    DialogueEvent?.Invoke("DynamicResponse", currentNode, response);
                    AddToConversationHistory("System", response, DialogueNode.DialogueType.GPTGenerated);
                    
                    // Continue dialogue flow
                    AdvanceToNextNode();
                });
            }
        }
        
        public void SelectChoice(int choiceIndex)
        {
            if (currentNode != null && choiceIndex >= 0 && choiceIndex < currentNode.choices.Count)
            {
                DialogueChoice selectedChoice = currentNode.choices[choiceIndex];
                MoveToNode(selectedChoice.targetNodeId);
            }
        }
        
        public void MoveToNode(string nodeId)
        {
            if (currentDialogue != null)
            {
                DialogueNode targetNode = FindNodeById(nodeId);
                if (targetNode != null)
                {
                    currentNode = targetNode;
                    ProcessCurrentNode();
                }
                else
                {
                    Debug.LogWarning($"Node not found: {nodeId}");
                    EndDialogue();
                }
            }
        }
        
        private void AdvanceToNextNode()
        {
            // Simple linear advancement - you can implement more complex logic
            if (currentDialogue != null)
            {
                int currentIndex = currentDialogue.allNodes.IndexOf(currentNode);
                if (currentIndex >= 0 && currentIndex < currentDialogue.allNodes.Count - 1)
                {
                    currentNode = currentDialogue.allNodes[currentIndex + 1];
                    ProcessCurrentNode();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
        
        private IEnumerator AutoAdvanceDialogue()
        {
            yield return new WaitForSeconds(responseDelay);
            isProcessingDialogue = false;
            
            // Check if we should auto-advance
            if (currentNode.choices.Count == 0)
            {
                AdvanceToNextNode();
            }
        }
        
        private bool CheckNodeConditions(DialogueNode node)
        {
            foreach (var condition in node.conditions)
            {
                if (!EvaluateCondition(condition))
                {
                    return false;
                }
            }
            return true;
        }
        
        private bool EvaluateCondition(DialogueCondition condition)
        {
            if (!conversationContext.ContainsKey(condition.conditionKey))
            {
                return condition.comparison == DialogueCondition.ComparisonType.NotEquals;
            }
            
            object contextValue = conversationContext[condition.conditionKey];
            string contextString = contextValue?.ToString() ?? "";
            
            switch (condition.comparison)
            {
                case DialogueCondition.ComparisonType.Equals:
                    return contextString.Equals(condition.expectedValue);
                case DialogueCondition.ComparisonType.NotEquals:
                    return !contextString.Equals(condition.expectedValue);
                case DialogueCondition.ComparisonType.Contains:
                    return contextString.ToLower().Contains(condition.expectedValue.ToLower());
                case DialogueCondition.ComparisonType.Exists:
                    return conversationContext.ContainsKey(condition.conditionKey);
                default:
                    return true;
            }
        }
        
        private void ExecuteNodeActions(DialogueNode node)
        {
            foreach (var action in node.actions)
            {
                ExecuteAction(action);
            }
        }
        
        private void ExecuteAction(DialogueAction action)
        {
            switch (action.actionType.ToLower())
            {
                case "setcontext":
                    conversationContext[action.targetKey] = action.actionValue;
                    break;
                case "addpoints":
                    // Award points in scenario
                    var scenarioManager = FindObjectOfType<MedTrainAI.Medical.ScenarioManager>();
                    if (scenarioManager != null)
                    {
                        int points = int.TryParse(action.actionValue, out points) ? points : 0;
                        scenarioManager.AwardPoints(action.targetKey, points);
                    }
                    break;
                case "triggerevent":
                    DialogueEvent?.Invoke("ActionTriggered", currentNode, action.actionParameter);
                    break;
            }
        }
        
        private string ProcessTextWithContext(string text)
        {
            string processedText = text;
            
            // Replace context variables in text
            foreach (var kvp in conversationContext)
            {
                string placeholder = $"{{{kvp.Key}}}";
                if (processedText.Contains(placeholder))
                {
                    processedText = processedText.Replace(placeholder, kvp.Value.ToString());
                }
            }
            
            return processedText;
        }
        
        private List<DialogueChoice> GetAvailableChoices(DialogueNode node)
        {
            List<DialogueChoice> availableChoices = new List<DialogueChoice>();
            
            foreach (var choice in node.choices)
            {
                bool choiceAvailable = true;
                foreach (var requirement in choice.requirements)
                {
                    if (!EvaluateCondition(requirement))
                    {
                        choiceAvailable = false;
                        break;
                    }
                }
                
                if (choiceAvailable)
                {
                    availableChoices.Add(choice);
                }
            }
            
            // Sort by priority
            availableChoices.Sort((a, b) => b.priority.CompareTo(a.priority));
            
            return availableChoices;
        }
        
        private void PresentChoices(List<DialogueChoice> choices)
        {
            DialogueEvent?.Invoke("ChoicesPresented", currentNode, string.Join(";", choices.ConvertAll(c => c.choiceText)));
        }
        
        private DialogueChoice FindBestMatchingChoice(string userInput)
        {
            DialogueChoice bestMatch = null;
            float bestScore = 0f;
            
            foreach (var choice in currentNode.choices)
            {
                float similarity = CalculateTextSimilarity(userInput.ToLower(), choice.choiceText.ToLower());
                if (similarity > bestScore)
                {
                    bestScore = similarity;
                    bestMatch = choice;
                }
            }
            
            return bestScore > 0.3f ? bestMatch : null; // Threshold for matching
        }
        
        private float CalculateTextSimilarity(string text1, string text2)
        {
            // Simple word overlap calculation
            string[] words1 = text1.Split(' ');
            string[] words2 = text2.Split(' ');
            
            int matches = 0;
            foreach (string word1 in words1)
            {
                foreach (string word2 in words2)
                {
                    if (word1.Equals(word2) && word1.Length > 2)
                    {
                        matches++;
                        break;
                    }
                }
            }
            
            return (float)matches / Mathf.Max(words1.Length, words2.Length);
        }
        
        private void UpdateContextFromUserInput(string userInput)
        {
            // Simple context extraction - you can make this more sophisticated
            if (userInput.ToLower().Contains("pain"))
            {
                conversationContext["mentioned_pain"] = true;
            }
            if (userInput.ToLower().Contains("help"))
            {
                conversationContext["needs_help"] = true;
            }
            
            conversationContext["last_user_input"] = userInput;
        }
        
        private string GetContextSummary()
        {
            return string.Join(", ", conversationContext.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        }
        
        private DialogueTree FindDialogueTree(string treeName)
        {
            return availableDialogueTrees.Find(tree => tree.treeName.Equals(treeName, StringComparison.OrdinalIgnoreCase));
        }
        
        private DialogueNode FindNodeById(string nodeId)
        {
            if (currentDialogue != null)
            {
                return currentDialogue.allNodes.Find(node => node.nodeId.Equals(nodeId));
            }
            return null;
        }
        
        private void FindAlternativeNode()
        {
            // Find next available node or end dialogue
            AdvanceToNextNode();
        }
        
        private void AddToConversationHistory(string speaker, string text, DialogueNode.DialogueType type)
        {
            conversationHistory.Add(new DialogueEntry
            {
                speaker = speaker,
                text = text,
                timestamp = Time.time,
                type = type
            });
            
            // Limit history size
            if (conversationHistory.Count > 50)
            {
                conversationHistory.RemoveAt(0);
            }
        }
        
        public void EndDialogue()
        {
            currentDialogue = null;
            currentNode = null;
            isProcessingDialogue = false;
            
            DialogueEvent?.Invoke("DialogueEnded", null, "");
            
            Debug.Log("Dialogue ended");
        }
        
        public void PauseSystem()
        {
            isProcessingDialogue = true;
        }
        
        public void ResumeSystem()
        {
            isProcessingDialogue = false;
        }
        
        public bool IsDialogueActive()
        {
            return currentDialogue != null && currentNode != null;
        }
        
        public List<DialogueEntry> GetConversationHistory()
        {
            return new List<DialogueEntry>(conversationHistory);
        }
    }
} 