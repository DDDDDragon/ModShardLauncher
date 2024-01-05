using System;
using System.Linq;
using Serilog;
using UndertaleModLib;
using UndertaleModLib.Models;

namespace ModShardLauncher
{
    public static class AssemblyWrapper
    {
        public static UndertaleInstruction.Reference<UndertaleVariable> CreateRefVariable(string name, UndertaleInstruction.InstanceType instanceType) 
        {
            if (ModLoader.Data == null) {
                throw new NullReferenceException("Data is null");
            }

            UndertaleString str = ModLoader.Data.Strings.MakeString(name, out int id);
            bool bytecode14 = ModLoader.Data.GeneralInfo?.BytecodeVersion <= 14;
            uint oldId = ModLoader.Data.VarCount1;

            if (bytecode14)
			    instanceType = UndertaleInstruction.InstanceType.Undefined;

            if (!bytecode14)
            {
                if (ModLoader.Data.IsVersionAtLeast(2, 3))
                {
                    ModLoader.Data.VarCount1++;
                    ModLoader.Data.VarCount2 = ModLoader.Data.VarCount1;
                    oldId = (uint)id;
                }
                else if (!ModLoader.Data.DifferentVarCounts)
                {
                    // Bytecode 16+
                    ModLoader.Data.VarCount1++;
                    ModLoader.Data.VarCount2++;
                }
                else
                {
                    // Bytecode 15
                    if (instanceType == UndertaleInstruction.InstanceType.Self)
                    {
                        oldId = ModLoader.Data.VarCount2;
                        ModLoader.Data.VarCount2++;
                    }
                    else if (instanceType == UndertaleInstruction.InstanceType.Global)
                    {
                        ModLoader.Data.VarCount1++;
                    }
                }
            }

            UndertaleVariable variable = new()
            {
                Name = str,
                InstanceType = instanceType,
                VarID = bytecode14 ? 0 : (int)oldId,
                NameStringID = id
            };
            ModLoader.Data.Variables.Add(variable);
            Log.Information(string.Format("Created variable: {0}", variable.ToString()));

            return new UndertaleInstruction.Reference<UndertaleVariable>(variable, UndertaleInstruction.VariableType.Normal);
        }
        public static UndertaleInstruction.Reference<UndertaleVariable> GetRefVariableOrCreate(string name, UndertaleInstruction.InstanceType instanceType)
        {
            try {
                UndertaleInstruction.Reference<UndertaleVariable> refVariable;
                UndertaleVariable? variable = ModLoader.Data.Variables.FirstOrDefault(t => t.Name?.Content == name);
                
                if (variable == null) 
                    refVariable = CreateRefVariable(name, instanceType);
                else
                    refVariable = new UndertaleInstruction.Reference<UndertaleVariable>(variable, UndertaleInstruction.VariableType.Normal);

                Log.Information(string.Format("Find variable: {0}", refVariable.ToString()));

                return refVariable;
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static UndertaleResourceById<UndertaleString, UndertaleChunkSTRG> CreateString(string name) 
        {
            UndertaleString str = ModLoader.Data.Strings.MakeString(name, out int ind);
            Log.Information(string.Format("Created string: {0}", str.ToString()));
            return new UndertaleResourceById<UndertaleString, UndertaleChunkSTRG>(str, ind);
        }
        public static UndertaleResourceById<UndertaleString, UndertaleChunkSTRG> GetStringOrCreate(string name)
        {
            try {
                UndertaleResourceById<UndertaleString, UndertaleChunkSTRG> stringById;
                (int ind, UndertaleString str) = ModLoader.Data.Strings.Enumerate().FirstOrDefault(x => x.Item2.Content == name);
                
                if (str == null)
                    stringById = CreateString(name);
                else
                    stringById = new UndertaleResourceById<UndertaleString, UndertaleChunkSTRG>(str, ind);

                Log.Information(string.Format("Find string: {0}", stringById.ToString()));

                return stringById;
            }
            catch(Exception ex) {
                Log.Error(ex, "Something went wrong");
                throw;
            }
        }
        public static UndertaleInstruction PushShort(short val)
        {
            return new() {
                Kind = UndertaleInstruction.Opcode.PushI,
                Value = val,
                Type1 = UndertaleInstruction.DataType.Int16,
            };
        }
        
        public static UndertaleInstruction PushInt(int val)
        {
            return new() {
                Kind = UndertaleInstruction.Opcode.Push,
                Value = val,
                Type1 = UndertaleInstruction.DataType.Int32,
            };
        }

        public static UndertaleInstruction PushString(string val) 
        {
            return new() {
                Kind = UndertaleInstruction.Opcode.Push,
                Value = GetStringOrCreate(val),
                Type1 = UndertaleInstruction.DataType.String,
            };
        }

        public static UndertaleInstruction PushGlb(string val) 
        {
            return new() {
                Kind = UndertaleInstruction.Opcode.PushGlb,
                Value = GetRefVariableOrCreate(val, UndertaleInstruction.InstanceType.Global),
                Type1 = UndertaleInstruction.DataType.Variable,
                TypeInst = UndertaleInstruction.InstanceType.Global,
            };
        }

        public static UndertaleInstruction PopIntGlb(string val)
        {
            return new() {
                Kind = UndertaleInstruction.Opcode.Pop,
                Destination = GetRefVariableOrCreate(val, UndertaleInstruction.InstanceType.Global),
                Type1 = UndertaleInstruction.DataType.Variable,
                Type2 = UndertaleInstruction.DataType.Int32,
                TypeInst = UndertaleInstruction.InstanceType.Global,
            };
        }
        public static UndertaleInstruction PopIntLcl(string val)
        {
            return new() {
                Kind = UndertaleInstruction.Opcode.Pop,
                Destination = GetRefVariableOrCreate(val, UndertaleInstruction.InstanceType.Local),
                Type1 = UndertaleInstruction.DataType.Variable,
                Type2 = UndertaleInstruction.DataType.Int32,
                TypeInst = UndertaleInstruction.InstanceType.Local,
            };
        }
        
        public static UndertaleInstruction PopIntSelf(string val)
        {
            return new() {
                Kind = UndertaleInstruction.Opcode.Pop,
                Destination = GetRefVariableOrCreate(val, UndertaleInstruction.InstanceType.Self),
                Type1 = UndertaleInstruction.DataType.Variable,
                Type2 = UndertaleInstruction.DataType.Int32,
                TypeInst = UndertaleInstruction.InstanceType.Self,
            };
        }
    }
}