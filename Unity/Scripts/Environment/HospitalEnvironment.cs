using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MedTrainAI.Medical;

namespace MedTrainAI.Environment
{
    public class HospitalEnvironment : MonoBehaviour
    {
        [Header("Environment Rooms")]
        public List<HospitalRoom> availableRooms = new List<HospitalRoom>();
        public HospitalRoom currentRoom;
        
        [Header("Equipment Management")]
        public List<GameObject> equipmentPrefabs = new List<GameObject>();
        public Transform equipmentParent;
        public Dictionary<string, GameObject> spawnedEquipment = new Dictionary<string, GameObject>();
        
        [Header("Lighting and Atmosphere")]
        public Light mainLight;
        public Light[] additionalLights;
        public AudioSource ambientAudio;
        public AudioClip[] hospitalSounds;
        
        [Header("Spatial Tracking")]
        public Transform playerSpawnPoint;
        public Transform patientBedPosition;
        public Transform[] equipmentSpawnPoints;
        
        private MedicalScenario currentScenario;
        
        [System.Serializable]
        public class HospitalRoom
        {
            public string roomName;
            public RoomType roomType;
            public GameObject roomPrefab;
            public List<string> requiredEquipment = new List<string>();
            public Vector3 roomDimensions;
            public bool isOccupied = false;
            
            public enum RoomType
            {
                EmergencyRoom,
                OperatingRoom,
                ICU,
                PatientRoom,
                ExaminationRoom,
                ConsultationRoom,
                Trauma_Bay
            }
        }
        
        private void Start()
        {
            InitializeEnvironment();
        }
        
        private void InitializeEnvironment()
        {
            // Set default room if none selected
            if (currentRoom == null && availableRooms.Count > 0)
            {
                SetupRoom(availableRooms[0]);
            }
            
            // Initialize spatial tracking
            SetupSpatialTracking();
            
            // Start ambient sounds
            PlayAmbientSounds();
        }
        
        public void SetupForScenario(MedicalScenario scenario)
        {
            currentScenario = scenario;
            
            // Determine appropriate room type
            HospitalRoom.RoomType requiredRoomType = GetRoomTypeForScenario(scenario.scenarioType);
            
            // Find and setup the appropriate room
            HospitalRoom targetRoom = FindRoomByType(requiredRoomType);
            if (targetRoom != null)
            {
                SetupRoom(targetRoom);
            }
            
            // Setup required equipment
            SetupScenarioEquipment(scenario);
            
            // Adjust lighting for scenario
            AdjustLightingForScenario(scenario);
            
            Debug.Log($"Environment setup for scenario: {scenario.title} in {targetRoom?.roomName ?? "default room"}");
        }
        
        private HospitalRoom.RoomType GetRoomTypeForScenario(MedicalScenario.ScenarioType scenarioType)
        {
            switch (scenarioType)
            {
                case MedicalScenario.ScenarioType.Emergency:
                    return HospitalRoom.RoomType.EmergencyRoom;
                case MedicalScenario.ScenarioType.Surgery:
                    return HospitalRoom.RoomType.OperatingRoom;
                case MedicalScenario.ScenarioType.Consultation:
                    return HospitalRoom.RoomType.ConsultationRoom;
                case MedicalScenario.ScenarioType.Examination:
                    return HospitalRoom.RoomType.ExaminationRoom;
                case MedicalScenario.ScenarioType.PatientCare:
                    return HospitalRoom.RoomType.PatientRoom;
                default:
                    return HospitalRoom.RoomType.ExaminationRoom;
            }
        }
        
        private HospitalRoom FindRoomByType(HospitalRoom.RoomType roomType)
        {
            foreach (var room in availableRooms)
            {
                if (room.roomType == roomType && !room.isOccupied)
                {
                    return room;
                }
            }
            
            // Return any available room if specific type not found
            foreach (var room in availableRooms)
            {
                if (!room.isOccupied)
                {
                    return room;
                }
            }
            
            return availableRooms.Count > 0 ? availableRooms[0] : null;
        }
        
        public void SetupRoom(HospitalRoom room)
        {
            if (currentRoom != null)
            {
                // Clean up current room
                CleanupCurrentRoom();
            }
            
            currentRoom = room;
            room.isOccupied = true;
            
            // Instantiate room prefab
            if (room.roomPrefab != null)
            {
                GameObject roomInstance = Instantiate(room.roomPrefab, transform);
                
                // Find important positions within the room
                FindRoomPositions(roomInstance);
            }
            
            // Setup basic room equipment
            SetupRoomEquipment(room);
            
            Debug.Log($"Setup room: {room.roomName}");
        }
        
        private void FindRoomPositions(GameObject roomInstance)
        {
            // Find standard positions in the room
            Transform bedTransform = roomInstance.transform.Find("PatientBed");
            if (bedTransform != null)
                patientBedPosition = bedTransform;
                
            Transform spawnTransform = roomInstance.transform.Find("PlayerSpawn");
            if (spawnTransform != null)
                playerSpawnPoint = spawnTransform;
                
            // Find equipment spawn points
            List<Transform> equipmentPoints = new List<Transform>();
            for (int i = 0; i < 10; i++) // Look for up to 10 equipment points
            {
                Transform point = roomInstance.transform.Find($"EquipmentPoint_{i}");
                if (point != null)
                    equipmentPoints.Add(point);
            }
            equipmentSpawnPoints = equipmentPoints.ToArray();
        }
        
        private void SetupRoomEquipment(HospitalRoom room)
        {
            foreach (string equipmentName in room.requiredEquipment)
            {
                SpawnEquipment(equipmentName);
            }
        }
        
        private void SetupScenarioEquipment(MedicalScenario scenario)
        {
            foreach (var equipment in scenario.requiredEquipment)
            {
                if (equipment.isEssential)
                {
                    SpawnEquipment(equipment.equipmentName);
                }
            }
        }
        
        public void SpawnEquipment(string equipmentName)
        {
            if (spawnedEquipment.ContainsKey(equipmentName))
            {
                // Equipment already spawned
                return;
            }
            
            GameObject equipmentPrefab = FindEquipmentPrefab(equipmentName);
            if (equipmentPrefab != null)
            {
                Vector3 spawnPosition = GetNextEquipmentSpawnPosition();
                GameObject spawnedItem = Instantiate(equipmentPrefab, spawnPosition, Quaternion.identity, equipmentParent);
                
                spawnedEquipment[equipmentName] = spawnedItem;
                
                Debug.Log($"Spawned equipment: {equipmentName}");
            }
            else
            {
                Debug.LogWarning($"Equipment prefab not found: {equipmentName}");
            }
        }
        
        private GameObject FindEquipmentPrefab(string equipmentName)
        {
            foreach (var prefab in equipmentPrefabs)
            {
                if (prefab.name.ToLower().Contains(equipmentName.ToLower()))
                {
                    return prefab;
                }
            }
            return null;
        }
        
        private Vector3 GetNextEquipmentSpawnPosition()
        {
            if (equipmentSpawnPoints != null && equipmentSpawnPoints.Length > 0)
            {
                int index = spawnedEquipment.Count % equipmentSpawnPoints.Length;
                return equipmentSpawnPoints[index].position;
            }
            
            // Default position near the room center
            return transform.position + Vector3.right * spawnedEquipment.Count * 0.5f;
        }
        
        private void AdjustLightingForScenario(MedicalScenario scenario)
        {
            switch (scenario.scenarioType)
            {
                case MedicalScenario.ScenarioType.Surgery:
                    // Bright, focused lighting for surgery
                    if (mainLight != null)
                    {
                        mainLight.intensity = 2.5f;
                        mainLight.color = Color.white;
                    }
                    break;
                    
                case MedicalScenario.ScenarioType.Emergency:
                    // Intense, clinical lighting
                    if (mainLight != null)
                    {
                        mainLight.intensity = 2.0f;
                        mainLight.color = new Color(1f, 0.95f, 0.9f);
                    }
                    break;
                    
                case MedicalScenario.ScenarioType.PatientCare:
                    // Softer, warmer lighting
                    if (mainLight != null)
                    {
                        mainLight.intensity = 1.5f;
                        mainLight.color = new Color(1f, 0.9f, 0.8f);
                    }
                    break;
                    
                default:
                    // Standard clinical lighting
                    if (mainLight != null)
                    {
                        mainLight.intensity = 1.8f;
                        mainLight.color = new Color(1f, 0.98f, 0.95f);
                    }
                    break;
            }
        }
        
        private void SetupSpatialTracking()
        {
            // Initialize spatial anchors for hand tracking and room scale VR
            if (playerSpawnPoint != null)
            {
                // Position the player at the spawn point
                Camera.main.transform.position = playerSpawnPoint.position;
                Camera.main.transform.rotation = playerSpawnPoint.rotation;
            }
            
            // Setup room boundaries for Quest 3S
            SetupRoomBoundaries();
        }
        
        private void SetupRoomBoundaries()
        {
            if (currentRoom != null)
            {
                // Configure guardian boundaries based on room dimensions
                Vector3 roomSize = currentRoom.roomDimensions;
                
                // Create invisible boundaries to keep player in safe area
                CreateBoundaryWalls(roomSize);
            }
        }
        
        private void CreateBoundaryWalls(Vector3 roomSize)
        {
            // Create invisible colliders to prevent player from walking through walls
            GameObject boundaryParent = new GameObject("RoomBoundaries");
            boundaryParent.transform.SetParent(transform);
            
            // Front wall
            CreateBoundaryWall(boundaryParent.transform, new Vector3(0, 0, roomSize.z/2), new Vector3(roomSize.x, 3, 0.1f));
            // Back wall
            CreateBoundaryWall(boundaryParent.transform, new Vector3(0, 0, -roomSize.z/2), new Vector3(roomSize.x, 3, 0.1f));
            // Left wall
            CreateBoundaryWall(boundaryParent.transform, new Vector3(-roomSize.x/2, 0, 0), new Vector3(0.1f, 3, roomSize.z));
            // Right wall
            CreateBoundaryWall(boundaryParent.transform, new Vector3(roomSize.x/2, 0, 0), new Vector3(0.1f, 3, roomSize.z));
        }
        
        private void CreateBoundaryWall(Transform parent, Vector3 position, Vector3 size)
        {
            GameObject wall = new GameObject("BoundaryWall");
            wall.transform.SetParent(parent);
            wall.transform.localPosition = position;
            
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = false;
            
            wall.layer = LayerMask.NameToLayer("Environment");
        }
        
        private void PlayAmbientSounds()
        {
            if (ambientAudio != null && hospitalSounds.Length > 0)
            {
                AudioClip ambientClip = hospitalSounds[Random.Range(0, hospitalSounds.Length)];
                ambientAudio.clip = ambientClip;
                ambientAudio.loop = true;
                ambientAudio.volume = 0.3f;
                ambientAudio.Play();
            }
        }
        
        public void CleanupCurrentRoom()
        {
            if (currentRoom != null)
            {
                currentRoom.isOccupied = false;
                
                // Destroy all spawned equipment
                foreach (var equipment in spawnedEquipment.Values)
                {
                    if (equipment != null)
                        Destroy(equipment);
                }
                spawnedEquipment.Clear();
                
                // Destroy room instance
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = transform.GetChild(i);
                    if (child.name.Contains("Room") || child.name.Contains("Boundaries"))
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        
        public void RemoveEquipment(string equipmentName)
        {
            if (spawnedEquipment.ContainsKey(equipmentName))
            {
                GameObject equipment = spawnedEquipment[equipmentName];
                if (equipment != null)
                    Destroy(equipment);
                    
                spawnedEquipment.Remove(equipmentName);
                
                Debug.Log($"Removed equipment: {equipmentName}");
            }
        }
        
        public bool IsEquipmentAvailable(string equipmentName)
        {
            return spawnedEquipment.ContainsKey(equipmentName) && spawnedEquipment[equipmentName] != null;
        }
        
        public Transform GetPatientPosition()
        {
            return patientBedPosition;
        }
        
        public Transform GetPlayerSpawnPosition()
        {
            return playerSpawnPoint;
        }
        
        public HospitalRoom GetCurrentRoom()
        {
            return currentRoom;
        }
        
        public List<string> GetAvailableEquipment()
        {
            return new List<string>(spawnedEquipment.Keys);
        }
        
        private void OnDestroy()
        {
            CleanupCurrentRoom();
        }
    }
} 