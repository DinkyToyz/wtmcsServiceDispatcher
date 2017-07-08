using System;
using ColossalFramework.UI;
using ICities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// An informational text label.
    /// </summary>
    public class InformationalText
    {
        /// <summary>
        /// The text field.
        /// </summary>
        private UITextField textField;

        /// <summary>
        /// The text field parent.
        /// </summary>
        private UIComponent textFieldParent;

        /// <summary>
        /// The text field label.
        /// </summary>
        private UILabel textFieldLabel;

        public bool IsVisible => this.textFieldParent.isVisible && this.textFieldLabel.isVisible;

        public bool IsEnabled => this.textFieldParent.isEnabled && this.textFieldLabel.isEnabled;

        public string Text
        {
            get => this.textFieldLabel.text;
            set
            {
                this.textFieldLabel.text = value;
            }
        }

        public void Hide()
        {
            this.textFieldParent.Hide();
        }

        public void Show()
        {
            this.textFieldParent.Show();
            this.textField.Hide();
        }

        public void Enable()
        {
            this.textFieldParent.Enable();
            this.textField.Disable();
        }

        public void Disable()
        {
            this.textFieldParent.Disable();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationalText"/> class.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="labelText">The label text.</param>
        /// <param name="informationalText">The informational text.</param>
        public InformationalText(UIHelperBase helper, string labelText, string informationalText)
        {
            textField = (UITextField)helper.AddTextfield(
                labelText,
                informationalText,
                value => { });

            textFieldParent = textField.parent;
            textField.Hide();
            textField.Disable();
            //// textFieldParent.RemoveUIComponent(textField);

            textFieldLabel = (UILabel)textFieldParent.AddUIComponent(typeof(UILabel));
            textFieldLabel.text = informationalText;
        }
    }
}
