using System;
using System.Collections.Generic;
using UnityEngine;
using TayDuKy.Network;

namespace TayDuKy.Managers
{
    public class MountAndPetManager : MonoBehaviour
    {
        public static MountAndPetManager Instance { get; private set; }

        [System.Serializable]
        public class MountData
        {
            public int id;
            public string mountName;
            public int speedBonus;
            public bool isEquipped;
        }

        [System.Serializable]
        public class PetData
        {
            public int id;
            public string petName;
            public string grade; // "Trân Thú", "Tán Tiên", "Kim Tiên"
            public int level;
            public bool isSummoned;
        }

        public List<MountData> ownedMountsList = new List<MountData>();
        public List<PetData> ownedPetsList = new List<PetData>();

        [Header("Pet Sprite Sheets (Placeholders)")]
        [SerializeField] private Sprite[] thoNgocSprites = new Sprite[16];
        [SerializeField] private Sprite[] tieuToanPhongSprites = new Sprite[16];

        private MountData activeMount = null;
        private PetData activePet = null;
        private GameObject spawnedPetInstance = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadInitialAssets();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadInitialAssets()
        {
            // Inject some mock starter mounts and pets for the player
            ownedMountsList.Add(new MountData { id = 1, mountName = "U Minh Bạch Hổ", speedBonus = 30, isEquipped = false });
            ownedMountsList.Add(new MountData { id = 2, mountName = "Lục Bảo Phi Kiếm", speedBonus = 35, isEquipped = false });

            ownedPetsList.Add(new PetData { id = 101, petName = "Thỏ Ngọc", grade = "Trân Thú", level = 5, isSummoned = false });
            ownedPetsList.Add(new PetData { id = 102, petName = "Tiểu Toàn Phong", grade = "Tán Tiên", level = 12, isSummoned = false });
        }

        // --- Mount Actions ---

        public void EquipMount(int mountId)
        {
            MountData mount = ownedMountsList.Find(x => x.id == mountId);
            if (mount == null) return;

            // Unequip current mount
            if (activeMount != null)
            {
                activeMount.isEquipped = false;
            }

            // Equip new mount
            mount.isEquipped = true;
            activeMount = mount;
            Debug.Log($"Mount: Rented/Equipped mount -> {mount.mountName}. Speed increased by {mount.speedBonus}%");

            // Notify server (Action ID 1010)
            SendMountEquipPacket(mountId, true);
        }

        public void UnequipMount()
        {
            if (activeMount == null) return;

            Debug.Log($"Mount: Unequipped mount -> {activeMount.mountName}");
            SendMountEquipPacket(activeMount.id, false);

            activeMount.isEquipped = false;
            activeMount = null;
        }

        public float GetCurrentSpeedMultiplier()
        {
            if (activeMount != null)
            {
                return 1.0f + (activeMount.speedBonus / 100.0f);
            }
            return 1.0f; // Default normal speed
        }

        // --- Pet Actions ---

        public void SummonPet(int petId)
        {
            PetData pet = ownedPetsList.Find(x => x.id == petId);
            if (pet == null) return;

            // 1. Unsummon current pet if any
            UnsummonPet();

            // 2. Summon new pet
            pet.isSummoned = true;
            activePet = pet;
            Debug.Log($"Pet: Summoned pet -> {pet.petName} (Level {pet.level}) to battle side-by-side!");

            // 3. Create the Pet GameObject in the scene
            spawnedPetInstance = new GameObject("SummonedPet_" + pet.petName);
            var sr = spawnedPetInstance.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5; // Render on top of the map
            
            var pc = spawnedPetInstance.AddComponent<Controllers.PetController>();
            
            // Assign sprites based on pet ID / placeholder
            if (petId == 101) // Thỏ Ngọc
            {
                pc.SetSprites(thoNgocSprites);
            }
            else if (petId == 102) // Tiểu Toàn Phong
            {
                pc.SetSprites(tieuToanPhongSprites);
            }

            // 4. Notify server (Action ID 1011)
            SendPetSummonPacket(petId, true);
        }

        public void UnsummonPet()
        {
            if (activePet != null)
            {
                activePet.isSummoned = false;
                SendPetSummonPacket(activePet.id, false);
                activePet = null;
            }

            if (spawnedPetInstance != null)
            {
                Destroy(spawnedPetInstance);
                spawnedPetInstance = null;
                Debug.Log("Pet: Destroyed active pet GameObject instance.");
            }
        }

        // --- Networking Packets ---

        private void SendMountEquipPacket(int mountId, bool equipState)
        {
            if (NetworkClient.Instance == null) return;

            // Action ID 1010 for mount action
            string mountPayload = $"{{\"action_id\": 1010, \"character_id\": 1024, \"mount_id\": {mountId}, \"is_equipped\": {equipState.ToString().ToLower()}}}";
            NetworkClient.Instance.SendPacket(mountPayload);
        }

        private void SendPetSummonPacket(int petId, bool summonState)
        {
            if (NetworkClient.Instance == null) return;

            // Action ID 1011 for pet action
            string petPayload = $"{{\"action_id\": 1011, \"character_id\": 1024, \"pet_id\": {petId}, \"is_summoned\": {summonState.ToString().ToLower()}}}";
            NetworkClient.Instance.SendPacket(petPayload);
        }
    }
}
