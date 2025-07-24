using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MedTrainAI.Interaction
{
    public class HandGestureRecognizer : MonoBehaviour
    {
        [Header("Gesture Recognition Settings")]
        public float gestureThreshold = 0.8f;
        public float gestureHoldTime = 0.5f;
        public bool enableDebugVisualization = false;
        
        [Header("Medical Gesture Detection")]
        public bool detectMedicalGestures = true;
        public float medicalGestureAccuracy = 0.9f;
        
        private Dictionary<string, GesturePattern> gesturePatterns;
        private Dictionary<string, float> gestureTimers = new Dictionary<string, float>();
        private string lastRecognizedGesture = "";
        
        [System.Serializable]
        public class GesturePattern
        {
            public string name;
            public List<FingerPosition> requiredFingerPositions = new List<FingerPosition>();
            public Vector3 wristRotation;
            public float confidenceThreshold = 0.8f;
            public bool isMedicalGesture = false;
        }
        
        [System.Serializable]
        public class FingerPosition
        {
            public OVRHand.HandFinger finger;
            public FingerState state;
            public float flexion; // 0 = straight, 1 = fully bent
            public Vector3 direction; // Optional direction constraint
            
            public enum FingerState
            {
                Extended,
                Bent,
                Pinching,
                Touching,
                Pointing
            }
        }
        
        private void Start()
        {
            InitializeGesturePatterns();
        }
        
        private void InitializeGesturePatterns()
        {
            gesturePatterns = new Dictionary<string, GesturePattern>();
            
            // Basic gestures
            CreateBasicGestures();
            
            // Medical-specific gestures
            if (detectMedicalGestures)
            {
                CreateMedicalGestures();
            }
        }
        
        private void CreateBasicGestures()
        {
            // Point gesture
            var pointGesture = new GesturePattern
            {
                name = "point",
                confidenceThreshold = 0.7f
            };
            pointGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.1f
            });
            pointGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Middle,
                state = FingerPosition.FingerState.Bent,
                flexion = 0.8f
            });
            gesturePatterns["point"] = pointGesture;
            
            // Thumbs up
            var thumbsUpGesture = new GesturePattern
            {
                name = "thumbsup",
                confidenceThreshold = 0.8f
            };
            thumbsUpGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Thumb,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.1f
            });
            thumbsUpGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Bent,
                flexion = 0.9f
            });
            gesturePatterns["thumbsup"] = thumbsUpGesture;
            
            // Peace sign
            var peaceGesture = new GesturePattern
            {
                name = "peace",
                confidenceThreshold = 0.8f
            };
            peaceGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.1f
            });
            peaceGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Middle,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.1f
            });
            gesturePatterns["peace"] = peaceGesture;
            
            // Fist
            var fistGesture = new GesturePattern
            {
                name = "fist",
                confidenceThreshold = 0.9f
            };
            for (int i = 0; i < 5; i++)
            {
                fistGesture.requiredFingerPositions.Add(new FingerPosition
                {
                    finger = (OVRHand.HandFinger)i,
                    state = FingerPosition.FingerState.Bent,
                    flexion = 0.8f
                });
            }
            gesturePatterns["fist"] = fistGesture;
            
            // Open palm
            var openPalmGesture = new GesturePattern
            {
                name = "openpalm",
                confidenceThreshold = 0.8f
            };
            for (int i = 0; i < 5; i++)
            {
                openPalmGesture.requiredFingerPositions.Add(new FingerPosition
                {
                    finger = (OVRHand.HandFinger)i,
                    state = FingerPosition.FingerState.Extended,
                    flexion = 0.2f
                });
            }
            gesturePatterns["openpalm"] = openPalmGesture;
        }
        
        private void CreateMedicalGestures()
        {
            // Injection grip (syringe hold)
            var injectionGrip = new GesturePattern
            {
                name = "injection_grip",
                confidenceThreshold = medicalGestureAccuracy,
                isMedicalGesture = true
            };
            injectionGrip.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Thumb,
                state = FingerPosition.FingerState.Touching,
                flexion = 0.6f
            });
            injectionGrip.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Bent,
                flexion = 0.7f
            });
            gesturePatterns["injection_grip"] = injectionGrip;
            
            // Stethoscope position
            var stethoscopeGrip = new GesturePattern
            {
                name = "stethoscope_grip",
                confidenceThreshold = medicalGestureAccuracy,
                isMedicalGesture = true
            };
            stethoscopeGrip.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Thumb,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.3f
            });
            stethoscopeGrip.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.2f
            });
            gesturePatterns["stethoscope_grip"] = stethoscopeGrip;
            
            // Surgical precision grip
            var surgicalGrip = new GesturePattern
            {
                name = "surgical_grip",
                confidenceThreshold = medicalGestureAccuracy,
                isMedicalGesture = true
            };
            surgicalGrip.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Thumb,
                state = FingerPosition.FingerState.Pinching,
                flexion = 0.5f
            });
            surgicalGrip.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Pinching,
                flexion = 0.5f
            });
            gesturePatterns["surgical_grip"] = surgicalGrip;
            
            // Palpation gesture
            var palpationGesture = new GesturePattern
            {
                name = "palpation",
                confidenceThreshold = medicalGestureAccuracy,
                isMedicalGesture = true
            };
            palpationGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.1f
            });
            palpationGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Middle,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.1f
            });
            gesturePatterns["palpation"] = palpationGesture;
            
            // CPR hand position
            var cprGesture = new GesturePattern
            {
                name = "cpr_position",
                confidenceThreshold = medicalGestureAccuracy,
                isMedicalGesture = true
            };
            cprGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Index,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.2f
            });
            cprGesture.requiredFingerPositions.Add(new FingerPosition
            {
                finger = OVRHand.HandFinger.Middle,
                state = FingerPosition.FingerState.Extended,
                flexion = 0.2f
            });
            gesturePatterns["cpr_position"] = cprGesture;
        }
        
        public string RecognizeGesture(OVRSkeleton handSkeleton)
        {
            if (handSkeleton == null || !handSkeleton.IsInitialized)
                return "";
                
            string bestGesture = "";
            float bestConfidence = 0f;
            
            foreach (var gesturePattern in gesturePatterns.Values)
            {
                float confidence = CalculateGestureConfidence(handSkeleton, gesturePattern);
                
                if (confidence > gesturePattern.confidenceThreshold && confidence > bestConfidence)
                {
                    bestConfidence = confidence;
                    bestGesture = gesturePattern.name;
                }
            }
            
            // Apply gesture hold timing
            if (!string.IsNullOrEmpty(bestGesture))
            {
                return ProcessGestureWithTiming(bestGesture);
            }
            
            return "";
        }
        
        private float CalculateGestureConfidence(OVRSkeleton handSkeleton, GesturePattern pattern)
        {
            if (pattern.requiredFingerPositions.Count == 0)
                return 0f;
                
            float totalConfidence = 0f;
            int validFingers = 0;
            
            foreach (var fingerPos in pattern.requiredFingerPositions)
            {
                float fingerConfidence = CalculateFingerConfidence(handSkeleton, fingerPos);
                totalConfidence += fingerConfidence;
                validFingers++;
            }
            
            float averageConfidence = validFingers > 0 ? totalConfidence / validFingers : 0f;
            
            // Apply medical gesture bonus if applicable
            if (pattern.isMedicalGesture && detectMedicalGestures)
            {
                averageConfidence *= 1.1f; // Slight boost for medical gestures
            }
            
            return Mathf.Clamp01(averageConfidence);
        }
        
        private float CalculateFingerConfidence(OVRSkeleton handSkeleton, FingerPosition fingerPos)
        {
            OVRBone fingerBone = GetFingerBone(handSkeleton, fingerPos.finger);
            if (fingerBone == null)
                return 0f;
                
            float currentFlexion = CalculateFingerFlexion(handSkeleton, fingerPos.finger);
            float flexionDifference = Mathf.Abs(currentFlexion - fingerPos.flexion);
            
            float flexionConfidence = 1f - (flexionDifference / 1f); // Normalize to 0-1
            
            // Check finger state
            float stateConfidence = CheckFingerState(handSkeleton, fingerPos);
            
            return (flexionConfidence + stateConfidence) * 0.5f;
        }
        
        private float CalculateFingerFlexion(OVRSkeleton handSkeleton, OVRHand.HandFinger finger)
        {
            OVRBone fingerTip = GetFingerBone(handSkeleton, finger);
            OVRBone fingerBase = GetFingerBaseBone(handSkeleton, finger);
            
            if (fingerTip == null || fingerBase == null)
                return 0f;
                
            // Calculate angle between finger segments
            Vector3 tipPosition = fingerTip.Transform.position;
            Vector3 basePosition = fingerBase.Transform.position;
            Vector3 fingerDirection = (tipPosition - basePosition).normalized;
            
            // Compare with extended finger direction (approximation)
            Vector3 extendedDirection = handSkeleton.transform.forward;
            float angle = Vector3.Angle(fingerDirection, extendedDirection);
            
            // Convert angle to flexion (0 = straight, 1 = fully bent)
            return Mathf.Clamp01(angle / 90f);
        }
        
        private float CheckFingerState(OVRSkeleton handSkeleton, FingerPosition fingerPos)
        {
            switch (fingerPos.state)
            {
                case FingerPosition.FingerState.Extended:
                    return CalculateFingerFlexion(handSkeleton, fingerPos.finger) < 0.3f ? 1f : 0f;
                    
                case FingerPosition.FingerState.Bent:
                    return CalculateFingerFlexion(handSkeleton, fingerPos.finger) > 0.7f ? 1f : 0f;
                    
                case FingerPosition.FingerState.Pinching:
                    return CheckPinchingState(handSkeleton, fingerPos.finger);
                    
                case FingerPosition.FingerState.Touching:
                    return CheckTouchingState(handSkeleton, fingerPos.finger);
                    
                case FingerPosition.FingerState.Pointing:
                    return CheckPointingState(handSkeleton, fingerPos.finger);
                    
                default:
                    return 1f;
            }
        }
        
        private float CheckPinchingState(OVRSkeleton handSkeleton, OVRHand.HandFinger finger)
        {
            // Check if finger is close to thumb
            OVRBone fingerTip = GetFingerBone(handSkeleton, finger);
            OVRBone thumbTip = GetFingerBone(handSkeleton, OVRHand.HandFinger.Thumb);
            
            if (fingerTip == null || thumbTip == null)
                return 0f;
                
            float distance = Vector3.Distance(fingerTip.Transform.position, thumbTip.Transform.position);
            return distance < 0.03f ? 1f : 0f; // 3cm threshold for pinching
        }
        
        private float CheckTouchingState(OVRSkeleton handSkeleton, OVRHand.HandFinger finger)
        {
            // Similar to pinching but with slightly larger threshold
            return CheckPinchingState(handSkeleton, finger) > 0f ? 1f : 0f;
        }
        
        private float CheckPointingState(OVRSkeleton handSkeleton, OVRHand.HandFinger finger)
        {
            // Check if finger is extended and others are bent
            float fingerFlexion = CalculateFingerFlexion(handSkeleton, finger);
            if (fingerFlexion > 0.3f)
                return 0f;
                
            // Check other fingers are bent
            int bentFingers = 0;
            for (int i = 0; i < 5; i++)
            {
                if ((OVRHand.HandFinger)i != finger)
                {
                    float otherFlexion = CalculateFingerFlexion(handSkeleton, (OVRHand.HandFinger)i);
                    if (otherFlexion > 0.6f)
                        bentFingers++;
                }
            }
            
            return bentFingers >= 3 ? 1f : 0f;
        }
        
        private OVRBone GetFingerBone(OVRSkeleton handSkeleton, OVRHand.HandFinger finger)
        {
            if (!handSkeleton.IsInitialized)
                return null;
                
            // Map finger to bone ID
            OVRSkeleton.BoneId boneId = GetFingerTipBoneId(finger);
            return handSkeleton.Bones.FirstOrDefault(bone => bone.Id == boneId);
        }
        
        private OVRBone GetFingerBaseBone(OVRSkeleton handSkeleton, OVRHand.HandFinger finger)
        {
            if (!handSkeleton.IsInitialized)
                return null;
                
            OVRSkeleton.BoneId boneId = GetFingerBaseBoneId(finger);
            return handSkeleton.Bones.FirstOrDefault(bone => bone.Id == boneId);
        }
        
        private OVRSkeleton.BoneId GetFingerTipBoneId(OVRHand.HandFinger finger)
        {
            switch (finger)
            {
                case OVRHand.HandFinger.Thumb:
                    return OVRSkeleton.BoneId.Hand_ThumbTip;
                case OVRHand.HandFinger.Index:
                    return OVRSkeleton.BoneId.Hand_IndexTip;
                case OVRHand.HandFinger.Middle:
                    return OVRSkeleton.BoneId.Hand_MiddleTip;
                case OVRHand.HandFinger.Ring:
                    return OVRSkeleton.BoneId.Hand_RingTip;
                case OVRHand.HandFinger.Pinky:
                    return OVRSkeleton.BoneId.Hand_PinkyTip;
                default:
                    return OVRSkeleton.BoneId.Hand_Start;
            }
        }
        
        private OVRSkeleton.BoneId GetFingerBaseBoneId(OVRHand.HandFinger finger)
        {
            switch (finger)
            {
                case OVRHand.HandFinger.Thumb:
                    return OVRSkeleton.BoneId.Hand_Thumb0;
                case OVRHand.HandFinger.Index:
                    return OVRSkeleton.BoneId.Hand_Index1;
                case OVRHand.HandFinger.Middle:
                    return OVRSkeleton.BoneId.Hand_Middle1;
                case OVRHand.HandFinger.Ring:
                    return OVRSkeleton.BoneId.Hand_Ring1;
                case OVRHand.HandFinger.Pinky:
                    return OVRSkeleton.BoneId.Hand_Pinky1;
                default:
                    return OVRSkeleton.BoneId.Hand_Start;
            }
        }
        
        private string ProcessGestureWithTiming(string gestureName)
        {
            if (gestureName == lastRecognizedGesture)
            {
                if (gestureTimers.ContainsKey(gestureName))
                {
                    gestureTimers[gestureName] += Time.deltaTime;
                    
                    if (gestureTimers[gestureName] >= gestureHoldTime)
                    {
                        return gestureName; // Gesture confirmed
                    }
                }
                else
                {
                    gestureTimers[gestureName] = 0f;
                }
            }
            else
            {
                // New gesture detected, reset timers
                gestureTimers.Clear();
                gestureTimers[gestureName] = 0f;
                lastRecognizedGesture = gestureName;
            }
            
            return ""; // Gesture not held long enough
        }
        
        public void AddCustomGesture(GesturePattern customGesture)
        {
            if (!gesturePatterns.ContainsKey(customGesture.name))
            {
                gesturePatterns[customGesture.name] = customGesture;
                Debug.Log($"Added custom gesture: {customGesture.name}");
            }
        }
        
        public bool IsGestureActive(string gestureName)
        {
            return gestureTimers.ContainsKey(gestureName) && gestureTimers[gestureName] >= gestureHoldTime;
        }
        
        public float GetGestureConfidence(string gestureName)
        {
            if (gesturePatterns.ContainsKey(gestureName))
            {
                // Would need current hand skeleton to calculate - this is simplified
                return IsGestureActive(gestureName) ? 1f : 0f;
            }
            return 0f;
        }
        
        public List<string> GetAvailableGestures()
        {
            return new List<string>(gesturePatterns.Keys);
        }
        
        public List<string> GetMedicalGestures()
        {
            return gesturePatterns.Where(kvp => kvp.Value.isMedicalGesture)
                                  .Select(kvp => kvp.Key)
                                  .ToList();
        }
    }
} 