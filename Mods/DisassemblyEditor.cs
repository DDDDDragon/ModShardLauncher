﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;

namespace ModShardLauncher.Mods
{
    public class DisassemblyEditor
    {
        public DisassemblyEditor() { }
        public UndertaleData Data => DataLoader.data;
        public DisassemblyEditor(UndertaleCode code)
        {
            Code = code;
            var text = code.Disassemble(Data.Variables, Data.CodeLocals.For(Code));
            CodeContents = text.Split("\n").ToList();
        }

        public UndertaleCode Code = new UndertaleCode();
        public List<string> CodeContents = new List<string>();
        public int Index { get; private set; } = 0;

        public bool TryGotoNext(Func<string, bool> predicate)
        {
            if (CodeContents.FirstOrDefault(predicate) == default) return false;
            else
            {
                Index = CodeContents.IndexOf(CodeContents.First(predicate));
                return true;
            }
        }
        public void Emit(OpCodes o)
        {

        }
    }
    public enum OpCodes
    {
        Conv,
        Mul,
        Div,
        Rem,
        Mod,
        Add,
        Sub
    }
}
