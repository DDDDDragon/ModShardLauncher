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
using UndertaleModLib.Models;
using ModShardLauncher.Extensions;
using UndertaleModLib.Compiler;
using System.Text;
using System.Xml.Linq;
using ModShardLauncher.Pages;
using Serilog;

namespace ModShardLauncher
{
    public static class ModLoader
    {
        internal static UndertaleData Data => DataLoader.data;
        public static string ModPath => Path.Join(Environment.CurrentDirectory, "Mods");
        public static string ModSourcesPath => Path.Join(Environment.CurrentDirectory, "ModSources");
        public static Dictionary<string, ModFile> Mods = new Dictionary<string, ModFile>();
        public static Dictionary<string, ModSource> ModSources = new Dictionary<string, ModSource>();
        private static List<Assembly> Assemblies = new List<Assembly>();
        private static bool patched = false;
        public static List<string> Weapons;
        public static List<string> WeaponDescriptions;
        public static void ShowMessage(string msg)
        {
            Trace.Write(msg);
        }
        public static void Initalize()
        {
            Weapons = GetTable("gml_GlobalScript_table_weapons");
            WeaponDescriptions = GetTable("gml_GlobalScript_table_weapons_text");
        }
        public static UndertaleGameObject AddObject(string name)
        {
            var obj = new UndertaleGameObject()
            {
                Name = Data.Strings.MakeString(name)
            };
            if(Data.GameObjects.FirstOrDefault(t => t.Name.Content == name) == default)
                Data.GameObjects.Add(obj);
            return obj;
        }
        public static UndertaleGameObject GetObject(string name)
        {
            return Data.GameObjects.FirstOrDefault(t => t.Name.Content == name);
        }
        public static UndertaleSprite GetSprite(string name)
        {
            return Data.Sprites.FirstOrDefault(t => t.Name.Content == name);
        }
        public static void SetObject(string name, UndertaleGameObject o)
        {
            var obj = Data.GameObjects.First(t => t.Name.Content.IndexOf(name) != -1);
            Data.GameObjects[Data.GameObjects.IndexOf(obj)] = o;
        }
        public static UndertaleCode AddCode(string Code, string name)
        {
            var code = new UndertaleCode();
            var locals = new UndertaleCodeLocals();
            code.Name = Data.Strings.MakeString(name);
            locals.Name = code.Name;
            UndertaleCodeLocals.LocalVar argsLocal = new()
            {
                Name = Data.Strings.MakeString("arguments"),
                Index = 0
            };
            locals.Locals.Add(argsLocal);
            code.LocalsCount = 1;
            Data.CodeLocals.Add(locals);
            code.ReplaceGML(Code, Data);
            Data.Code.Add(code);
            return code;
        }
        public static UndertaleCode AddFunction(string Code, string name)
        {
            var scriptCode = AddCode(Code, name);
            Data.Code.Add(Data.Code[0]);
            Data.Code.RemoveAt(0);
            return scriptCode;
        }
        public static List<string> GetTable(string name)
        {
            var table = Data.Code.First(t => t.Name.Content.IndexOf(name) != -1);
            GlobalDecompileContext context = new GlobalDecompileContext(Data, false);
            var text = Decompiler.Decompile(table, context);
            var ret = Regex.Match(text, "return (\\[.*\\])").Groups[1].Value;
            return JsonConvert.DeserializeObject<List<string>>(ret);
        }
        public static UndertaleVariable GetVariable(string name)
        {
            try {
                UndertaleVariable variable = Data.Variables.First(t => t.Name?.Content == name);
                Log.Information(string.Format("Find variable: {0}", variable.ToString()));

                return variable;
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static UndertaleString GetString(string name)
        {
            try {
                UndertaleString variable = Data.Strings.First(t => t.Content == name);
                Log.Information(string.Format("Find string: {0}", variable.ToString()));

                return variable;
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static FileEnumerable<string> LoadGML(string fileName)
        {
            try {
                UndertaleCode code = ModLoader.GetUMTCodeFromFile(fileName);
                GlobalDecompileContext context = new(ModLoader.Data, false);

                return new(
                    new(
                        fileName,
                        code,
                        PatchingWay.GML
                    ),
                    Decompiler.Decompile(code, context).Split("\n")
                );
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static FileEnumerable<string> LoadAssemblyAsString(string fileName)
        {
            try {
                UndertaleCode code = ModLoader.GetUMTCodeFromFile(fileName);

                return new(
                    new(
                        fileName,
                        code,
                        PatchingWay.AssemblyAsString
                    ),
                    code.Disassemble(ModLoader.Data.Variables, ModLoader.Data.CodeLocals.For(code)).Split("\n")
                );
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        /// <summary>
        /// Return the UndertaleCode from <c>name</c>.
        /// </summary>
        public static UndertaleCode GetUMTCodeFromFile(string fileName)
        {
            try {
                UndertaleCode code = Data.Code.First(t => t.Name?.Content == fileName);
                Log.Information(string.Format("Find function: {0}", code.ToString()));

                return code;
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static string GetStringGMLFromFile(string fileName)
        {
            try {
                UndertaleCode code = GetUMTCodeFromFile(fileName);
                GlobalDecompileContext context = new(Data, false);

                return Decompiler.Decompile(code, context);
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static void SetStringGMLInFile(string codeAsString, string fileName)
        {
            try {
                UndertaleCode code = GetUMTCodeFromFile(fileName);
                code.ReplaceGML(codeAsString, Data);
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        /// <summary>
        /// Insert GML <paramref name="codeAsString"/> from a string in <paramref name="fileName"/> at a given <paramref name="position"/>.
        /// <para>
        /// <example>For example:
        /// <code>
        /// InsertGMLString("scr_atr("LVL") == global.max_level", "gml_Object_o_character_panel_mask_Draw_0", 3);
        /// </code>
        /// results in gml_Object_o_character_panel_mask_Draw_0 line 3 being <c>scr_atr("LVL") == global.max_level</c>.
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="codeAsString">The code to insert.</param>
        /// <param name="fileName">The file to be patched.</param>
        /// <param name="position">The exact position to insert.</param>
        public static void InsertGMLString(string codeAsString, string fileName, int position)
        {
            try {
                Log.Information(string.Format("Trying insert code in: {0}", fileName.ToString()));

                List<string>? originalCode = GetStringGMLFromFile(fileName).Split("\n").ToList();
                originalCode.Insert(position, codeAsString);
                SetStringGMLInFile(string.Join("\n", originalCode), fileName);

                Log.Information(string.Format("Patched function with InsertGMLString: {0}", fileName.ToString()));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        /// <summary>
        /// Replace an existing GML code by another <paramref name="code"/> from a string in <paramref name="file"/> at a given <paramref name="position"/>.
        /// <para>
        /// <example>For example:
        /// <code>
        /// ReplaceGMLString("scr_atr("LVL") == global.max_level", "gml_Object_o_character_panel_mask_Draw_0", 3);
        /// </code>
        /// results in gml_Object_o_character_panel_mask_Draw_0 line 3 being replaced by <c>scr_atr("LVL") == global.max_level</c>.
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="code">The code to insert.</param>
        /// <param name="file">The file to be patched.</param>
        /// <param name="position">The exact position to insert.</param>
        public static void ReplaceGMLString(string codeAsString, string fileName, int position)
        {
            try {
                Log.Information(string.Format("Trying replace code in: {0}", fileName.ToString()));

                List<string>? originalCode = GetStringGMLFromFile(fileName).Split("\n").ToList();
                originalCode[position] = codeAsString;
                SetStringGMLInFile(string.Join("\n", originalCode), fileName);

                Log.Information(string.Format("Patched function with ReplaceGMLString: {0}", fileName.ToString()));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        /// <summary>
        /// Replace an existing GML code by another <paramref name="code"/> from a string in <paramref name="file"/> at a given <paramref name="position"/>
        /// and remove the next len-1 lines.
        /// <para>
        /// <example>For example:
        /// <code>
        /// ReplaceGMLString("scr_atr("LVL") == global.max_level", "gml_Object_o_character_panel_mask_Draw_0", 3, 2);
        /// </code>
        /// results in gml_Object_o_character_panel_mask_Draw_0 line 3 being replaced by <c>scr_atr("LVL") == global.max_level</c>
        /// and line 4 being removed.
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="code">The code to insert.</param>
        /// <param name="file">The file to be patched.</param>
        /// <param name="position">The exact position to insert.</param>
        public static void ReplaceGMLString(string codeAsString, string fileName, int start, int len)
        {
            try {
                Log.Information(string.Format("Trying replace code in: {0}", fileName.ToString()));

                List<string>? originalCode = GetStringGMLFromFile(fileName).Split("\n").ToList();
                originalCode[start] = codeAsString;
                for (int i = 1; i < Math.Min(len, originalCode.Count - start); i++) {
                    originalCode[start + i] = "";
                }

                SetStringGMLInFile(string.Join("\n", originalCode), fileName);

                Log.Information(string.Format("Patched function with ReplaceGMLString: {0}", fileName.ToString()));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static string GetAssemblyString(string fileName)
        {
            try {
                UndertaleCode func = GetUMTCodeFromFile(fileName);
                return func.Disassemble(Data.Variables, Data.CodeLocals.For(func));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static void SetAssemblyString(string codeAsString, string fileName)
        {
            try {
                UndertaleCode originalCode = GetUMTCodeFromFile(fileName);
                originalCode.Replace(Assembler.Assemble(codeAsString, Data));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static void InsertAssemblyString(string codeAsString, string fileName, int position)
        {
            try {
                Log.Information(string.Format("Trying insert assembly in: {0}", fileName.ToString()));

                List<string>? originalCode = GetAssemblyString(fileName).Split("\n").ToList();
                originalCode.Insert(position, codeAsString);
                SetAssemblyString(string.Join("\n", originalCode), fileName);

                Log.Information(string.Format("Patched function with InsertDisassemblyCode: {0}", fileName.ToString()));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static void ReplaceAssemblyString(string codeAsString, string fileName, int position)
        {
            try {
                Log.Information(string.Format("Trying replace assembly in: {0}", fileName.ToString()));

                List<string>? originalCode = GetAssemblyString(fileName).Split("\n").ToList();
                originalCode[position] = codeAsString;
                SetAssemblyString(string.Join("\n", originalCode), fileName);

                Log.Information(string.Format("Patched function with ReplaceDisassemblyCode: {0}", fileName.ToString()));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static void ReplaceAssemblyString(string codeAsString, string fileName, int start, int len)
        {
            try {
                Log.Information(string.Format("Trying replace assembly in: {0}", fileName.ToString()));

                List<string>? originalCode = GetAssemblyString(fileName).Split("\n").ToList();
                originalCode[start] = codeAsString;
                for (int i = 1; i < Math.Min(len, originalCode.Count - start); i++) {
                    originalCode[start + i] = "";
                }

                SetAssemblyString(string.Join("\n", originalCode), fileName);

                Log.Information(string.Format("Patched function with ReplaceDisassemblyCode: {0}", fileName.ToString()));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static void InjectAssemblyInstruction(string name, Func<IEnumerable<UndertaleInstruction>, IEnumerable<UndertaleInstruction>> patch)
        {
            try {
                Log.Information(string.Format("Trying patch assembly in: {0}", name.ToString()));

                UndertaleCode originalCode = GetUMTCodeFromFile(name);
                originalCode.Replace(patch(originalCode.Instructions).ToList());

                Log.Information(string.Format("Patched function with PatchDisassemblyCode: {0}", name.ToString()));
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
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
            var descs = WeaponDescriptions.FindAll(t => t.StartsWith(ID))[1].Split(";").ToList();
            descs.Remove("");
            descs.RemoveAt(0);
            var names = WeaponDescriptions.First(t => t.StartsWith(ID)).Split(";").ToList();
            names.Remove("");
            names.RemoveAt(0);
            var weapon = new Weapon(str, descs, names);
            return weapon;
        }
        public static void SetWeapon(string ID, Weapon weapon)
        {
            var target = Weapons.First(t => t.StartsWith(ID));
            var name = WeaponDescriptions.First(t => t.StartsWith(ID));
            var desc = WeaponDescriptions.FindAll(t => t.StartsWith(ID))[1];
            var index = Weapons.IndexOf(target);
            var index2 = WeaponDescriptions.IndexOf(desc);
            var index3 = WeaponDescriptions.IndexOf(name);
            Weapons[index] = Weapon.Weapon2String(weapon).Item1;
            WeaponDescriptions[index2] = Weapon.Weapon2String(weapon).Item2;
            WeaponDescriptions[index3] = Weapon.Weapon2String(weapon).Item3;
        }
        public static void LoadFiles()
        {
            var mods = Main.Instance.ModPage.Mods;
            var modSources = Main.Instance.ModSourcePage.ModSources;
            foreach(ModFile i in mods)
                if(i.Stream != null) i.Stream.Close();
            var modCaches = new List<ModFile>();
            Mods.Clear();
            modSources.Clear();
            ModSources.Clear();

            // List all folders being a C# project
            // Currently only test the existence of a .csproj file
            // TODO: test framework
            // TODO: test inclusion of ModShardLauncher as a reference
            IEnumerable<string> sources = Directory
                .GetDirectories(ModSourcesPath)
                .Where(
                    x => Directory
                        .EnumerateFiles(x, "*.csproj", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault() 
                        != null
            );

            foreach(var i in sources)
            {
                var info = new ModSource()
                {
                    Name = i.Split("\\").Last(),
                    Path = i
                };
                modSources.Add(info);
                ModSources.Add(info.Name, info);
            }
            var files = Directory.GetFiles(ModPath, "*.sml");
            foreach (var file in files)
            {
                var f = FileReader.Read(file);
                if (f == null) continue;
                Assembly assembly = f.Assembly;
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
                    mod.ModFiles = f;
                    f.instance = mod;
                    var old = mods.FirstOrDefault(t => t.Name == f.Name);
                    if (old != null) f.isEnabled = old.isEnabled;
                    modCaches.Add(f);
                }
                Assemblies.Add(assembly);
            }
            mods.Clear();
            modCaches.ForEach(i => {
                mods.Add(i);
                Mods.Add(i.Name, i);
            });
        }
        public static void PatchMods()
        {
            Assembly ass = Assembly.GetEntryAssembly();
            var mods = ModInfos.Instance.Mods;
            foreach (ModFile mod in mods)
            {
                if (!mod.isEnabled) continue;
                if (!mod.isExisted)
                {
                    MessageBox.Show(Application.Current.FindResource("ModLostWarning").ToString() + " : " + mod.Name);
                    continue;
                }
                Main.Settings.EnableMods.Add(mod.Name);
                var version = DataLoader.GetVersion();
                var reg = new Regex("0([0-9])");
                version = reg.Replace(version, "$1");
                if (mod.Version != version)
                {
                    var result = MessageBox.Show(Application.Current.FindResource("VersionDifferentWarning").ToString(),
                        Application.Current.FindResource("VersionDifferentWarningTitle").ToString() + " : " + mod.Name, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No) continue;
                }
                TextureLoader.LoadTextures(mod);
                mod.instance.PatchMod();
                var modAss = mod.Assembly;
                Type[] types = modAss.GetTypes().Where(t => !t.IsAbstract).ToArray();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Weapon))) 
                        LoadWeapon(type);
                }
            }
        }
        public static void LoadWeapon(Type type)
        {
            var weapon = Activator.CreateInstance(type) as Weapon;
            weapon.SetDefaults();
            var strs = weapon.AsString();
            Weapons.Insert(Weapons.IndexOf("SWORDS - BLADES;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;") + 1, strs.Item1);
            WeaponDescriptions.Insert(WeaponDescriptions.IndexOf(";;SWORDS;;;;;;SWORDS;SWORDS;;;;") + 1, weapon.Name + ";" + string.Join(";", weapon.NameList.Values));
            WeaponDescriptions.Insert(WeaponDescriptions.IndexOf(";weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;weapon_desc;") + 1,
                weapon.Name + ";" + string.Join(";", weapon.WeaponDescriptions.Values));
            WeaponDescriptions.Insert(WeaponDescriptions.IndexOf(";weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;weapon_pronoun;") + 1,
                weapon.Name + ";He;;;It;She;She;She;She;He;;;;");
        }
        public static async void PatchFile()
        {
            PatchInnerFile();
            PatchMods();
            SetTable(Weapons, "gml_GlobalScript_table_weapons");
            SetTable(WeaponDescriptions, "gml_GlobalScript_table_weapons_text");
            LoadFiles();
        }
        internal static void PatchInnerFile()
        {
            AddExtension(new ModShard());
            var engine = AddObject("o_ScriptEngine");
            engine.Persistent = true;
            var ev = new UndertaleGameObject.Event();
            ev.EventSubtypeStep = EventSubtypeStep.Step;
            ev.Actions.Add(new UndertaleGameObject.EventAction()
            {
                CodeId = AddCode(@"if(GetScript() != ""NoScript"")
{
    var scr = string_split(GetScript(),"" "")
    var scriptID = asset_get_index(scr[0])
    array_delete(scr, 0, 1)
    for(i = 0;i<array_length(scr);i++)
        scr[i]=string_replace(scr[i],""_"","" "")
    var ret = """"
    if(scriptID == -1)
        ret = (""script is wrong: "" + GetScript())
    else
    {
        if(array_length(scr) > 0)
            ret = script_execute_ext(scriptID, scr)
        else ret = script_execute(scriptID)
    }
    RunCallBack(string(ret))
    PopScript()
}", "ScriptEngine_step")
            });
            engine.Events[3].Add(ev);
            AddFunction(@"function print(argument0)
{
    show_message(argument0)
}","print");
            AddFunction(@"function give(argument0)
{
    with (o_inventory)
    {
        with (scr_inventory_add_weapon(argument0, (1 << 0)))
            scr_inv_atr_set(""Duration"", 100)
    }
}", "give");
            var create = new UndertaleGameObject.Event();
            create.Actions.Add(new UndertaleGameObject.EventAction()
            {
                CodeId = AddCode(@"ScriptThread()", "ScriptEngine_create")
            });
            engine.Events[0].Add(create);
            var start = Data.Rooms.First(t => t.Name.Content == "START");
            var newObj = new UndertaleRoom.GameObject()
            {
                ObjectDefinition = engine,
                InstanceID = Data.GeneralInfo.LastObj++
            };

            start.GameObjects.Add(newObj);
        }
        public static void AddExtension(UndertaleExtensionFile file)
        {
            var ext = Data.Extensions.First(t => t.Name.Content == "display_mouse_lock");
            ext.Files.Add(file);
        }
    }
}
