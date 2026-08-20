using System.Collections.Generic;
using UnityEngine;

namespace BasePlatformer.Fairy
{
    public enum FairyType
    {
        Attack_Burn,
        Attack_Freeze,
        Attack_Explosion,
        Platform_Barrier,
        Platform_JumpBoost,
        Platform_SpeedUp
    }

    [System.Serializable]
    public class FairyInfo
    {
        public FairyType fairyType;
        public GameObject fairyPrefab;         // 생성할 정령 프리팹
        public MainFairyData fairyData;        // ScriptableObject 데이터 (비워둘 경우 프리팹 내 컴포넌트에서 자동 추출)
        public Sprite fairySprite;             // 카드에 표시할 스프라이트 (비워둘 경우 프리팹 SpriteRenderer에서 자동 추출)
        public Color fairyColor = Color.white; // 카드 색상 / 틴트

        public string GetCategory()
        {
            MainFairyData data = GetFairyData();
            if (data != null && !string.IsNullOrEmpty(data.Category))
                return data.Category;

            return fairyType.ToString().StartsWith("Attack") ? "공격 정령" : "플랫폼 정령";
        }

        public string GetFairyName()
        {
            MainFairyData data = GetFairyData();
            if (data != null && !string.IsNullOrEmpty(data.FairyName))
                return data.FairyName;

            return fairyPrefab != null ? fairyPrefab.name : fairyType.ToString();
        }

        public string GetDescription()
        {
            MainFairyData data = GetFairyData();
            if (data != null && !string.IsNullOrEmpty(data.Description))
                return data.Description;

            return "";
        }

        public MainFairyData GetFairyData()
        {
            if (fairyData != null) return fairyData;
            return ExtractFairyData(fairyPrefab);
        }

        public static MainFairyData ExtractFairyData(GameObject obj)
        {
            if (obj == null) return null;

            var atk = obj.GetComponentInChildren<FairyAttackController>(true);
            if (atk != null && atk.fairyData != null) return atk.fairyData;

            var burn = obj.GetComponentInChildren<FairyBurnAttackController>(true);
            if (burn != null && burn.fairyData != null) return burn.fairyData;

            var freeze = obj.GetComponentInChildren<FairyFreezeAttackController>(true);
            if (freeze != null && freeze.fairyData != null) return freeze.fairyData;

            var explosion = obj.GetComponentInChildren<FairyExplosionAttackController>(true);
            if (explosion != null && explosion.fairyData != null) return explosion.fairyData;

            var plat = obj.GetComponentInChildren<FairyPlatformController>(true);
            if (plat != null && plat.fairyData != null) return plat.fairyData;

            return null;
        }

        public Color GetColor()
        {
            if (fairyColor != Color.white) return fairyColor;
            if (fairyPrefab != null)
            {
                var sr = fairyPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null) return sr.color;
            }
            return fairyColor;
        }
    }
}
