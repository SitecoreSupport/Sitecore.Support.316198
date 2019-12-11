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
  public class RecaptchaField : Sitecore.Forms.Mvc.ViewModels.Fields.RecaptchaField, IConfiguration, IValidatableObject
  {   

    public new IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
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
        // check if the "Value" property is not empty when validation is successfull
        // the validation logic located in the "RecaptchaResponseValidator" attribute under the "Velue" property.
        isSessionSubmitsExceeded = string.IsNullOrEmpty(Value) ?
            this.RobotDetection.IsSessionThresholdExceeded(formId) : false;
      }

      if (this.RobotDetection.Server.Enabled)
      {
        // check if the "Value" property is not empty when validation is successfull
        // the validation logic located in the "RecaptchaResponseValidator" attribute under the "Velue" property.
        isApplicationOverallSubmitsExceeded = string.IsNullOrEmpty(Value) ?
            this.RobotDetection.IsServerThresholdExceeded(formId) : false;
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