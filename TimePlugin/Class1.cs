using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.Events.EventArgs;
using Exiled.API.Interfaces;
using PlayerRoles;
using static MapGeneration.ImageGenerator;
using Exiled.CustomItems.API;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace CustomRole
{
    public class CustomRolesPlugin : Plugin<Config>
    {
        public override string Author => "Rozy";
        public override string Name => "CustomRole";
        public override string Prefix => "CustomRole";
        public override Version RequiredExiledVersion => new Version(8, 9, 11);

        private CoroutineHandle hintCoroutine;

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            Timing.KillCoroutines(hintCoroutine);
            base.OnDisabled();
        }

        private void OnRoundStarted()
        {
            var random = new System.Random();
            var players = Player.List.ToList();

            foreach (var player in players)
            {
                int roll = random.Next(0, 100);
                if (roll < 15) 
                {
                    AssignRole(player, CustomRole.HeadOfSecurity);
                }
                else if (roll < 25) 
                {
                    AssignRole(player, CustomRole.SecurityOfficer);
                }
                else if (roll < 30) 
                {
                    AssignRole(player, CustomRole.HeadOfScientists);
                }
                else if (roll < 40)
                {
                    AssignRole(player, CustomRole.Lucky);
                }
            }

            RenameDefaultRoles();

            hintCoroutine = Timing.RunCoroutine(ShowHints());
        }

        private void AssignRole(Player player, CustomRole role)
        {
            switch (role)
            {
                case CustomRole.HeadOfSecurity:
                    player.Role.Set(RoleTypeId.FacilityGuard);
                    player.ClearInventory();
                    player.AddItem(ItemType.GunE11SR);
                    player.AddItem(ItemType.GunCOM15);
                    player.AddItem(ItemType.KeycardMTFOperative);
                    player.AddItem(ItemType.ArmorCombat);
                    player.Health = 200;
                    player.CustomInfo = "Глава Службы Безопасности";
                    player.AddAmmo(AmmoType.Nato556, 120);
                    player.AddAmmo(AmmoType.Nato9, 60);
                    break;

                case CustomRole.SecurityOfficer:
                    player.Role.Set(RoleTypeId.FacilityGuard);
                    player.ClearInventory();
                    player.AddItem(ItemType.GunCOM18);
                    player.AddItem(ItemType.KeycardGuard);
                    player.AddItem(ItemType.Radio);
                    player.AddItem(ItemType.ArmorLight);
                    player.Health = 100;
                    player.CustomInfo = "Рядовой Службы Безопасности";
                    player.AddAmmo(AmmoType.Nato9, 60); 
                    break;

                case CustomRole.HeadOfScientists:
                    player.Role.Set(RoleTypeId.Scientist);
                    player.ClearInventory();
                    player.AddItem(ItemType.KeycardO5);
                    player.AddItem(ItemType.Medkit);
                    player.AddItem(ItemType.Radio);
                    player.Health = 120;
                    player.CustomInfo = "Глава Научной Службы";
                    player.AddAmmo(AmmoType.Nato9, 60);
                    break;

                case CustomRole.Lucky:
                    player.Role.Set(RoleTypeId.ClassD);
                    player.ClearInventory();
                    player.AddItem(ItemType.Coin);
                    player.AddItem(ItemType.KeycardJanitor);
                    player.Health = 120;
                    player.CustomInfo = "<color=red>Счастливчик</color>";
                    Map.Broadcast(10, "<b><color=red>На объекте появился Счастливчик! Он может открыть любые двери с шансом 50%.</b></color>");
                    player.ShowHint("<b> Ты стал счастливчиком! Ты можешь открывать любые двери с шансом 50 процентов без карты! Удачной игры!)", 10f);
                    player.Scale = new Vector3(1.1f, 1.1f, 0.2f);
                    break;
            }
        }

        private void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player.CustomInfo == "<color=red>Счастливчик</color>")
            {
                var random = new System.Random();
                if (random.Next(0, 100) < 50) 
                {
                    ev.IsAllowed = true; 
                }
            }
        }

        private IEnumerator<float> ShowHints()
        {
            while (true)
            {
                foreach (var player in Player.List)
                {
                    string roleName = player.CustomInfo != null ? player.CustomInfo : player.Role.Name;
                    Map.ShowHint($"<size=25><align=left><voffset=10><color=orange>Твой Ник👤</color> <color=#FFA07A>|</color>: {player.Nickname}</voffset>\n<color=#98FB98>Твоя Роль🎭</color> <color=#228B22>|</color>: {roleName}\n<color=orange><b>Раунд идёт:</b></color>: {Math.Floor(Round.ElapsedTime.TotalMinutes)} минут(ы)</color></align></size>\n\n<size=15><align=center><voffset=-30em><b><u><color=#CA33FF>M</color><color=#E333FF>H</color><color=#FF33F0>C</color> <color=white>|</color> <color=#FF6800>E</color><color=#FF8700>v</color><color=#FFA200>e</color><color=#FFC500>n</color><color=#FFE400>t</color><color=#FFFF00>s</color></b></size></u></voffset>", 1.0f);
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        private void RenameDefaultRoles()
        {
            foreach (var player in Player.List)
            {
                if (string.IsNullOrEmpty(player.CustomInfo))
                {
                    switch (player.Role.Type)
                    {
                        case RoleTypeId.ClassD:
                            player.CustomInfo = "D-класс";
                            break;
                        case RoleTypeId.Scientist:
                            player.CustomInfo = "Ученый";
                            break;
                        case RoleTypeId.FacilityGuard:
                            player.CustomInfo = "Охранник";
                            break;
                        case RoleTypeId.NtfPrivate:
                            player.CustomInfo = "Рядовой МОГ";
                            break;
                        case RoleTypeId.NtfSergeant:
                            player.CustomInfo = "Сержант МОГ";
                            break;
                        case RoleTypeId.NtfCaptain:
                            player.CustomInfo = "Капитан МОГ";
                            break;
                        case RoleTypeId.ChaosConscript:
                            player.CustomInfo = "Хаос Рекрут";
                            break;
                        case RoleTypeId.ChaosRifleman:
                            player.CustomInfo = "Хаос Снайпер";
                            break;
                        case RoleTypeId.ChaosRepressor:
                            player.CustomInfo = "Хаос Подавитель";
                            break;
                        case RoleTypeId.ChaosMarauder:
                            player.CustomInfo = "Хаос Мародер";
                            break;
                        case RoleTypeId.Scp049:
                            player.CustomInfo = "SCP-049";
                            break;
                        case RoleTypeId.Scp0492:
                            player.CustomInfo = "SCP-049-2";
                            break;
                        case RoleTypeId.Scp096:
                            player.CustomInfo = "SCP-096";
                            break;
                        case RoleTypeId.Scp106:
                            player.CustomInfo = "SCP-106";
                            break;
                        case RoleTypeId.Scp173:
                            player.CustomInfo = "SCP-173";
                            break;
                        case RoleTypeId.Scp939:
                            player.CustomInfo = "SCP-939";
                            break;
                        case RoleTypeId.Scp3114:
                            player.CustomInfo = "SCP-3114";
                            break;
                    }
                }
            }
        }
    }

    public enum CustomRole
    {
        HeadOfSecurity,
        SecurityOfficer,
        HeadOfScientists,
        Lucky,
    }
}
