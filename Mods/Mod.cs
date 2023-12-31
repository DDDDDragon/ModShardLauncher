﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        public virtual string Name { get => GetType().Name; }
        public virtual string Author { get => "未知"; }
        public virtual string Description { get => "未知"; }
        public virtual string ShortDesc { get => "未知"; }
        public virtual string Version { get => "v0.0.0.0"; }
        public List<Weapon> ModWeapons;
        public ModFile ModFiles;
        public Mod() { }
        public virtual void LoadAssembly()
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
