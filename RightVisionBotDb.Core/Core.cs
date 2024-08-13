using DryIoc;
using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Core.Data;
using RightVisionBotDb.Models;
using RightVisionBotDb.Models.Forms;
using Serilog;

namespace RightVisionBotDb.Core
{
    public class Core
    {
        internal static IConfiguration Configuration { get; set; }
        private Db _db;

        #region Methods

        public void RegisterTypes()
        {
            Log.Logger?.Information("Регистрация типов и сервисов из {project}...", "RightVisionBotDb.Core");

            App.Container.Register<Db>(Reuse.Singleton);

            Log.Logger?.Information("Сборка конфигурации...");
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("config.json", false)
                .Build();

            _db = App.Container.Resolve<Db>();
            _db.OpenDatabaseConnection(Configuration);
        }

        #region Get Methods

        public RvUser GetRvUser(long userId)
            => _db.Context
            .RvUsers
            .First(u => u.UserId == userId);

        public CriticForm? GetCriticForm(long userId)
            => _db.Context
            .CriticForms
            .FirstOrDefault(u => u.UserId == userId);

        public CriticForm? GetAcceptedCriticForm(long userId)
        {
            var criticForm = GetCriticForm(userId);
            if (criticForm == null) return null;

            return criticForm.Status == Enums.FormStatus.Accepted
                ? criticForm
                : null;
        }

        public ParticipantForm? GetParticipantForm(long userId, string rightvision)
            => _db.RightVisions.First(rv => rv.Key == rightvision).Value
            .ParticipantForms
            .FirstOrDefault(p => p.UserId == userId);

        public ParticipantForm? GetAcceptedParticipantForm(long userId, string rightvision)
        {
            var participantForm = GetParticipantForm(userId, rightvision);
            if (participantForm == null) return null;

            return participantForm.Status == Enums.FormStatus.Accepted
                ? participantForm
                : null;
        }

        #endregion

        #region Add Methods

        public async void AddNewRvUser(RvUser rvUser)
        {
            _db.Context.RvUsers.Add(rvUser);
            await _db.Context.SaveChangesAsync();
        }

        public async void AddNewCriticForm(CriticForm criticForm)
        {
            _db.Context.CriticForms.Add(criticForm);
            await _db.Context.SaveChangesAsync();
        }

        public async void AddNewParticipantForm(ParticipantForm participantForm, string rightvision)
        {
            _db.RightVisions[rightvision].ParticipantForms.Add(participantForm);
            await _db.RightVisions[rightvision].SaveChangesAsync();
        }

        #endregion

        #endregion
    }
}
