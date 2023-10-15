using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModShardLauncher.Mods
{
    public class Mod
    {
        public override string ToString()
        {
            return Name;
        }
        public bool Enable = false;
        public virtual string Name { get => GetType().Name; }
        public virtual string Author { get => "未知"; }
        public virtual string Description { get => "未知"; }
        public Mod() { }
        public bool isEnabled {  get; set; }
        public virtual void LoadAssembly()
        {

        }
        public virtual void LoadMod() 
        { 

        }
        public virtual void PatchMod()
        {

        }
    }
    public enum ModLanguage
    {
        Russian,
        English,
        Chinese,
        German,
        Spanish,
        Franch,
        Italian,
        Portuguese,
        Polish,
        Turkish,
        Japanese,
        Korean
    }
}
