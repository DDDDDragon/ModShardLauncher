using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModShardLauncher.Mods
{
    public class Weapon
    {
        public Weapon()
        {
            Description = new Dictionary<ModLanguage, string>();
        }
        public string? Name;
        public Dictionary<ModLanguage, string>? Description;
        public string? ID;
        public string? Slot;
        public string? Rare;
        public string? Material;
        public string? tags;
        public string? NoDrop;
        public int MaxDuration;
        public int Price;
        public int Lv;
        public int E;
        public int Rng;
        public int WeaponDamage;
        public int ArmorDamage;
        public int ArmorPiercing;
        public int BodypartDamage;
        public int SlashingDamage;
        public int PiercingDamage;
        public int BluntDamage;
        public int RendingDamage;
        public int FireDamage;
        public int ShockDamage;
        public int PoisonDamage;
        public int CausticDamage;
        public int FrostDamage;
        public int ArcaneDamage;
        public int UnholyDamage;
        public int SacredDamage;
        public int PsionicDamage;
        public int HitChance;
        public int CRT;
        public int CRTD;
        public int PRR;
        public int BlockPower;
        public int CTA;
        public int FMB;
        public int EVS;
        public int BleedingChance;
        public int DazeChance;
        public int StunChance;
        public int KnockbackChance;
        public int ImmobChance;
        public int StaggerChance;
        public int MP;
        public int MPRestoration;
        public int CooldownReduction;
        public int SkillsEnergyCost;
        public int SpellsEnergyCost;
        public int MagicPower;
        public int MiscastChance;
        public int MiracleChance;
        public int MiraclePower;
        public int BackfireDamage;
        public int PyromanticPower;
        public int GeomanticPower;
        public int VenomanticPower;
        public int ElectromanticPower;
        public int CryomanticPower;
        public int ArcanisticPower;
        public int AstromanticPower;
        public int PsimanticPower;
        public int ChronomanticPower;
        public int HealthRestoration;
        public int Lifesteal;
        public int Manasteal;
        public int BonusRange;
        public int RangeModifier;
        public int DamageReceived;
        public int DamageReturned;
        public int HealingReceived;
        public int STL;
        public int NoiseProduced;
        public int Balance;
        public int OffhandEfficiency;
        public int SlayingChance;

        public static (string, string) Weapon2String(Weapon weapon)
        {
            var type = typeof(Weapon);
            var str =
            "Name;ID;Slot;Rare;Material;MaxDuration;Lv;E;Price;Rng;WeaponDamage;ArmorDamage;" +
            "ArmorPiercing;BodypartDamage;SlashingDamage;PiercingDamage;BluntDamage;RendingDamage;" +
            "FireDamage;ShockDamage;PoisonDamage;CausticDamage;FrostDamage;ArcaneDamage;" +
            "UnholyDamage;SacredDamage;PsionicDamage;HitChance;CRT;CRTD;PRR;BlockPower;CTA;FMB;EVS;" +
            "BleedingChance;DazeChance;StunChance;KnockbackChance;ImmobChance;StaggerChance;MP;MPRestoration;" +
            "CooldownReduction;SkillsEnergyCost;SpellsEnergyCost;MagicPower;MiscastChance;MiracleChance;" +
            "MiraclePower;BackfireDamage;PyromanticPower;GeomanticPower;VenomanticPower;ElectromanticPower;" +
            "CryomanticPower;ArcanisticPower;AstromanticPower;PsimanticPower;ChronomanticPower;HealthRestoration;" +
            "Lifesteal;Manasteal;BonusRange;RangeModifier;DamageReceived;DamageReturned;HealingReceived;" +
            "STL;NoiseProduced;Balance;OffhandEfficiency;SlayingChance;tags;NoDrop;";
            var attributes = str.Split(";").ToList();
            attributes.Remove("");
            var ret = "";
            foreach (var attr in attributes)
            {
                var field = type.GetField(attr, BindingFlags.Public | BindingFlags.Instance);
                var value = field?.GetValue(weapon);
                if (value != null)
                {
                    if((field.FieldType == typeof(int) && (value as int?) != 0) || field.FieldType == typeof(string))
                        ret += value.ToString() + ";";
                    else ret += ";";
                }
                else ret += ";";
            }
            var des = string.Join(";", weapon.Description.Values.ToList());
            return (ret, des);
        }
        public static Weapon String2Weapon(string str)
        {
            var weapon = new Weapon();
            var attributes = str.Split(";").ToList();
            attributes.RemoveAt(attributes.Count - 1);
            var str2 =
            "Name;ID;Slot;Rare;Material;MaxDuration;Lv;E;Price;Rng;WeaponDamage;ArmorDamage;" +
            "ArmorPiercing;BodypartDamage;SlashingDamage;PiercingDamage;BluntDamage;RendingDamage;" +
            "FireDamage;ShockDamage;PoisonDamage;CausticDamage;FrostDamage;ArcaneDamage;" +
            "UnholyDamage;SacredDamage;PsionicDamage;HitChance;CRT;CRTD;PRR;BlockPower;CTA;FMB;EVS;" +
            "BleedingChance;DazeChance;StunChance;KnockbackChance;ImmobChance;StaggerChance;MP;MPRestoration;" +
            "CooldownReduction;SkillsEnergyCost;SpellsEnergyCost;MagicPower;MiscastChance;MiracleChance;" +
            "MiraclePower;BackfireDamage;PyromanticPower;GeomanticPower;VenomanticPower;ElectromanticPower;" +
            "CryomanticPower;ArcanisticPower;AstromanticPower;PsimanticPower;ChronomanticPower;HealthRestoration;" +
            "Lifesteal;Manasteal;BonusRange;RangeModifier;DamageReceived;DamageReturned;HealingReceived;" +
            "STL;NoiseProduced;Balance;OffhandEfficiency;SlayingChance;tags;NoDrop;";
            var attributes2 = str2.Split(";").ToList();
            attributes2.Remove("");
            foreach (var attr in attributes2)
            {
                var field = typeof(Weapon).GetField(attr, BindingFlags.Public | BindingFlags.Instance);
                if (field.FieldType == typeof(string)) field?.SetValue(weapon, attributes[attributes2.IndexOf(attr)]);
                else if (attributes[attributes2.IndexOf(attr)] == "") field?.SetValue(weapon, 0);
                else field?.SetValue(weapon, int.Parse(attributes[attributes2.IndexOf(attr)]));
            }
            return weapon;
        }
        public void SetDefaults()
        {

        }
    }
}
