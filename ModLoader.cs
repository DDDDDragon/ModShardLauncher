using System.Collections.Generic;
using ModShardLauncher.Mods;
using System.Linq;
using System.Windows;
using UndertaleModLib;
using System.Diagnostics;
using UndertaleModLib.Decompiler;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Controls;

namespace ModShardLauncher
{
    public class ModLoader
    {
        internal static UndertaleData Data => DataLoader.data;
        public static string ModPath => Path.Join(Environment.CurrentDirectory, "Mods");
        public static GlobalDecompileContext context = new GlobalDecompileContext(Data, false);
        public static Dictionary<string, Mod> Mods = new Dictionary<string, Mod>();
        private static List<Assembly> Assemblies = new List<Assembly>();
        public static List<string> Weapons;
        public static List<string> WeaponDescriptions;
        public static void ShowMessage(string msg)
        {
            Trace.Write(msg);
        }
        public async static Task Initalize()
        {
            Weapons = await GetTable("gml_GlobalScript_table_weapons");
            WeaponDescriptions = await GetTable("gml_GlobalScript_table_weapons_text");
        }
        public async static Task<List<string>> GetTable(string name)
        {
            var table = Data.Code.First(t => t.Name.Content.IndexOf(name) != -1);
            var text = Decompiler.Decompile(table, context);
            var ret = Regex.Match(text, "return (\\[.*\\])").Groups[1].Value;
            return JsonConvert.DeserializeObject<List<string>>(ret);
        }
        public async static Task<string> GetDecompiledFunction(string name)
        {
            var func = Data.Code.First(t => t.Name.Content.IndexOf(name) != -1);
            var text = Decompiler.Decompile(func, context);
            return text;
        }
        public async static Task<string> GetDisassemblyFunction(string name)
        {
            var func = Data.Code.First(t => t.Name.Content.IndexOf(name) != -1);
            var text = func.Disassemble(Data.Variables, Data.CodeLocals.For(func));
            return text;
        }
        public static void SetTable(List<string> table, string name)
        {
            var ret = JsonConvert.SerializeObject(table).Replace("\n", "");
            var target = Data.Code.First(t => t.Name.Content.IndexOf(name) != -1);
            GlobalDecompileContext context = new GlobalDecompileContext(Data, false);
            var text = Decompiler.Decompile(target, context);
            text = Regex.Replace(text, "\\[.*\\]", ret);
            target.ReplaceGML(text, Data);
        }
        public static Weapon GetWeapon(string ID)
        {
            var str = Weapons.First(t => t.StartsWith(ID));
            var weapon = Weapon.String2Weapon(str);
            var descs = WeaponDescriptions.First(t => t.StartsWith(ID)).Split(";").ToList();
            descs.Remove("");
            foreach (var desc in descs)
            {
                if (descs.IndexOf(desc) != 0)
                    weapon.Description.Add((ModLanguage)(descs.IndexOf(desc) - 1), desc);
            }
            return weapon;
        }
        public static void SetWeapon(string ID, Weapon weapon)
        {
            var target = Weapons.First(t => t.StartsWith(ID));
            var desc = WeaponDescriptions.First(t => t.StartsWith(ID));
            var index = Weapons.IndexOf(target);
            var index2 = WeaponDescriptions.IndexOf(desc);
            Weapons[index] = Weapon.Weapon2String(weapon).Item1;
            WeaponDescriptions[index] = Weapon.Weapon2String(weapon).Item2;
        }
        public static async Task LoadAssemblies()
        {
            var mods = MainWindow.Instance.MainTree.Items[0] as TreeViewItem;
            mods.Items.Clear();
            Mods.Clear();
            var files = Directory.GetFiles(ModPath, "*.dll");
            foreach (var item in files)
            {
                Assembly assembly = Assembly.LoadFrom(item);
                if (assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Mod))).Count() == 0)
                {
                    MessageBox.Show("加载错误: " + assembly.GetName().Name + " 此Mod需要一个Mod类");
                    continue;
                }
                else
                {
                    var type = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Mod))).ToList()[0];
                    var mod = Activator.CreateInstance(type) as Mod;
                    mod.LoadAssembly();
                    AddMod(mod);
                }
                Assemblies.Add(assembly);
            }
        }
        private static void AddMod(Mod mod)
        {
            var mods = MainWindow.Instance.MainTree.Items[0] as TreeViewItem;
            mods.Items.Add(mod);
            Mods.Add(mod.Name, mod);
        }
        public static async Task LoadMods()
        {
            Assembly ass = Assembly.GetEntryAssembly();
            foreach (var assembly in Assemblies)
            {
                if (assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Mod))).Count() == 0)
                {
                    MessageBox.Show("加载错误: " + assembly.GetName().Name + " 此Mod需要一个Mod类");
                    continue;
                }
                Type[] types = assembly.GetTypes().Where(t => !t.IsAbstract).ToArray();
                foreach (var type in types)
                {
                    //if (type.IsSubclassOf(typeof(Item))) LoadItem(type);
                    //if (type.IsSubclassOf(typeof(RPGNPC))) LoadNPC(type);
                    if (type.IsSubclassOf(typeof(Mod)))
                    {
                        var mod = Activator.CreateInstance(type) as Mod;
                        mod.LoadAssembly();
                        //Mods.Add(mod);
                    }
                }
            }
        }
        public static async Task PatchFile()
        {
            await PatchInnerFile();
            SetTable(Weapons, "gml_GlobalScript_table_weapons");
            SetTable(WeaponDescriptions, "gml_GlobalScript_table_weapons_text");
        }
        private static async Task PatchInnerFile()
        {

        }
    }
}
