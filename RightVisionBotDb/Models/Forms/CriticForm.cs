﻿using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyForms.Types;
using EasyForms.Attributes;
using Serilog;

namespace RightVisionBotDb.Models.Forms
{
    public class CriticForm : Form, IForm
    {

        #region Properties

        [FormField(1)]
        public string Name { get; set; } = "0";
        [FormField(2)]
        public string Link { get; set; } = "0";
        [FormField(4)]
        public string AboutYou { get; set; } = "0";
        [FormField(5)]
        public string WhyYou { get; set; } = "0";

        #region IForm Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; } = 0;
        public string Telegram { get; set; } = "0";
        [FormField(3)]
        public int Rate { get; set; } = 0;
        public Category Category { get; set; } = Category.None;
        public long CuratorId { get; set; } = 0;
        public FormStatus Status { get; set; } = FormStatus.NotFinished;

        #endregion

        #endregion

        #region IForm Events

        public event EventHandler<Category>? FormAccepted;
        public event EventHandler? FormDenied;
        public event EventHandler? FormReset;

        #endregion

        #region IForm Methods

        public void Accept(Category category)
        {
            if (Status == FormStatus.Accepted) return;

            Status = FormStatus.Accepted;
            FormAccepted?.Invoke(this, category);
        }

        public void Deny()
        {
            if (Status == FormStatus.Denied) return;

            Status = FormStatus.Denied;
            FormDenied?.Invoke(this, new EventArgs());
        }

        public void Reset()
        {

            FormReset?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Constructor

        public CriticForm()
        {
        }

        public CriticForm(long userId, string telegram)
        {
            UserId = userId;
            Telegram = "@" + telegram;
        }

        #endregion
    }
}