using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Extensions
{
    public static class Extensions
    {
        public static HtmlString ShowLabel(this HtmlHelper htmlHelper, string label)
        {
            if (label != null)
            {
                var result = string.Join(
                    "<br/>",
                    label
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .Select(x => HttpUtility.HtmlEncode(x))
                );
                return MvcHtmlString.Create(result);
            }
            return null;
        }

        public static ViewResult WithMessage(this ViewResult viewResult, Controller controller, string message, MessageType messageType = MessageType.Success)
        {
            controller.ShowMessage(message, messageType);
            return viewResult;
        }

        public static RedirectToRouteResult WithMessage(this RedirectToRouteResult viewResult, Controller controller, string message, MessageType messageType = MessageType.Success)
        {
            controller.ShowMessage(message, messageType);
            return viewResult;
        }


        private static void ShowMessage(this Controller controller, string message, MessageType messageType, bool showAfterRedirect = true)
        {
            var messageTypeKey = messageType.ToString();
            if (showAfterRedirect)
            {
                controller.TempData[messageTypeKey] = message;
            }
            else
            {
                controller.ViewData[messageTypeKey] = message;
            }
        }

        /// <summary>
        /// Render all messages that have been set during execution of the controller action.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static HtmlString RenderMessages(this HtmlHelper htmlHelper, MessageDialogType type = MessageDialogType.AdminLTE)
        {
            var messages = String.Empty;
            foreach (var messageType in Enum.GetNames(typeof(MessageType)))
            {
                var message = htmlHelper.ViewContext.ViewData.ContainsKey(messageType)
                                ? htmlHelper.ViewContext.ViewData[messageType]
                                : htmlHelper.ViewContext.TempData.ContainsKey(messageType)
                                    ? htmlHelper.ViewContext.TempData[messageType]
                                    : null;
                if (message != null)
                {
                    var lowermessage = messageType.ToLowerInvariant();

                    MessageType msgType = (MessageType)Enum.Parse(typeof(MessageType), messageType, true);

                    var messageTitle = msgType.GetDisplayValue();

                    var messageBoxBuilder = new TagBuilder("div");

                    if (type == MessageDialogType.BootStrap)
                    {
                        messageBoxBuilder.AddCssClass(string.Format("alert alert-{0} fade in", lowermessage));
                        messageBoxBuilder.InnerHtml = string.Format("<button class='close' data-dismiss='alert' aria-label='close'>&times;</button><strong>{0}</strong> {1}", messageTitle, message.ToString());
                    }
                    else
                    {
                        messageBoxBuilder.AddCssClass(string.Format("alert alert-{0} fade in", lowermessage));
                        string msgicon = "";
                        switch (lowermessage)
                        {
                            case "danger":
                                msgicon = "fa-ban";
                                break;
                            case "info":
                                msgicon = "fa-info";
                                break;
                            case "warning":
                                msgicon = "fa-warning";
                                break;
                            case "success":
                                msgicon = "fa-check";
                                break;
                            default:
                                break;
                        }
                        messageBoxBuilder.InnerHtml =
                            string.Format("<button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>&times;</button>" +
                                          "<h4><i class='icon fa {0}'></i> {1}</h4>{2}", msgicon, messageTitle, message.ToString());
                    }
                    messages += messageBoxBuilder.ToString();
                }
            }
            return MvcHtmlString.Create(messages);
        }

        public static MvcHtmlString PartialFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            object model = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Model;
            var viewData = new ViewDataDictionary(helper.ViewData)
            {
                TemplateInfo = new System.Web.Mvc.TemplateInfo
                {
                    HtmlFieldPrefix = name
                }
            };

            return helper.Partial(partialViewName, model, viewData);
        }

        public static bool IsPasswordValid(this string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 6)
            {
                return false;
            }

            Match match = Regex.Match(str, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,15}$");
            if (!match.Success)
            {
                return false;
            }
            return true;
        }
        public static bool IsPhoneValid(this string str)
        {
            if (string.IsNullOrEmpty(str) || str.IsNumeric() == false || str.Trim().Substring(0, 1) == "0" || str.Trim().Length != 10)
            {
                return false;
            }

            return true;
        }
        public static bool IsNumeric(this string str)
        {
            long n;
            bool isNumeric = long.TryParse(str, out n);

            return isNumeric;
        }
        public static bool IsValidEmail(this string str)
        {
            try
            {
                var address = new MailAddress(str);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool In<T>(this T source, params T[] list)
        {
            if (null == source) throw new ArgumentNullException("source");
            return list.Contains(source);
        }
        public static string TruncateString(this string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }
        public static string TruncateStringWithDots(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return "";

            return str.Length < maxLength ? str : str.Substring(0, Math.Min(str.Length, maxLength)) + "...";
        }
        public static string FirstCharUpper(this string str)
        {
            return string.IsNullOrEmpty(str) ? "" : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }
        public static IEnumerable Errors(this ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                return modelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors
                                    .Select(e => e.ErrorMessage).ToArray())
                                    .Where(m => m.Value.Any());
            }

            return null;
        }
        public static string TimePassed(this DateTime inputDateTime)
        {
            var ts = new TimeSpan(DateTime.Now.Ticks - inputDateTime.Ticks);
            var delta = Math.Abs(ts.TotalSeconds);

            const int second = 1;
            const int minute = 60 * second;
            const int hour = 60 * minute;
            const int day = 24 * hour;
            const int month = 30 * day;

            if (delta < 0)
            {
                return "henüz değil";
            }
            if (delta < 1 * minute)
            {
                return Math.Abs(ts.Seconds) == 1 ? "1 saniye önce" : Math.Abs(ts.Seconds) + " saniye önce";
            }
            if (delta < 2 * minute)
            {
                return "1 dakika önce";
            }
            if (delta < 45 * minute)
            {
                return Math.Abs(ts.Minutes) + " dakika önce";
            }
            if (delta < 90 * minute)
            {
                return "1 saat önce";
            }
            if (delta < 24 * hour)
            {
                return Math.Abs(ts.Hours) + " saat önce";
            }
            if (delta < 48 * hour)
            {
                return "1 gün önce";
            }
            if (delta < 30 * day)
            {
                return Math.Abs(ts.Days) + " gün önce";
            }
            if (delta < 12 * month)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return Math.Abs(months) <= 1 ? "1 ay önce" : Math.Abs(months) + " ay önce";
            }
            else
            {
                var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return Math.Abs(years) <= 1 ? "1 yıl önce" : Math.Abs(years) + " yıl önce";
            }
        }

        public static string ToSlug(this string text)
        {
            var sb = new StringBuilder();
            var lastWasInvalid = false;

            foreach (var c in text.Trim().ToLower())
            {
                if (Char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    lastWasInvalid = false;
                }
                else
                {
                    if (!lastWasInvalid)
                        sb.Append("-");
                    lastWasInvalid = true;
                }
            }

            var slug = sb.ToString().ToLowerInvariant().Trim();

            while (slug[slug.Length - 1] == '-')
            {
                slug = slug.Substring(0, slug.Length - 1);
            }

            return slug;
        }

        // Convert the string to Pascal case.
        public static string ToPascalCase(this string the_string)
        {
            // If there are 0 or 1 characters, just return the string.
            if (the_string == null) return the_string;
            if (the_string.Length < 2) return the_string.ToUpper();

            // Split the string into words.
            string[] words = the_string.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                var englishWord = String.Join("", word.Normalize(NormalizationForm.FormD)
                    .Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

                englishWord = englishWord.Replace('ı', 'i');

                //remove all non aphanumeric chars
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                englishWord = rgx.Replace(englishWord, "");

                result +=
                    englishWord.Substring(0, 1).ToUpper(new CultureInfo("en-US", false)) +
                    englishWord.Substring(1);
            }

            return result;
        }


        public static bool IsHexaDecimal(this string kod)
        {
            if (string.IsNullOrEmpty(kod))
                return false;

            var allowChars = new HashSet<char>("0123456789ABCDEF");
            return kod.All(allowChars.Contains) && kod.Length == 32;
        }
    }

    public static class EnumerableExtensions
    {
        public static T GetNext<T>(this IEnumerable<T> list, T current)
        {
            try
            {
                return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetPrevious<T>(this IEnumerable<T> list, T current)
        {
            try
            {
                return list.TakeWhile(x => !x.Equals(current)).Last();
            }
            catch
            {
                return default(T);
            }
        }

        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int groups)
        {
            return source.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / groups).Select(x => x.Select(v => v.Value).ToList()).ToList();
        }
    }

    public static class ExpressionBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
                                               Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters
                           .Select((f, i) => new { f, s = second.Parameters[i] })
                           .ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from 
            // the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            // apply composition of lambda expression bodies to parameters from 
            // the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        public class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> map;

            public ParameterRebinder(
                Dictionary<ParameterExpression,
                ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(
                Dictionary<ParameterExpression,
                ParameterExpression> map,
                Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;
                if (map.TryGetValue(p, out replacement))
                {
                    p = replacement;
                }
                return base.VisitParameter(p);
            }
        }
    }
}
