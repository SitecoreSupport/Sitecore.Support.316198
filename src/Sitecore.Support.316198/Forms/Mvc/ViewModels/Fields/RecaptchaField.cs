using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Data;
using Sitecore.Form.Web.UI.Controls;
using Sitecore.Forms.Mvc.Attributes;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Forms.Mvc.Validators;
using Sitecore.Forms.Mvc.ViewModels.Fields;
using Sitecore.WFFM.Abstractions.Actions;
using Sitecore.WFFM.Abstractions.Dependencies;
using Sitecore.WFFM.Abstractions.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Sitecore.Support.Forms.Mvc.ViewModels.Fields
{
  public class RecaptchaField : SingleLineTextField, IConfiguration, IValidatableObject
  {
    private readonly IAnalyticsTracker analyticsTracker;

    public RecaptchaField() : this(DependenciesManager.AnalyticsTracker)
    {
    }

    public RecaptchaField(IAnalyticsTracker analyticsTracker)
    {
      this.analyticsTracker = analyticsTracker;

      this.Theme = "light";
      this.CaptchaType = "image";
    }

    [NotNull]
    public string Theme { get; set; }

    [NotNull]
    public string CaptchaType { get; set; }

    [NotNull]
    public string SiteKey { get; set; }

    [NotNull]
    public string SecretKey { get; set; }

    [CanBeNull]
    [TypeConverter(typeof(ProtectionSchemaAdapter))]
    public virtual ProtectionSchema RobotDetection { get; set; }

    public virtual bool IsRobot
    {
      get
      {
        return this.analyticsTracker.IsRobot;
      }
    }

    [CanBeNull]
    [RequestFormValue("g-recaptcha-response")]
    [RecaptchaResponseValidator(ParameterName = "RecaptchaValidatorError")]
    public override string Value { get; set; }

    public override ControlResult GetResult()
    {
      return new ControlResult(this.FieldItemId, this.Title, this.Value, null, true);
    }

    public override void Initialize()
    {
      this.SiteKey = this.GetAppSetting("RecaptchaPublicKey") ?? this.GetSitecoreSetting("WFM.RecaptchaSiteKey", null);
      this.SecretKey = this.GetAppSetting("RecaptchaPrivateKey") ?? this.GetSitecoreSetting("WFM.RecaptchaSecretKey", null);
      this.Visible = !(this.RobotDetection != null && this.RobotDetection.Enabled);
    }

    public override void SetValueFromQuery(string valueFromQuery)
    {
    }

    public virtual string GetAppSetting(string key)
    {
      Assert.ArgumentNotNullOrEmpty(key, "key");
      return ConfigurationManager.AppSettings[key];
    }

    public virtual string GetSitecoreSetting(string key, string defaultValue)
    {
      Assert.ArgumentNotNullOrEmpty(key, "key");
      return Settings.GetSetting(key, defaultValue);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (this.Visible || this.RobotDetection == null || !this.RobotDetection.Enabled)
      {
        return new[]
        {
                    ValidationResult.Success
                };
      }

      var formId = new ID(this.FormId);

      if (this.RobotDetection != null && this.RobotDetection.Session.Enabled)
      {
        this.RobotDetection.AddSubmitToSession(formId);
      }

      if (this.RobotDetection != null && this.RobotDetection.Server.Enabled)
      {
        this.RobotDetection.AddSubmitToServer(formId);
      }

      bool isRobot = this.IsRobot;
      bool isSessionSubmitsExceeded = false;
      bool isApplicationOverallSubmitsExceeded = false;

      if (this.RobotDetection.Session.Enabled)
      {
        isSessionSubmitsExceeded = this.RobotDetection.IsSessionThresholdExceeded(formId);
      }

      if (this.RobotDetection.Server.Enabled)
      {
        isApplicationOverallSubmitsExceeded = this.RobotDetection.IsServerThresholdExceeded(formId);
      }

      if (isRobot || isSessionSubmitsExceeded || isApplicationOverallSubmitsExceeded)
      {
        this.Visible = true;

        return new[]
        {
          new ValidationResult("You've been treated as a robot. Please enter the captcha to proceed")
        };
      }

      return new[]
      {
        ValidationResult.Success
      };
    }
  }
}