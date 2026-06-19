using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TayDuKy.Network;

namespace TayDuKy.Managers
{
    /// <summary>
    /// PetSystemManager — Quản lý toàn bộ hệ thống Tiên Sủng nâng cao:
    /// - Ấp trứng (Egg Hatching) theo thời gian thực
    /// - Nâng cấp Pet (Level Up, Tẩy tủy)
    /// - Học kỹ năng cho Pet (tối đa 6 ô kỹ năng)
    /// - Tiến hóa cấp bậc: Trân Thú → Tán Tiên → Kim Tiên
    /// </summary>
    public class PetSystemManager : MonoBehaviour
    {
        public static PetSystemManager Instance { get; private set; }

        // ============================================================
        // Data Structures
        // ============================================================

        [System.Serializable]
        public class PetEgg
        {
            public int eggId;
            public string eggName;
            public string potentialPetType; // Tiên sủng có thể nở ra
            public float hatchDurationSeconds; // Thời gian ấp (giây)
            public float hatchStartTime;       // Thời điểm bắt đầu ấp (Time.time)
            public bool isHatching;
        }

        [System.Serializable]
        public class PetSkill
        {
            public int skillId;
            public string skillName;
            public string skillType; // "attack", "heal", "buff", "debuff"
            public int damage;
            public int mpCost;
        }

        [System.Serializable]
        public class PetProfile
        {
            public int id;
            public string petName;
            public string petType;
            public string grade; // "Trân Thú", "Tán Tiên", "Kim Tiên"
            public int level;
            public int exp;
            public int expToNextLevel;
            public int str;
            public int intStat;
            public int vit;
            public int agi;
            public int hpMax;
            public int hpCurrent;
            public bool isSummoned;
            public List<PetSkill> skills = new List<PetSkill>(); // Tối đa 6 ô
        }

        // ============================================================
        // State
        // ============================================================

        public List<PetEgg> eggInventory = new List<PetEgg>();
        public List<PetProfile> petRoster = new List<PetProfile>();

        private const int MAX_SKILL_SLOTS = 6;
        private const int MAX_GRADE_LEVEL = 3; // Trân Thú=1, Tán Tiên=2, Kim Tiên=3

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadInitialPets();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadInitialPets()
        {
            // Thêm một số pet mẫu để test
            petRoster.Add(new PetProfile
            {
                id = 201, petName = "Thỏ Ngọc", petType = "tho_ngoc",
                grade = "Trân Thú", level = 1, exp = 0, expToNextLevel = 100,
                str = 8, intStat = 12, vit = 10, agi = 15,
                hpMax = 80, hpCurrent = 80, isSummoned = false
            });

            // Thêm trứng mẫu
            eggInventory.Add(new PetEgg
            {
                eggId = 1001, eggName = "Trứng Kỳ Lân Con",
                potentialPetType = "ky_lan_con",
                hatchDurationSeconds = 300f, // 5 phút
                isHatching = false
            });
        }

        private void Update()
        {
            // Kiểm tra các trứng đang ấp để kích hoạt nở
            foreach (var egg in eggInventory)
            {
                if (egg.isHatching)
                {
                    float elapsed = Time.time - egg.hatchStartTime;
                    if (elapsed >= egg.hatchDurationSeconds)
                    {
                        HatchEgg(egg);
                        break; // Modify list safely — next frame tiếp tục
                    }
                }
            }
        }

        // ============================================================
        // Egg Hatching System
        // ============================================================

        /// <summary>
        /// Bắt đầu ấp trứng. Sẽ nở sau hatchDurationSeconds giây.
        /// </summary>
        public void StartHatching(int eggId)
        {
            PetEgg egg = eggInventory.Find(e => e.eggId == eggId);
            if (egg == null || egg.isHatching)
            {
                Debug.LogWarning($"PetSystem: Egg {eggId} not found or already hatching.");
                return;
            }

            egg.isHatching = true;
            egg.hatchStartTime = Time.time;
            float minutes = egg.hatchDurationSeconds / 60f;
            Debug.Log($"PetSystem: Started hatching [{egg.eggName}]. Will hatch in {minutes:F1} minutes.");
        }

        /// <summary>
        /// Kiểm tra tiến độ ấp trứng (phần trăm hoàn thành).
        /// </summary>
        public float GetHatchProgress(int eggId)
        {
            PetEgg egg = eggInventory.Find(e => e.eggId == eggId);
            if (egg == null || !egg.isHatching) return 0f;

            float elapsed = Time.time - egg.hatchStartTime;
            return Mathf.Clamp01(elapsed / egg.hatchDurationSeconds);
        }

        private void HatchEgg(PetEgg egg)
        {
            egg.isHatching = false;
            eggInventory.Remove(egg);

            // Tạo pet mới với chỉ số ngẫu nhiên (mô phỏng hệ thống gốc)
            PetProfile newPet = CreateRandomPet(egg.potentialPetType, eggInventory.Count + petRoster.Count + 300);
            petRoster.Add(newPet);

            Debug.Log($"PetSystem: Egg hatched! New pet born: [{newPet.petName}] (Grade: {newPet.grade})");

            // Thông báo lên UI
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.AppendChatMessage("Hệ thống",
                    $"✨ Trứng đã nở! Chào mừng Tiên Sủng [{newPet.petName}] cấp bậc [{newPet.grade}] ra đời!");
            }
        }

        private PetProfile CreateRandomPet(string petType, int newId)
        {
            // Sinh chỉ số ngẫu nhiên trong khoảng cơ bản
            return new PetProfile
            {
                id = newId,
                petName = GetPetDisplayName(petType),
                petType = petType,
                grade = "Trân Thú",
                level = 1, exp = 0, expToNextLevel = 100,
                str = UnityEngine.Random.Range(8, 15),
                intStat = UnityEngine.Random.Range(8, 15),
                vit = UnityEngine.Random.Range(8, 15),
                agi = UnityEngine.Random.Range(8, 15),
                hpMax = UnityEngine.Random.Range(70, 100),
                hpCurrent = 80, isSummoned = false,
                skills = new List<PetSkill>()
            };
        }

        // ============================================================
        // Pet Level Up & Evolution
        // ============================================================

        /// <summary>
        /// Thêm EXP vào pet và kiểm tra lên cấp.
        /// </summary>
        public void AddPetExp(int petId, int expAmount)
        {
            PetProfile pet = petRoster.Find(p => p.id == petId);
            if (pet == null) return;

            pet.exp += expAmount;
            bool leveledUp = false;

            while (pet.exp >= pet.expToNextLevel)
            {
                pet.exp -= pet.expToNextLevel;
                pet.level++;
                pet.expToNextLevel = pet.level * 80; // Đường cong EXP pet
                pet.str += UnityEngine.Random.Range(1, 3);
                pet.intStat += UnityEngine.Random.Range(1, 3);
                pet.vit += UnityEngine.Random.Range(1, 3);
                pet.agi += UnityEngine.Random.Range(1, 3);
                pet.hpMax += 10;
                pet.hpCurrent = pet.hpMax;
                leveledUp = true;
                Debug.Log($"PetSystem: [{pet.petName}] leveled up to Lv.{pet.level}!");
            }

            if (leveledUp && UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.AppendChatMessage("Hệ thống",
                    $"🌟 Tiên Sủng [{pet.petName}] thăng lên cấp {pet.level}!");
            }
        }

        /// <summary>
        /// Tiến hóa Pet lên cấp bậc cao hơn:
        /// Trân Thú → Tán Tiên (yêu cầu Lv.30) → Kim Tiên (yêu cầu Lv.60)
        /// </summary>
        public bool EvolvePet(int petId)
        {
            PetProfile pet = petRoster.Find(p => p.id == petId);
            if (pet == null) return false;

            int requiredLevel = pet.grade switch
            {
                "Trân Thú" => 30,
                "Tán Tiên" => 60,
                _ => int.MaxValue // Kim Tiên không tiến hóa thêm được
            };

            if (pet.level < requiredLevel)
            {
                Debug.LogWarning($"PetSystem: {pet.petName} needs Lv.{requiredLevel} to evolve (current: {pet.level}).");
                return false;
            }

            string oldGrade = pet.grade;
            pet.grade = pet.grade switch
            {
                "Trân Thú" => "Tán Tiên",
                "Tán Tiên" => "Kim Tiên",
                _ => pet.grade
            };

            // Tăng chỉ số mạnh khi tiến hóa
            pet.str += 10; pet.intStat += 10; pet.vit += 10; pet.agi += 10;
            pet.hpMax += 50; pet.hpCurrent = pet.hpMax;

            Debug.Log($"PetSystem: [{pet.petName}] evolved from [{oldGrade}] to [{pet.grade}]!");

            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.AppendChatMessage("Hệ thống",
                    $"💫 Tiên Sủng [{pet.petName}] đã tiến hóa thành công [{pet.grade}]!");
            }
            return true;
        }

        // ============================================================
        // Skill Learning System
        // ============================================================

        /// <summary>
        /// Cho pet học một kỹ năng mới (tối đa 6 ô).
        /// </summary>
        public bool LearnSkill(int petId, PetSkill skill)
        {
            PetProfile pet = petRoster.Find(p => p.id == petId);
            if (pet == null) return false;

            if (pet.skills.Count >= MAX_SKILL_SLOTS)
            {
                Debug.LogWarning($"PetSystem: {pet.petName} already has {MAX_SKILL_SLOTS} skills. Cannot learn more.");
                return false;
            }

            // Kiểm tra kỹ năng đã học chưa
            if (pet.skills.Exists(s => s.skillId == skill.skillId))
            {
                Debug.LogWarning($"PetSystem: {pet.petName} already knows skill [{skill.skillName}].");
                return false;
            }

            pet.skills.Add(skill);
            Debug.Log($"PetSystem: [{pet.petName}] learned new skill [{skill.skillName}]! ({pet.skills.Count}/{MAX_SKILL_SLOTS})");
            return true;
        }

        /// <summary>
        /// Tẩy tủy pet — reset kỹ năng để học lại từ đầu.
        /// </summary>
        public void ResetSkills(int petId)
        {
            PetProfile pet = petRoster.Find(p => p.id == petId);
            if (pet == null) return;

            int count = pet.skills.Count;
            pet.skills.Clear();
            Debug.Log($"PetSystem: [{pet.petName}] had {count} skills cleared (Tẩy Tủy).");
        }

        // ============================================================
        // Helper
        // ============================================================

        private string GetPetDisplayName(string petType)
        {
            return petType switch
            {
                "tho_ngoc" => "Thỏ Ngọc",
                "tieu_toan_phong" => "Tiểu Toàn Phong",
                "ky_lan_con" => "Kỳ Lân Con",
                "phuong_hoang_lua" => "Phượng Hoàng Lửa",
                "than_long" => "Thần Long",
                "bai_ze" => "Bạch Trạch",
                _ => petType
            };
        }

        /// <summary>
        /// Lấy thông tin Pet theo ID.
        /// </summary>
        public PetProfile GetPet(int petId) => petRoster.Find(p => p.id == petId);
    }
}
