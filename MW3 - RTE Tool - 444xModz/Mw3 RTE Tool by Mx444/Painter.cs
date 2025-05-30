﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using PS3Lib;

using Call_Of_Duty_Multi_Tool_1;
namespace Call_Of_Duty_Multi_Tool_1._0._0
{
    public partial class Painter : MetroFramework.Forms.MetroForm
    {
    
        public Painter()
        {
            InitializeComponent();
            RPC.PS3.Connect();
            RPC.PS3.Attach();
            RPC.Enable();
            
        }

        private void Painter_Load(object sender, EventArgs e)
        {
          
        }
            #region Vezah FPS RPC
        public class RPC
        {
            #region PS3
            public class PS3
            {

                private static uint GetProcessID()
                {
                    uint[] array;
                    PS3TMAPI.GetProcessList(0, out array);
                    return array[0];
                }
                public static Int32 Target = 0;
                public static String GetTargetName()
                {
                    if ((Parameters.ConsoleName == null) || (Parameters.ConsoleName == string.Empty))
                    {
                        PS3TMAPI.InitTargetComms();
                        PS3TMAPI.TargetInfo targetInfo = new PS3TMAPI.TargetInfo
                        {
                            Flags = PS3TMAPI.TargetInfoFlag.TargetID,
                            Target = Target
                        };
                        PS3TMAPI.GetTargetInfo(ref targetInfo);
                        Parameters.ConsoleName = targetInfo.Name;
                    }
                    return Parameters.ConsoleName;
                }
                public static UInt32 ProcessID()
                {
                    return Parameters.ProcessID;
                }
                public class Parameters
                {
                    public static PS3TMAPI.ConnectStatus connectStatus;
                    public static string ConsoleName;
                    public static string info;
                    public static string MemStatus;
                    public static uint ProcessID;
                    public static uint[] processIDs;
                    public static byte[] Retour;
                    public static string snresult;
                    public static string Status;
                    public static string usage;
                }
                public enum ResetTarget
                {
                    Hard,
                    Quick,
                    ResetEx,
                    Soft
                }
                public static Boolean Attach()
                {
                    Boolean flag = false;
                    PS3TMAPI.GetProcessList((Int32)Target, out Parameters.processIDs);
                    if (Parameters.processIDs.Length > 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        ulong num = Parameters.processIDs[0];
                        Parameters.ProcessID = Convert.ToUInt32(num);
                        PS3TMAPI.ProcessAttach((Int32)Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID);
                        PS3TMAPI.ProcessContinue((Int32)Target, Parameters.ProcessID);
                        Parameters.info = "The Process 0x" + Parameters.ProcessID.ToString("X8") + " Has Been Attached !";
                    }
                    return flag;
                }
                public static Boolean Connect(Int32 TargetInPS3 = 0)
                {
                    Boolean flag = false;
                    Target = TargetInPS3;
                    flag = PS3TMAPI.SUCCEEDED(PS3TMAPI.InitTargetComms());
                    return PS3TMAPI.SUCCEEDED(PS3TMAPI.Connect(TargetInPS3, null));
                }
                public static void GetMemory(uint addr, ref byte[] Buffer)
                {
                    PS3TMAPI.ProcessGetMemory(0, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, 0, addr, ref Buffer);
                }
                public static void SetMemory(UInt32 Address, Byte[] bytes)
                {
                    PS3TMAPI.ProcessSetMemory(0, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, 0L, (ulong)Address, bytes);
                }
                public static Byte[] GetMem(UInt32 Address, Int32 Length)
                {
                    Byte[] buff = new Byte[Length];
                    PS3TMAPI.ProcessGetMemory(0, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, 0, Address, ref buff);
                    return buff;
                }
                public static Byte[] SetMem(UInt32 Address, Int32 Length)
                {
                    Byte[] bytes = new Byte[Length];
                    PS3TMAPI.ProcessSetMemory(0, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, 0L, (ulong)Address, bytes);
                    return bytes;
                }
                public static float ReadFloat(UInt32 offset)
                {
                    byte[] myBuffer = PS3.GetMem(offset, 4);
                    Array.Reverse(myBuffer, 0, 4);
                    return BitConverter.ToSingle(myBuffer, 0);
                }
                public static void WriteFloat(UInt32 offset, float input)
                {
                    byte[] array = new byte[4];
                    BitConverter.GetBytes(input).CopyTo(array, 0);
                    Array.Reverse(array, 0, 4);
                    PS3.SetMemory(offset, array);
                }

                public static Byte ReadByte(UInt32 address)
                {
                    return PS3.GetMem(address, 1)[0];
                }

                public static Byte[] ReadBytes(UInt32 address, Int32 length)
                {
                    return PS3.GetMem(address, length);
                }

                public static Int32 ReadInt32(UInt32 address)
                {
                    Byte[] memory = PS3.GetMem(address, 4);
                    Array.Reverse(memory, 0, 4);
                    return BitConverter.ToInt32(memory, 0);
                }

                public static float ReadSingle(UInt32 address)
                {
                    Byte[] memory = PS3.GetMem(address, 4);
                    Array.Reverse(memory, 0, 4);
                    return BitConverter.ToSingle(memory, 0);
                }

                public static float[] ReadSingle(UInt32 address, Int32 length)
                {
                    Byte[] memory = PS3.GetMem(address, length * 4);
                    ReverseBytes(memory);
                    float[] numArray = new float[length];
                    for (Int32 i = 0; i < length; i++)
                    {
                        numArray[i] = BitConverter.ToSingle(memory, ((length - 1) - i) * 4);
                    }
                    return numArray;
                }

                public static string ReadString(UInt32 address)
                {
                    Int32 length = 40;
                    Int32 num2 = 0;
                    string source = "";
                    do
                    {
                        Byte[] memory = PS3.GetMem(address + ((UInt32)num2), length);
                        source = source + Encoding.UTF8.GetString(memory);
                        num2 += length;
                    }
                    while (!source.Contains<char>('\0'));
                    Int32 inPS3 = source.IndexOf('\0');
                    string str2 = source.Substring(0, inPS3);
                    source = string.Empty;
                    return str2;
                }

                public static Byte[] ReverseBytes(Byte[] toReverse)
                {
                    Array.Reverse(toReverse);
                    return toReverse;
                }

                public static void WriteByte(UInt32 address, Byte input)
                {
                    PS3.SetMemory(address, new Byte[] { input });
                }

                public static void WriteBytes(UInt32 address, Byte[] input)
                {
                    PS3.SetMemory(address, input);
                }

                public static bool WriteBytesToggle(uint Offset, Byte[] On, Byte[] Off)
                {
                    bool flag = ReadByte(Offset) == On[0];
                    WriteBytes(Offset, !flag ? On : Off);
                    return flag;
                }

                public static void WriteInt16(UInt32 address, short input)
                {
                    Byte[] array = new Byte[2];
                    ReverseBytes(BitConverter.GetBytes(input)).CopyTo(array, 0);
                    PS3.SetMemory(address, array);
                }

                public static void WriteInt32(UInt32 address, Int32 input)
                {
                    Byte[] array = new Byte[4];
                    ReverseBytes(BitConverter.GetBytes(input)).CopyTo(array, 0);
                    PS3.SetMemory(address, array);
                }

                public static void WriteSingle(UInt32 address, float input)
                {
                    Byte[] array = new Byte[4];
                    BitConverter.GetBytes(input).CopyTo(array, 0);
                    Array.Reverse(array, 0, 4);
                    PS3.SetMemory(address, array);
                }

                public static void WriteSingle(UInt32 address, float[] input)
                {
                    Int32 length = input.Length;
                    Byte[] array = new Byte[length * 4];
                    for (Int32 i = 0; i < length; i++)
                    {
                        ReverseBytes(BitConverter.GetBytes(input[i])).CopyTo(array, (Int32)(i * 4));
                    }
                    PS3.SetMemory(address, array);
                }

                public static void WriteString(UInt32 address, String input)
                {
                    Byte[] Bytes = Encoding.UTF8.GetBytes(input);
                    Array.Resize<byte>(ref Bytes, Bytes.Length + 1);
                    PS3.SetMemory(address, Bytes);
                }

                public static void WriteUInt16(UInt32 address, ushort input)
                {
                    Byte[] array = new Byte[2];
                    BitConverter.GetBytes(input).CopyTo(array, 0);
                    Array.Reverse(array, 0, 2);
                    PS3.SetMemory(address, array);
                }

                public static void WriteUInt32(UInt32 address, UInt32 input)
                {
                    Byte[] array = new Byte[4];
                    BitConverter.GetBytes(input).CopyTo(array, 0);
                    Array.Reverse(array, 0, 4);
                    PS3.SetMemory(address, array);
                }

            }
            
            #endregion
            public static uint func_address = 0x0277208; //FPS Address 1.24
            /*
             * MW3 FPS RPC by VezahHFH!
             * This function get written at the FPS Offset!
             *
                 lis r28, 0x1005
                 lwz r12, 0x48(r28)
                 cmpwi r12, 0
                 beq loc_277290
                 lwz r3, 0x00(r28)
                 lwz r4, 0x04(r28)
                 lwz r5, 0x08(r28)
                 lwz r6, 0x0C(r28)
                 lwz r7, 0x10(r28)
                 lwz r8, 0x14(r28)
                 lwz r9, 0x18(r28)
                 lwz r10, 0x1C(r28)
                 lwz r11, 0x20(r28)
                 lfs f1, 0x24(r28)
                 lfs f2, 0x28(r28)
                 lfs f3, 0x2C(r28)
                 lfs f4, 0x30(r28)
                 lfs f5, 0x34(r28)
                 lfs f6, 0x38(r28)
                 lfs f7, 0x3C(r28)
                 lfs f8, 0x40(r28)
                 lfs f9, 0x44(r28)
                 mtctr r12
                 bctrl
                 li r4, 0
                 stw r4, 0x48(r28)
                 stw r3, 0x4C(r28)
                 stfs f1, 0x50(r28)
                 b loc_277290
             */
            public static uint GetFuncReturn()
            {
                byte[] ret = new byte[4];
                PS3.GetMemory(0x114AE64, ref ret);
                Array.Reverse(ret);
                return BitConverter.ToUInt32(ret, 0);
            }
            public static void Enable()
            {
                byte[] CheckRPC = new byte[1];
                PS3.GetMemory(0x27720C, ref CheckRPC);
                if (CheckRPC[0] == 0x80)
                {
                    byte[] WritePPC = new byte[] {0x3F,0x80,0x10,0x05,0x81,0x9C,0x00,0x48,0x2C,0x0C,0x00,0x00,0x41,0x82,0x00,0x78,
                                        0x80,0x7C,0x00,0x00,0x80,0x9C,0x00,0x04,0x80,0xBC,0x00,0x08,0x80,0xDC,0x00,0x0C,
                                        0x80,0xFC,0x00,0x10,0x81,0x1C,0x00,0x14,0x81,0x3C,0x00,0x18,0x81,0x5C,0x00,0x1C,
                                        0x81,0x7C,0x00,0x20,0xC0,0x3C,0x00,0x24,0xC0,0x5C,0x00,0x28,0xC0,0x7C,0x00,0x2C,
                                        0xC0,0x9C,0x00,0x30,0xC0,0xBC,0x00,0x34,0xC0,0xDC,0x00,0x38,0xC0,0xFC,0x00,0x3C,
                                        0xC1,0x1C,0x00,0x40,0xC1,0x3C,0x00,0x44,0x7D,0x89,0x03,0xA6,0x4E,0x80,0x04,0x21,
                                        0x38,0x80,0x00,0x00,0x90,0x9C,0x00,0x48,0x90,0x7C,0x00,0x4C,0xD0,0x3C,0x00,0x50,
                                        0x48,0x00,0x00,0x14};
                    PS3.SetMemory(func_address, new byte[] { 0x41 });
                    PS3.SetMemory(func_address + 4, WritePPC);
                    PS3.SetMemory(func_address, new byte[] { 0x40 });
                    System.Threading.Thread.Sleep(10);
                    RPC.DestroyAll();


                }
                else if (CheckRPC[0] == 0x3F)
                {
                    MessageBox.Show("RPC Is Already Enabled\nWe Don't Want You To Freeze Now Do We ", "Draw FX By kiwi_modz");
                }
            }
            public static Int32 Call(UInt32 address, params Object[] parameters)
            {
                Int32 length = parameters.Length;
                Int32 index = 0;
                UInt32 count = 0;
                UInt32 Strings = 0;
                UInt32 Single = 0;
                UInt32 Array = 0;
                while (index < length)
                {
                    if (parameters[index] is Int32)
                    {
                        PS3.WriteInt32(0x10050000 + (count * 4), (Int32)parameters[index]);
                        count++;
                    }
                    else if (parameters[index] is UInt32)
                    {
                        PS3.WriteUInt32(0x10050000 + (count * 4), (UInt32)parameters[index]);
                        count++;
                    }
                    else if (parameters[index] is Int16)
                    {
                        PS3.WriteInt16(0x10050000 + (count * 4), (Int16)parameters[index]);
                        count++;
                    }
                    else if (parameters[index] is UInt16)
                    {
                        PS3.WriteUInt16(0x10050000 + (count * 4), (UInt16)parameters[index]);
                        count++;
                    }
                    else if (parameters[index] is Byte)
                    {
                        PS3.WriteByte(0x10050000 + (count * 4), (Byte)parameters[index]);
                        count++;
                    } //Should work now :D let me try
                    else
                    {
                        UInt32 pointer;
                        if (parameters[index] is String)
                        {
                            pointer = 0x10052000 + (Strings * 0x400);
                            PS3.WriteString(pointer, Convert.ToString(parameters[index]));
                            PS3.WriteUInt32(0x10050000 + (count * 4), pointer);
                            count++;
                            Strings++;
                        }
                        else if (parameters[index] is Single)
                        {
                            WriteSingle(0x10050024 + (Single * 4), (Single)parameters[index]);
                            Single++;
                        }
                        else if (parameters[index] is Single[])
                        {
                            Single[] Args = (Single[])parameters[index];
                            pointer = 0x10051000 + Array * 4;
                            WriteSingle(pointer, Args);
                            PS3.WriteUInt32(0x10050000 + count * 4, pointer);
                            count++;
                            Array += (UInt32)Args.Length;
                        }

                    }
                    index++;
                }
                PS3.WriteUInt32(0x10050048, address);
                System.Threading.Thread.Sleep(20);
                return PS3.ReadInt32(0x1005004c);
            }
            private static void WriteSingle(uint address, float input)
            {
                byte[] array = new byte[4];
                BitConverter.GetBytes(input).CopyTo(array, 0);
                Array.Reverse(array, 0, 4);
                PS3.SetMemory(address, array);
            }
            private static byte[] ReverseBytes(byte[] inArray)
            {
                Array.Reverse(inArray);
                return inArray;
            }
            private static void WriteSingle(uint address, float[] input)
            {
                int length = input.Length;
                byte[] array = new byte[length * 4];
                for (int i = 0; i < length; i++)
                {
                    ReverseBytes(BitConverter.GetBytes(input[i])).CopyTo(array, (int)(i * 4));
                }
                PS3.SetMemory(address, array);
            }
            public static void DestroyAll()
            {
                Byte[] clear = new Byte[0xB4 * 1024];
                PS3.SetMemory(0xF0E10C, clear);
            }

            public static Single[] ReadSingle(uint address, int length)
            {
                byte[] mem = PS3.ReadBytes(address, length * 4);
                Array.Reverse(mem);
                float[] numArray = new float[length];
                for (int index = 0; index < length; ++index)
                    numArray[index] = BitConverter.ToSingle(mem, (length - 1 - index) * 4);
                return numArray;
            }
        #endregion
            #region PlayerFX
            public static void Earthquake(int Duration, float[] origin, float radius, float scale)
            {
                int ent = RPC.Call(0x1C0B7C, origin, 0x5F);
                PS3.WriteFloat((uint)ent + 0x5C, radius);
                PS3.WriteFloat((uint)ent + 0x54, scale);
                PS3.WriteFloat((uint)ent + 0x58, Duration);
                PS3.WriteInt32((uint)ent + 0xD8, 0x00);
            }
            public static uint PlayFX(float[] Origin, int EffectIndex)
            {
                uint ent = (uint)RPC.Call(0x1C0B7C, Origin, 0x56); //G_Temp
                PS3.WriteInt32(ent + 0xA0, EffectIndex);
                PS3.WriteInt32(ent + 0xD8, 0);
                PS3.WriteFloat(ent + 0x40, 0f);
                PS3.WriteFloat(ent + 0x44, 0f);
                PS3.WriteFloat(ent + 0x3C, 270f);
                return ent;
            }
            public static void SetFX(UInt32 Client, Int32 FX_Value, uint Distance_in_Meters = 6)
            {
                float[] Origin = new float[] { PS3.ReadFloat(Offsets.G_Client + 0x1C + (0x3980 * (uint)Client)), PS3.ReadFloat(Offsets.G_Client + 0x20 + (0x3980 * (uint)Client)), PS3.ReadFloat(Offsets.G_Client + 0x24 + (0x3980 * (uint)Client)) };
                float[] Angles = PS3.ReadSingle(Offsets.Funcs.G_Client((int)Client) + 0x158, 3);
                #region AnglestoForward
                float diff = Distance_in_Meters * 40;
                float num = ((float)Math.Sin((Angles[0] * Math.PI) / 180)) * diff;
                float num1 = (float)Math.Sqrt(((diff * diff) - (num * num)));
                float num2 = ((float)Math.Sin((Angles[1] * Math.PI) / 180)) * num1;
                float num3 = ((float)Math.Cos((Angles[1] * Math.PI) / 180)) * num1;
                float[] Forward = new float[] { Origin[0] + num3, Origin[1] + num2, Origin[2] += 60 - num };
                Origin[2] += 50;
                #endregion
                #region Set FX Origin
                PlayFX(Forward, FX_Value);
            }
                #endregion
            #endregion
            #region Offsets
            class Offsets
            {
                public static UInt32 FuncAddr = 0x277208;
                public static UInt32 G_Client = 0x110a280;
                public static UInt32 G_ClientSize = 0x3980;
                public static UInt32 G_Entity = 0xfca280;
                public static UInt32 G_EntitySize = 640;
                public class Funcs
                {
                    public static UInt32 G_Client(Int32 clientIndex, UInt32 Mod = 0)
                    {
                        return ((Offsets.G_Client + Mod) + ((UInt32)(Offsets.G_ClientSize * clientIndex)));
                    }

                    public static UInt32 G_Entity(Int32 clientIndex, UInt32 Mod = 0)
                    {
                        return ((Offsets.G_Entity + Mod) + ((UInt32)(Offsets.G_EntitySize * clientIndex)));
                    }
                }

            }

        }
            #endregion
            #region Restart
        public static void restartGame()
        {
            RPC.Call(0x00223B20);
        }
        #endregion
            #region ServerInFo
        public static class ServerInfo
        {//credits to Seb5594 for this

            public static string GetName(int Client)
            {
                byte[] buffer = new byte[16];
                RPC.PS3.GetMemory(0x0110D694 + 0x3980 * (uint)Client, ref buffer);
                string names = Encoding.ASCII.GetString(buffer);
                names = names.Replace("\0", "");
                return names;
            }
            public static String ReturnInfos(Int32 Index)
            {

                return Encoding.ASCII.GetString(RPC.PS3.ReadBytes(0x8360d5, 0x100)).Replace(@"\", "|").Split(new char[] { '|' })[Index];

            }
            public static String getHostName()
            {
                String str = ReturnInfos(0x10);
                switch (str)
                {
                    case "Modern Warfare 3":
                        return "Dedicated Server (No Player is Host)";
                    case "":
                        return "You are not In-Game";
                }
                return str;
            }
            public static String getGameMode()
            {
                switch (ReturnInfos(2))
                {
                    case "war":
                        return "Team Deathmatch";
                    case "dm":
                        return "Free for All";
                    case "sd":
                        return "Search and Destroy";
                    case "dom":
                        return "Domination";
                    case "conf":
                        return "Kill Confirmed";
                    case "sab":
                        return "Sabotage";
                    case "koth":
                        return "Head Quartes";
                    case "ctf":
                        return "Capture The Flag";
                    case "infect":
                        return "Infected";
                    case "sotf":
                        return "Hunted";
                    case "dd":
                        return "Demolition";
                    case "grnd":
                        return "Drop Zone";
                    case "tdef":
                        return "Team Defender";
                    case "tjugg":
                        return "Team Juggernaut";
                    case "jugg":
                        return "Juggernaut";
                    case "gun":
                        return "Gun Game";
                    case "oic":
                        return "One In The Chamber";
                }
                return "Unknown Gametype";
            }
            public static String getMapName()
            {
                switch (ReturnInfos(6))
                {
                    case "mp_alpha":
                        return "Lockdown";
                    case "mp_bootleg":
                        return "Bootleg";
                    case "mp_bravo":
                        return "Mission";
                    case "mp_carbon":
                        return "Carbon";
                    case "mp_dome":
                        return "Dome";
                    case "mp_exchange":
                        return "Downturn";
                    case "mp_hardhat":
                        return "Hardhat";
                    case "mp_interchange":
                        return "Interchange";
                    case "mp_lambeth":
                        return "Fallen";
                    case "mp_mogadishu":
                        return "Bakaara";
                    case "mp_paris":
                        return "Resistance";
                    case "mp_plaza2":
                        return "Arkaden";
                    case "mp_radar":
                        return "Outpost";
                    case "mp_seatown":
                        return "Seatown";
                    case "mp_underground":
                        return "Underground";
                    case "mp_village":
                        return "Village";
                    case "mp_aground_ss":
                        return "Aground";
                    case "mp_aqueduct_ss":
                        return "Aqueduct";
                    case "mp_cement":
                        return "Foundation";
                    case "mp_hillside_ss":
                        return "Getaway";
                    case "mp_italy":
                        return "Piazza";
                    case "mp_meteora":
                        return "Sanctuary";
                    case "mp_morningwood":
                        return "Black Box";
                    case "mp_overwatch":
                        return "Overwatch";
                    case "mp_park":
                        return "Liberation";
                    case "mp_qadeem":
                        return "Oasis";
                    case "mp_restrepo_ss":
                        return "Lookout";

                    case "mp_terminal_cls":
                        return "Terminal";
                }
                return "Unknown Map";
            }
        }
        #endregion
            #region Ps3
       

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                HOST.Text = ServerInfo.getHostName();
                MAP.Text = ServerInfo.getMapName();
                MODE.Text = ServerInfo.getGameMode();

                for (int i = 0; i < 18; i++)
                {
                    dataGridView1.Enabled = true;
                    dataGridView1.RowCount = 18;
                    dataGridView1.Rows[i].Cells[0].Value = i;
                    dataGridView1.Rows[i].Cells[1].Value = ServerInfo.GetName(i);
                    Application.DoEvents();

                }
            }
            catch
            {
                for (int i = 0; i < 18; i++)
                {
                    HOST.Text = "Null";
                    MAP.Text = "Null";
                    MODE.Text = "Null";
                    dataGridView1.RowCount = 18;
                    dataGridView1.Rows[i].Cells[0].Value = i;
                    dataGridView1.Rows[i].Cells[1].Value = "";
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "DrawFX [ OFF ]")
            {
                Painter1.Start();
                button2.Text = "DrawFX [ ON ]";
                button2.ForeColor = Color.Green;
            }
            else if (button2.Text == "DrawFX [ ON ]")
            {
                Painter1.Stop();
                button2.Text = "DrawFX [ OFF ]";
                button2.ForeColor = Color.Crimson;
            }
        }

        private void Box_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((Box.Text == "Green") && (MAP.Text == "Dome"))
            {
                FXValue.Value = 92;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Dome"))
            {
                FXValue.Value = 66;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Seatown"))
            {
                FXValue.Value = 90;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Seatown"))
            {
                FXValue.Value = 64;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Arkaden"))
            {
                FXValue.Value = 98;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Arkaden"))
            {
                FXValue.Value = 72;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Bakaara"))
            {
                FXValue.Value = 88;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Bakaara"))
            {
                FXValue.Value = 62;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Resistance"))
            {
                FXValue.Value = 84;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Resistance"))
            {
                FXValue.Value = 58;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Downturn"))
            {
                FXValue.Value = 95;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Downturn"))
            {
                FXValue.Value = 69;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Bootleg"))
            {
                FXValue.Value = 105;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Bootleg"))
            {
                FXValue.Value = 79;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Bootleg"))
            {
                FXValue.Value = 105;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Bootleg"))
            {
                FXValue.Value = 79;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Carbon"))
            {
                FXValue.Value = 132;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Carbon"))
            {
                FXValue.Value = 106;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Hardhat"))
            {
                FXValue.Value = 100;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Hardhat"))
            {
                FXValue.Value = 74;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Lockdown"))
            {
                FXValue.Value = 86;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Lockdown"))
            {
                FXValue.Value = 60;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Village"))
            {
                FXValue.Value = 85;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Village"))
            {
                FXValue.Value = 59;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Fallen"))
            {
                FXValue.Value = 102;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Fallen"))
            {
                FXValue.Value = 76;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Outpost"))
            {
                FXValue.Value = 118;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Outpost"))
            {
                FXValue.Value = 92;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Interchange"))
            {
                FXValue.Value = 83;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Interchange"))
            {
                FXValue.Value = 57;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Underground"))
            {
                FXValue.Value = 109;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Underground"))
            {
                FXValue.Value = 83;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Mission"))
            {
                FXValue.Value = 87;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Mission"))
            {
                FXValue.Value = 61;
            }
            else if ((Box.Text == "Green") && (MAP.Text == "Terminal"))
            {
                FXValue.Value = 92;
            }
            else if ((Box.Text == "Red") && (MAP.Text == "Terminal"))
            {
                FXValue.Value = 66;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (RPC.PS3.ReadFloat(0x110a5f8 + ((uint)FXC.Value * 0x3980)) > 0)
            {
                RPC.SetFX((uint)FXC.Value, (Int32)FXValue.Value);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            FXC.Value = dataGridView1.CurrentRow.Index;
        }
    }
}
            #endregion