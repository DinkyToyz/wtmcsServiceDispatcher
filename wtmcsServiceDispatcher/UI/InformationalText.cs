using ColossalFramework.UI;
using ICities;

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
        /// The text field label.
        /// </summary>
        private UILabel textFieldLabel;

        /// <summary>
        /// The text field parent.
        /// </summary>
        private UIComponent textFieldParent;

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

        /// <summary>
        /// Gets a value indicating whether this component is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this component is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled => this.textFieldParent.isEnabled && this.textFieldLabel.isEnabled;

        /// <summary>
        /// Gets a value indicating whether this component is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this component is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible => this.textFieldParent.isVisible && this.textFieldLabel.isVisible;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get => this.textFieldLabel.text;
            set
            {
                this.textFieldLabel.text = value;
            }
        }

        /// <summary>
        /// Disables this component.
        /// </summary>
        public void Disable()
        {
            this.textFieldParent.Disable();
        }

        /// <summary>
        /// Enables this component.
        /// </summary>
        public void Enable()
        {
            this.textFieldParent.Enable();
            this.textField.Disable();
        }

        /// <summary>
        /// Hides this component.
        /// </summary>
        public void Hide()
        {
            this.textFieldParent.Hide();
        }

        /// <summary>
        /// Shows this component.
        /// </summary>
        public void Show()
        {
            this.textFieldParent.Show();
            this.textField.Hide();
        }
    }
}