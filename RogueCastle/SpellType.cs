// 
// RogueLegacyArchipelago - SpellType.cs
// Last Modified 2021-12-24
// 
// This project is based on the modified disassembly of Rogue Legacy's engine, with permission to do so by its
// original creators. Therefore, former creators' copyright notice applies to the original disassembly.
// 
// Original Disassembled Source - © 2011-2015, Cellar Door Games Inc.
// Rogue Legacy™ is a trademark or registered trademark of Cellar Door Games Inc. All Rights Reserved.
// 

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueCastle.TypeDefinitions;

namespace RogueCastle
{
    internal class SpellType
    {
        public const byte None = 0;
        public const byte Dagger = 1;
        public const byte Axe = 2;
        public const byte TimeBomb = 3;
        public const byte TimeStop = 4;
        public const byte Nuke = 5;
        public const byte Translocator = 6;
        public const byte Displacer = 7;
        public const byte Boomerang = 8;
        public const byte DualBlades = 9;
        public const byte Close = 10;
        public const byte DamageShield = 11;
        public const byte Bounce = 12;
        public const byte DragonFire = 13;
        public const byte RapidDagger = 14;
        public const byte DragonFireNeo = 15;
        public const byte Total = 16;
        public const byte Laser = 100;
        public const byte Shout = 20;

        public static string ToString(byte spellType)
        {
            switch (spellType)
            {
                case 1:
                    return "Dagger";
                case 2:
                    return "Axe";
                case 3:
                    return "Bomb";
                case 4:
                    return "Time Stop";
                case 5:
                    return "Crow Storm";
                case 6:
                    return "Quantum Translocator";
                case 7:
                    return "Displacer";
                case 8:
                    return "Chakram";
                case 9:
                    return "Scythe";
                case 10:
                    return "Blade Wall";
                case 11:
                    return "Flame Barrier";
                case 12:
                    return "Conflux";
                case 13:
                case 15:
                    return "Dragon Fire";
                case 14:
                    return "Rapid Dagger";
                default:
                    if (spellType != 100)
                    {
                        return "";
                    }
                    return "B.E.A.M";
            }
        }

        public static string Description(byte spellType)
        {
            switch (spellType)
            {
                case 1:
                    return "[Input:" + 24 + "]  Fires a dagger directly in front of you.";
                case 2:
                    return "[Input:" + 24 + "]  Throws a giant axe in an arc.";
                case 3:
                    return "[Input:" + 24 + "]  Summons a bomb that explodes after a while.";
                case 4:
                    return "[Input:" + 24 + "]  Toggles freezing all enemies on-screen. ";
                case 5:
                    return "[Input:" + 24 + "]  Hits all enemies on screen. Costly.";
                case 6:
                    return "[Input:" + 24 + "]  Drops and teleports to your shadow.";
                case 7:
                    return "[Input:" + 24 + "]  Sends out a marker which teleports you.";
                case 8:
                    return "[Input:" + 24 + "]  Throws a chakram which comes back to you.";
                case 9:
                    return "[Input:" + 24 + "]  Send Scythes flying out from above you.";
                case 10:
                    return "[Input:" + 24 + "]  Summon a Grand Blade to defend you.";
                case 11:
                    return "[Input:" + 24 + "]  Encircles you with protective fire.";
                case 12:
                    return "[Input:" + 24 + "]  Launches orbs that bounce everywhere.";
                case 13:
                case 15:
                    return "[Input:" + 24 + "]  Shoot fireballs at your enemies.";
                case 14:
                    return "[Input:" + 24 + "]  Fire a barrage of daggers.";
                default:
                    if (spellType != 100)
                    {
                        return "";
                    }
                    return "[Input:" + 24 + "]  Fire a laser that blasts everyone it touches.";
            }
        }

        public static string Icon(byte spellType)
        {
            switch (spellType)
            {
                case 1:
                    return "DaggerIcon_Sprite";
                case 2:
                    return "AxeIcon_Sprite";
                case 3:
                    return "TimeBombIcon_Sprite";
                case 4:
                    return "TimeStopIcon_Sprite";
                case 5:
                    return "NukeIcon_Sprite";
                case 6:
                    return "TranslocatorIcon_Sprite";
                case 7:
                    return "DisplacerIcon_Sprite";
                case 8:
                    return "BoomerangIcon_Sprite";
                case 9:
                    return "DualBladesIcon_Sprite";
                case 10:
                    return "CloseIcon_Sprite";
                case 11:
                    return "DamageShieldIcon_Sprite";
                case 12:
                    return "BounceIcon_Sprite";
                case 13:
                case 15:
                    return "DragonFireIcon_Sprite";
                case 14:
                    return "RapidDaggerIcon_Sprite";
                default:
                    if (spellType != 100)
                    {
                        return "DaggerIcon_Sprite";
                    }
                    return "DaggerIcon_Sprite";
            }
        }

        public static Vector3 GetNext3Spells()
        {
            var spellList = ClassType.GetSpellList(9);
            var list = new List<byte>();
            var array = spellList;
            for (var i = 0; i < array.Length; i++)
            {
                var item = array[i];
                list.Add(item);
            }
            var num = list.IndexOf(Game.PlayerStats.Spell);
            list.Clear();
            var array2 = new byte[3];
            for (var j = 0; j < 3; j++)
            {
                array2[j] = spellList[num];
                num++;
                if (num >= spellList.Length)
                {
                    num = 0;
                }
            }
            return new Vector3(array2[0], array2[1], array2[2]);
        }
    }
}
