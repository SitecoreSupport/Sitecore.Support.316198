using Sitecore.Forms.Mvc.Interfaces;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Sitecore.Forms.Mvc.Html;
using Sitecore.Diagnostics;
using Sitecore.Support.Forms.Mvc.ViewModels.Fields;

namespace Sitecore.Support.Forms.Mvc.Html
{
  public static class HtmlHelperExtensions
  {
    public static MvcHtmlString Recaptcha([NotNull] this HtmlHelper helper, IViewModel model = null)
    {
      var view = (model ?? helper.ViewData.Model) as RecaptchaField;
      Assert.IsNotNull(view, "view");

      var robotDetection = view.RobotDetection;
      var res = new StringBuilder();
      res.Append(helper.OpenFormField(view, robotDetection == null || !robotDetection.Enabled));
      res.Append(helper.Hidden("Value"));

      if (view.Visible)
      {
        var div = new TagBuilder("div");
        div.AddCssClass("g-recaptcha");
        div.MergeAttribute("data-sitekey", view.SiteKey);
        div.MergeAttribute("data-theme", view.Theme);
        div.MergeAttribute("data-type", view.CaptchaType);

        var script = new TagBuilder("script");

        script.MergeAttribute("src", "https://www.google.com/recaptcha/api.js");

        res.Append(div);
        res.Append(script);
      }

      res.Append(helper.CloseFormField(view));
      return MvcHtmlString.Create(res.ToString());
    }
  }
}