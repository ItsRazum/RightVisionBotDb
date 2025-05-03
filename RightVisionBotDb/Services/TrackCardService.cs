using EasyForms.Types;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Text;
using RightVisionBotDb.Text.Sections;
using RightVisionBotDb.Types;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Services
{
    public class TrackCardService
    {
        private readonly Bot _bot;

        public TrackCardService(Bot bot)
        {
            _bot = bot;
        }

        private ParticipantTrackCard GetMessagesParticipantTrackCard(Lang lang) => Phrases.Lang[lang].Messages.Participant.TrackCard;

        public async Task HandleTrackAsync(ParticipantForm form, CommandContext c, CancellationToken token)
        {
            var document = c.Message.Document;
            var audio = c.Message.Audio;
            string? fileName = document?.FileName ?? audio?.FileName;

            string? fileId;
            if (document != null && (fileName?.EndsWith(".mp3") == true || fileName?.EndsWith(".wav") == true))
                fileId = document.FileId;
            else if (audio != null && (fileName?.EndsWith(".mp3") == true || fileName?.EndsWith(".wav") == true))
                fileId = audio.FileId;

            else
            {
                await _bot.Client.SendTextMessageAsync(
                    c.Message.Chat,
                    GetMessagesParticipantTrackCard(c.RvUser.Lang).SendTrackInstruction,
                    cancellationToken: token);
                return;
            }

            if (fileId == null)
            {
                await _bot.Client.SendTextMessageAsync(
                    c.Message.Chat,
                    GetMessagesParticipantTrackCard(c.RvUser.Lang).SendTrackInstruction,
                    cancellationToken: token);
                return;
            }

            form.TrackCard.TrackFileId = fileId;
            ((RightVisionDbContext)c.RvContext).Entry(form).State = EntityState.Modified;

            await _bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                GetMessagesParticipantTrackCard(c.RvUser.Lang).SendTrackSuccess,
                cancellationToken: token);
        }

        public async Task HandleTextAsync(ParticipantForm form, CommandContext c, CancellationToken token)
        {
            var doc = c.Message.Document;
            if (doc == null || !doc.FileName?.EndsWith(".txt") == true)
            {
                await _bot.Client.SendTextMessageAsync(
                    c.Message.Chat,
                    GetMessagesParticipantTrackCard(c.RvUser.Lang).SendTextInstruction,
                    cancellationToken: token);
                return;
            }

            form.TrackCard.TextFileId = doc.FileId;
            ((RightVisionDbContext)c.RvContext).Entry(form).State = EntityState.Modified;

            await _bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                GetMessagesParticipantTrackCard(c.RvUser.Lang).SendTextSuccess,
                cancellationToken: token);
        }

        public async Task HandleImageAsync(ParticipantForm form, CommandContext c, CancellationToken token)
        {
            var photos = c.Message.Photo;
            if (photos == null || photos.Length == 0)
            {
                await _bot.Client.SendTextMessageAsync(
                    c.Message.Chat,
                    GetMessagesParticipantTrackCard(c.RvUser.Lang).SendImageInstruction,
                    cancellationToken: token);
                return;
            }

            var largest = photos[^1];
            form.TrackCard.ImageFileId = largest.FileId;
            ((RightVisionDbContext)c.RvContext).Entry(form).State = EntityState.Modified;

            await _bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                GetMessagesParticipantTrackCard(c.RvUser.Lang).SendImageSuccess,
                cancellationToken: token);
        }

        public async Task HandleVisualAsync(ParticipantForm form, CommandContext c, CancellationToken token)
        {
            var doc = c.Message.Document;
            if (doc == null || !doc.FileName?.EndsWith(".mp4") == true)
            {
                await _bot.Client.SendTextMessageAsync(
                    c.Message.Chat,
                    GetMessagesParticipantTrackCard(c.RvUser.Lang).SendVisualInstruction,
                    cancellationToken: token);
                return;
            }

            form.TrackCard.VisualFileId = doc.FileId;
            ((RightVisionDbContext)c.RvContext).Entry(form).State = EntityState.Modified;

            await _bot.Client.SendTextMessageAsync(
                c.Message.Chat,
                GetMessagesParticipantTrackCard(c.RvUser.Lang).SendVisualSuccess,
                cancellationToken: token);
        }

        public string GetStatus(TrackCard card)
        {
            return 
                $"🔊 Track: {(card.TrackFileId != null ? "✅" : "❌")}\n" +
                $"📄 Text: {(card.TextFileId != null ? "✅" : "❌")}\n" +
                $"📷 Image: {(card.ImageFileId != null ? "✅" : "❌")}\n" +
                $"🎥 Visual (Необязательно): {(card.VisualFileId != null ? "✅" : "❌")}\n" +
                $"\n" +
                $"Ты можешь либо кинуть трек, текст и обложку по отдельности, либо сразу отправить визуал, и ничего другого больше отправлять не нужно будет :)\n" +
                $"\n" +
                $"Чтобы сдать материалы - напиши одну из команд ниже и отправь соответствующий файл вместе с ней:\n" +
                $"/track - отправить ремикс. Принимаются форматы .mp3 и .wav\n" +
                $"/image - отправить обложку. Старайся сделать её в формате 1:1, т.е. квадратную. Отправлять в виде фотографии, а не в виде документа!\n" +
                $"/text - отправить текст. Принимается формат .txt.\n" +
                $"/visual - отправить визуал. Отправлять нужно без сжатия (т.е. в виде документа) и в формате .mp4.\n" +
                $"/menu - вернуться в главное меню";
        }
    }
}
