using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Microsoft.CodeAnalysis;
using ModShardLauncher.Mods;
using UndertaleModLib.Models;

namespace ModShardLauncher
{
    /// <summary>
    /// Utility class for localization.
    /// </summary>
    static public class Localization
    {
        /// <summary>
        /// Immutable List of all possible languages defined by ModLanguage
        /// </summary>
        static public readonly ImmutableList<ModLanguage> LanguageList = Enum.GetValues(typeof(ModLanguage)).Cast<ModLanguage>().ToImmutableList();
        /// <summary>
        /// Convert a string delimited by semi-colons into a dictionnary of ModLanguage.
        /// It assumes the string follows the game convention order (first Russian, then English, and so on), and will use by default the second language (English) as the default one.
        /// You can change the default language by changing <paramref name="indexDefault"/>.
        /// <example>
        /// For example:
        /// <code>
        /// Localization.ToDict("test");
        /// </code>
        /// returns the dictionnary { Russian: "test", English: "test", Chinese: "test", German: "test", Spanish: "test", French: "test", Italian: "test", Portuguese: "test", Polish: "test", Turkish: "test", Japanese: "test", Korean: "test"}.
        /// <code>
        /// Localization.ToDict("testRu;testEn;testCh");
        /// </code>
        /// returns the dictionnary { Russian: "testRu", English: "testEn", Chinese: "testCh", German: "testEn", Spanish: "testEn", French: "testEn", Italian: "testEn", Portuguese: "testEn", Polish: "testEn", Turkish: "testEn", Japanese: "testEn", Korean: "testEn"}.
        /// </example>
        /// </summary>
        /// <param name="values"></param>
        /// <param name="indexDefault"></param>
        /// <returns></returns>
        static private Dictionary<ModLanguage, string> ToDict(string values, int indexDefault = 1)
        {
            string[] strings = values.Split(";");
            Dictionary<ModLanguage, string> tmp = LanguageList.Zip(strings).ToDictionary(x => x.First, x => x.Second);

            if (strings.Length <= indexDefault) indexDefault = 0;
            string defaultName = strings[indexDefault];

            foreach (ModLanguage language in LanguageList)
            {
                if (!tmp.ContainsKey(language))
                {
                    tmp[language] = defaultName;
                }
            }

            return tmp;
        }
        /// <summary>
        /// Fill a destination ModLanguage dictionary with the values contained in a source dictionary.
        /// The source dictionary is assumed to always have an English key which will be used as default if there is missing values in the source dictionary.
        /// <example>
        /// For example:
        /// <code>
        /// Localization.SetDictionary(new Dictionary &lt; ModLanguage, string &gt; () { {Russian, "testRu"} , {English, "testEn"}, {Italian, "testIt"} });
        /// </code>
        /// returns the dictionnary { Russian: "testRu", English: "testEn", Chinese: "testEn", German: "testEn", Spanish: "testEn", French: "testEn", Italian: "testIt", Portuguese: "testEn", Polish: "testEn", Turkish: "testEn", Japanese: "testEn", Korean: "testEn"}.
        /// </example>
        /// </summary>
        /// <param name="source"></param>
        static public Dictionary<ModLanguage, string> SetDictionary(Dictionary<ModLanguage, string> source)
        {
            Dictionary<ModLanguage, string> dest = new();
            string englishName = source[ModLanguage.English];
            foreach (ModLanguage language in LanguageList)
            { 
                if (!source.ContainsKey(language))
                {
                    dest[language] = englishName;
                }
                else
                {
                    dest[language] = source[language];
                }
            }
            return dest;
        }
        /// <summary>
        /// Fill a destination ModLanguage dictionary with the values contained in a source string delimited by semi-colon. 
        /// If no semi-colon are found, it assumes there is only one language filled out, so every keys are set with the same value.
        /// Else it uses the <see cref="ToDict"/> function to create a dictionary from the string.
        /// <example>
        /// For example:
        /// <code>
        /// Localization.SetDictionary("testRu;testEn;testCh;testGe");
        /// </code>
        /// returns the dictionnary { Russian: "testRu", English: "testEn", Chinese: "testCh", German: "testGe", Spanish: "testEn", French: "testEn", Italian: "testEn", Portuguese: "testEn", Polish: "testEn", Turkish: "testEn", Japanese: "testEn", Korean: "testEn"}.
        /// </example>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static public Dictionary<ModLanguage, string> SetDictionary(string source)
        {
            Dictionary<ModLanguage, string> dest = new();
            if (source.Contains(';'))
            {
                dest = ToDict(source);
            } 
            else
            {
                foreach (ModLanguage language in LanguageList)
                {
                    dest[language] = source;
                }
            }
            return dest;
        }
    }
    /// <summary>
    /// Class to abstract the localization of items.
    /// </summary>
    public class LocalizationItem
    {
        /// <summary>
        /// Name of the object in the localization table.
        /// </summary>
        public string OName { get; set; }
        /// <summary>
        /// Dictionary that contains for each available languages a translation of the item name as displayed in-game.
        /// </summary>
        public Dictionary<ModLanguage, string> ConsumableName { get; set; } = new();
        /// <summary>
        /// Dictionary that contains for each available languages a translation of the item effect as displayed in-game.
        /// </summary>
        public Dictionary<ModLanguage, string> ConsumableID { get; set; } = new();
        /// <summary>
        /// Dictionary that contains for each available languages a translation of the item description as displayed in-game.
        /// </summary>
        public Dictionary<ModLanguage, string> ConsumableDescription { get; set; } = new();
        /// <summary>
        /// Return an instance of <see cref="LocalizationItem"/> with empty <see cref="ConsumableName"/>, <see cref="ConsumableID"/> and <see cref="ConsumableDescription"/>.
        /// <example>
        /// For example:
        /// <code>
        /// LocalizationItem("myTestItem");
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="oName"></param>
        public LocalizationItem(string oName)
        {
            OName = oName;
        }
        /// <summary>
        /// Return an instance of <see cref="LocalizationItem"/> with <see cref="ConsumableName"/>, <see cref="ConsumableID"/> and <see cref="ConsumableDescription"/> filled by input dictionaries.
        /// It is expected to have at least an English key for each dictionary. It does not need to follow the convention order of the localization table.
        /// <example>
        /// For example:
        /// <code>
        /// LocalizationItem("myTestItem", 
        ///     new Dictionary &lt; ModLanguage, string &gt; () { {Russian, "testRu"}, {English, "testEn"}, {Italian, "testIt"} },
        ///     new Dictionary &lt; ModLanguage, string &gt; () { {Russian, "effectRu"}, {English, "effectEn"}, {Italian, "effectIt"} },
        ///     new Dictionary &lt; ModLanguage, string &gt; () { {Russian, "descRu"}, {English, "descEn"}, {Italian, "descIt"} } );
        /// </code>
        /// </example>
       /// </summary>
        /// <param name="oName"></param>
        /// <param name="dictName"></param>
        /// <param name="dictID"></param>
        /// <param name="dictDescription"></param>
        public LocalizationItem(string oName, Dictionary<ModLanguage, string> dictName, Dictionary<ModLanguage, string> dictID, Dictionary<ModLanguage, string> dictDescription)
        {
            OName = oName;
            ConsumableName = Localization.SetDictionary(dictName);
            ConsumableID = Localization.SetDictionary(dictID);
            ConsumableDescription = Localization.SetDictionary(dictDescription);
        }
        /// <summary>
        /// Return an instance of <see cref="LocalizationItem"/> with <see cref="ConsumableName"/>, <see cref="ConsumableID"/> and <see cref="ConsumableDescription"/> filled by input strings delimited by semi-colon.
        /// It is expected to follow the convention order of the localization table.
        /// <example>
        /// For example:
        /// <code>
        /// LocalizationItem("myTestItem", 
        ///     "testRu;testEn;testCh",
        ///     "effectRu;effectEn;effectCh",
        ///     "descRu;descEn;descIt");
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="oName"></param>
        /// <param name="valuesName"></param>
        /// <param name="valuesID"></param>
        /// <param name="valuesDescription"></param>
        public LocalizationItem(string oName, string valuesName, string valuesID, string valuesDescription)
        {
            OName = oName;
            ConsumableName = Localization.SetDictionary(valuesName);
            ConsumableID = Localization.SetDictionary(valuesID);
            ConsumableDescription = Localization.SetDictionary(valuesDescription);
        }
        /// <summary>
        /// Create a string delimited by semi-colon that follows the in-game convention order for localization of items.
        /// <example>
        /// For example:
        /// <code>
        /// CreateLine("testItem", new Dictionary &lt; ModLanguage, string &gt; () {{Russian, "testRu"}, {English, "testEn"}, {Chinese, "testCh"}, {German, "testGe"}, {Spanish, "testSp"}, 
        /// {French, "testFr"}, {Italian, "testIt"}, {Portuguese, "testPr"}, {Polish, "testPl"}, {Turkish, "testTu"}, {Japanese, "testJp"}, {Korean, "testKr"}} );
        /// </code>
        /// returns the string "testItem;testRu;testEn;testCh;testGe;testSp;testFr;testIt;testPr;testPl;testTu;testJp;testKr;//;".
        /// </example>
        /// </summary>
        /// <param name="oName"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        static private string CreateLine(string oName, Dictionary<ModLanguage, string> dict)
        {
            string line = oName;
            foreach (KeyValuePair<ModLanguage, string> kp in dict)
            {
                line += ";";
                line += kp.Value;
            }
            return line + ";//;";
        }
        /// <summary>
        /// Browse a table with an iterator, and at special lines, yield a new line constructed by the dictionaries <see cref="ConsumableName"/>, <see cref="ConsumableID"/> and <see cref="ConsumableDescription"/>.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private IEnumerable<string> EditTable(IEnumerable<string> table)
        {
            foreach (string line in table)
            {
                if (line.Contains("consum_name_end"))
                {
                    yield return CreateLine(OName, ConsumableName);
                }
                else if (line.Contains("consum_mid_end"))
                {
                    yield return CreateLine(OName, ConsumableID);
                }
                else if (line.Contains("consum_desc_end"))
                {
                    yield return CreateLine(OName, ConsumableDescription);
                }
                yield return line;
            }
        }
        /// <summary>
        /// Browse a table with an iterator, and at special lines, insert a new line constructed by the dictionaries <see cref="ConsumableName"/>, <see cref="ConsumableID"/> and <see cref="ConsumableDescription"/>.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public void InjectTable()
        {
            List<string> table = Msl.ThrowIfNull(ModLoader.GetTable("gml_GlobalScript_table_consumables"));
            ModLoader.SetTable(EditTable(table).ToList(), "gml_GlobalScript_table_consumables");
        }
    }
    public class LocalizationSentence
    {
        public string Id { get; set; }
        public string Tags { get; set; } = "any";
        public string Role { get; set; } = "any";
        public string Type { get; set; } = "any";
        public string Faction { get; set; } = "any";
        public string Settlement { get; set; } = "any";
        public Dictionary<ModLanguage, string> Sentence { get; set; } = new();
        public LocalizationSentence(string id)
        {
            Id = id;
        }
        public LocalizationSentence(string id, Dictionary<ModLanguage, string> sentence)
        {
            Id = id;
            Sentence = Localization.SetDictionary(sentence);
            
        }
        public LocalizationSentence(string id, string sentence)
        {
            Id = id;
            Sentence = Localization.SetDictionary(sentence);
        }
        public string CreateLine()
        {
            string line = string.Format("{0};{1};{2};{3};{4};{5}", Id, Tags, Role, Type, Faction, Settlement);
            foreach (KeyValuePair<ModLanguage, string> kp in Sentence)
            {
                line += ";";
                line += kp.Value;
            }
            return line + ";";
        }
    }
    public class LocalizationDialog
    {
        public List<LocalizationSentence> Sentences { get; set; } = new();
        public LocalizationDialog(params LocalizationSentence[] sentences)
        {
            foreach (LocalizationSentence sentence in sentences)
            {   
                Sentences.Add(sentence);
            }
            
        }
        private IEnumerable<string> EditTable(IEnumerable<string> table)
        {
            foreach (string line in table)
            {
                if (line.Contains("NPC - GREETINGS;"))
                {
                    yield return line;

                    foreach (LocalizationSentence sentence in Sentences) 
                    {
                        yield return sentence.CreateLine();
                    }
                }
            }
        }
        private IEnumerable<string> EditTable(string tableName)
        {
            return EditTable(Msl.ThrowIfNull(ModLoader.GetTable(tableName)));
        }
        public void InjectTable()
        {
            ModLoader.SetTable(EditTable("gml_GlobalScript_table_NPC_Lines").ToList(), "gml_GlobalScript_table_NPC_Lines");
        }
    }
}