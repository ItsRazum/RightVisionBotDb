﻿using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Extensions.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Lang.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Types;
using System.Globalization;
using System.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Helpers
{
    public static class ProfileHelper
    {
        #region Methods

        #region Public methods

        public static async Task<(string content, InlineKeyboardMarkup? keyboard)> Profile(
            RvUser targetRvUser, 
            RvUser rvUser, 
            ChatType chatType, 
            string rightvision,
            CancellationToken token = default)
        {
            var lang = rvUser.Lang;
            var phrases = Language.Phrases[lang];
            Category? participationCategory;
            ParticipantForm? participantForm;
            CriticForm? criticForm;

            var sb = new StringBuilder(targetRvUser == rvUser
                ? phrases.Profile.Headers.Private
                : string.Format(phrases.Profile.Headers.Global, targetRvUser.Name))
                .AppendLine()
                .AppendLine("———");

            sb.AppendLine( //Статус
                "- " + phrases.Profile.Properties.Status
                + Language.GetUserStatusString(targetRvUser.Status, lang));

            if (targetRvUser.Role != Role.None)
                sb.AppendLine( //Должность
                    "- " + phrases.Profile.Properties.Role
                    + Language.GetUserRoleString(targetRvUser.Role, lang) + "\n");

            using (var db = DatabaseHelper.GetRightVisionContext(rightvision))
            {
                participantForm = await db.ParticipantForms.FirstOrDefaultAsync(m => m.UserId == targetRvUser.UserId && m.Status == FormStatus.Accepted, token);

                if (participantForm != null)
                {
                    if (participantForm.Country != null)
                        sb.AppendLine( //Страна (легаси)
                            "- " + phrases.Profile.Properties.Country
                            + participantForm.Country);

                    if (participantForm.City != null)
                        sb.AppendLine( //Город (легаси)
                            "- " + phrases.Profile.Properties.City
                            + participantForm.City);

                    sb.AppendLine( //Трек
                        "- " + phrases.Profile.Properties.Track +
                        (db.Status == RightVisionStatus.Relevant
                        ? phrases.Profile.Track.Hidden
                        : participantForm.Track
                        )
                    );
                    participationCategory = participantForm?.Category;
                }
            }

            if (chatType == ChatType.Private)
                sb.AppendLine( //Подписка на новости
                    "- " + phrases.Profile.Properties.Sending
                    + (targetRvUser.Has(Permission.Sending)
                        ? phrases.Profile.Sending.Active
                        : phrases.Profile.Sending.Inactive));

            using (var db = DatabaseHelper.GetApplicationDbContext())
            {
                criticForm = await db.CriticForms.FirstOrDefaultAsync(c => c.UserId == targetRvUser.UserId && c.Status == FormStatus.Accepted, token);
                if (criticForm != null)
                    sb.AppendLine( //Категория оценивания
                        phrases.Profile.Properties.CategoryCritic
                        + Language.GetCategoryString(criticForm.Category));
            }

            if(participantForm != null)
                sb.AppendLine( //Категория участия
                    phrases.Profile.Properties.CategoryParticipant
                    + Language.GetCategoryString(participantForm.Category));

            if (chatType == ChatType.Private)
            {
                sb
                    .AppendLine(phrases.Profile.Properties.CandidacyStatus) //Статус кандидатур                    
                    .AppendLine("- " + phrases.Profile.Forms.Properties.Critic + GetCandidateStatus(rvUser.Lang, criticForm))
                    .AppendLine("- " + phrases.Profile.Forms.Properties.Participant + GetCandidateStatus(rvUser.Lang, participantForm));
            }

            sb
                .AppendLine()
                .AppendLine(phrases.Profile.Rewards.Header); //Награды
            if (rvUser.Rewards.Count > 0)
                foreach (var reward in rvUser.Rewards)
                    sb.AppendLine($"- {rvUser.Rewards.IndexOf(reward)}: [{reward.Icon}] {reward.Description}");

            else
                sb.AppendLine("- " + phrases.Profile.Rewards.NoRewards);

            return (sb.ToString(), await KeyboardsHelper.Profile(targetRvUser, chatType, rightvision, rvUser.Lang));
        }

        public static (string content, InlineKeyboardMarkup? keyboard) RvUserPermissions(CallbackContext c, RvUser targetRvUser, bool minimize)
        {
            var rvUser = c.RvUser;
            var lang = rvUser.Lang;

            StringBuilder sb = new(
                c.RvUser == targetRvUser
                ? Language.Phrases[lang].Profile.Permissions.Header
                : string.Format(Language.Phrases[lang].Profile.Permissions.HeaderGlobal, targetRvUser.Name));

            IEnumerable<Permission> permissions;
            UserPermissions layout = new(PermissionsHelper.Layouts[rvUser.Status] + PermissionsHelper.Layouts[rvUser.Role]);

            sb.AppendLine();
            if (minimize)
            {
                var standartCount = 10;
                permissions = rvUser.UserPermissions.Take(standartCount);

                foreach (var permission in permissions)
                    sb.AppendLine("• " + permission);

                if (rvUser.UserPermissions.Count >= standartCount)
                    sb.AppendLine($"... ({Language.Phrases[lang].Profile.Permissions.HowMuchLeft} {rvUser.UserPermissions.Count})");
            }
            else
            {
                permissions = rvUser.UserPermissions;

                foreach (var permission in permissions)
                    sb.AppendLine("• " + permission);
            }


            var addedList = AddedPermissionsList(layout, rvUser.UserPermissions);
            if (addedList.Count > 0)
            {
                sb
                    .AppendLine()
                    .AppendLine(Language.Phrases[lang].Profile.Permissions.AddedList);

                foreach (var permission in addedList)
                    sb.AppendLine("+ " + permission);
            }

            var blockedList = BlockedPermissionsList(layout, rvUser.UserPermissions);
            if (blockedList.Count > 0)
            {
                sb
                    .AppendLine()
                    .AppendLine(Language.Phrases[lang].Profile.Permissions.BlockedList);

                foreach (var permission in blockedList)
                    sb.AppendLine("- " + permission);
            }

            return (sb.ToString(), KeyboardsHelper.PermissionsList(targetRvUser, minimize, targetRvUser.UserPermissions.Count > 10, c.RvUser.Lang));
        }

        public static (string content, InlineKeyboardMarkup? keyboard) RvUserPunishments(CallbackContext c, RvUser targetRvUser, bool showBans, bool showMutes)
        {
            var sb = new StringBuilder();
            var cultureInfo = new CultureInfo("ru-RU");
            var filteredPunishments = targetRvUser.Punishments
                .Where(p => (showBans || p.Type != PunishmentType.Ban) &&
                             (showMutes || p.Type != PunishmentType.Mute))
                .OrderByDescending(p => p.StartDateTime);


            foreach (var punishment in filteredPunishments)
            {
                var punishmentType = punishment.Type switch
                {
                    PunishmentType.Ban => Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.Ban,
                    PunishmentType.Mute => Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.Mute,
                    _ => string.Empty
                };

                var group = punishment.GroupId switch
                {
                    Bot.CriticChatId => Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.InCritics,
                    Bot.ParticipantChatId => Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.InParticipants,
                    _ => string.Empty
                };

                sb.AppendLine($"{punishmentType} {group}{punishment.StartDateTime.ToString("g", new CultureInfo("ru-RU"))}");
                sb.AppendLine($"- {Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.Reason}: {punishment.Reason ?? Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.NoReason}");

                string? timeLeft = null;
                var punishmentEndDateTime = punishment.EndDateTime.ToString("d", cultureInfo);
                if (DateTime.Now < punishment.EndDateTime)
                {
                    var targetValue = 0;
                    ITimeFormat format;
                    var timeSpan = punishment.EndDateTime - DateTime.Now;
                    if (timeSpan.Days > 0)
                    {
                        targetValue = timeSpan.Days;
                        format = Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.TimeLeftFormat.Days;
                    }
                    else if (timeSpan.Hours > 0)
                    {
                        targetValue = timeSpan.Hours;
                        format = Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.TimeLeftFormat.Hours;
                    }
                    else if (timeSpan.Minutes > 0)
                    {
                        targetValue = timeSpan.Minutes;
                        format = Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.TimeLeftFormat.Minutes;
                    }
                    else throw new InvalidOperationException(nameof(timeSpan));

                    string formattedResult = targetValue.ToString().Last() switch 
                    {
                        '1' => format.Singular,
                        '2' or '3' or '4' => format.Genitive,
                        _ => format.Plural
                    };

                    timeLeft = $"{targetValue} {formattedResult}";
                }

                sb.AppendLine($"- {Language.Phrases[c.RvUser.Lang].Profile.Punishments.Punishment.DateTo}: {punishmentEndDateTime}");
                sb.AppendLine();
            }
            return (sb.ToString(), KeyboardsHelper.PunishmentsList(targetRvUser, showBans, showMutes, c.RvUser.Lang));
        }

        #endregion

        #region Private methods

        private static string GetCandidateStatus(Enums.Lang lang, IForm? form = null)
        {
            try
            {
                if (form != null)
                    return form.Status.ToString(lang);
                else
                    return Language.Phrases[lang].Profile.Forms.Status.Allowed;
            }
            catch
            {
                return Language.Phrases[lang].Profile.Forms.Status.CouldNotGet;
            }
        }

        private static List<Permission> AddedPermissionsList(UserPermissions layout, UserPermissions userPermissions)
        {
            var addedList = new List<Permission>();
            foreach (var permission in userPermissions.Where(permission => !layout.Contains(permission)))
                addedList.Add(permission);

            return addedList;
        }

        private static List<Permission> BlockedPermissionsList(UserPermissions layout, UserPermissions userPermissions)
        {
            var blockedList = new List<Permission>();
            foreach (var permission in layout.Collection.Where(permission => !userPermissions.Contains(permission)))
                blockedList.Add(permission);

            return blockedList;
        }

        #endregion

        #endregion
    }
}
