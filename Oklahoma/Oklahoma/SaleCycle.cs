using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Oklahoma
{
    public class SaleCycle
    {
        public const string StateKey = "__salecycle__";

        public static IStateStorage StateStorage = new HttpContextStateStorage();
        public static Func<string> SessionResolver = () => HttpContext.Current.Session.SessionID;

        private readonly IDictionary<string, string> _pageVariables = new Dictionary<string, string>();

        /// <summary>
        /// Page-scoped variables to be included in the request.
        /// </summary>
        public IDictionary<string, string> PageVariables
        {
            get { return _pageVariables; }
        }

        /// <summary>
        /// Adds a SaleCycle page-scoped variable to the request.
        /// </summary>
        /// <param name="name">Name of the variable. <example>e</example></param>
        /// <param name="value">Value of the variable. <example>john@gmail.com</example></param>
        public static void AddPageVariable(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            Current.PageVariables[name] = value;
        }

        #region Page variable names

        /// <summary>
        /// Page variable name which is set with a customer name.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CustomerNameVariable = "n";

        /// <summary>
        /// Page variable name which is set with a customer email.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CustomerEmailVariable = "e";

        /// <summary>
        /// Page variable name which is set with a customer phone number.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CustomerPhoneNumberVariable = "t";

        /// <summary>
        /// Page variable name which is set with a client id. 
        /// Can be overridden by setting this field.
        /// </summary>
        public static string ClientIdVariable = "c";

        /// <summary>
        /// Page variable name which is set with a session id.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string SessionIdVariable = "b";

        /// <summary>
        /// Page variable name which is set with a cart status value.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CartStatusVariable = "s";

        /// <summary>
        /// Page variable name which is set with a currency variable.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CurrencyVariable = "y";

        /// <summary>
        /// Page variable name which is set with a cookie behavior.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CookieBehaviorVariable = "uc";

        /// <summary>
        /// Page variable name which is set with a total value.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string TotalValueVariable = "v2";

        /// <summary>
        /// Page variable name which is set with an item ids.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CartItemIdsVariable = "p";

        /// <summary>
        /// Page variable name which is set with an item names.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CartItemNamesVariable = "i";

        /// <summary>
        /// Page variable name which is set with an item values.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CartItemValuesVariable = "v1";

        /// <summary>
        /// Page variable name which is set with an item quantities.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CartItemQuantitiesVariable = "q1";

        /// <summary>
        /// Page variable name which is set with an item image urls.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CartItemImageUrlsVariable = "u";

        /// <summary>
        /// Page variable name which is set with a custom field #1.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CustomFieldOneVariable = "cu1";

        /// <summary>
        /// Page variable name which is set with a custom field #2.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string CustomFieldTwoVariable = "cu2";

        /// <summary>
        /// Page variable name which is set with a web page name.
        /// Can be overridden by setting this field.
        /// </summary>
        public static string PageNameVariable = "w";

        #endregion

        /// <summary>
        /// Client id supplied by SaleCycle.
        /// This field should be set at the beginning of an application - (for example Application_Start).
        /// This field is mantadory.
        /// </summary>
        public static string ClientId;

        /// <summary>
        /// Currency defined for SaleCycle specified as ISO 4217 code.
        /// For example for UK it's "GBP".
        /// This field should be set at the beginning of an application - (for example Application_Start).
        /// This field is optional.
        /// </summary>
        public static string Currency = "GBP";

        /// <summary>
        /// Allows client to select cookie functionality.
        /// This field should be set at the beginning of an application - (for example Application_Start).
        /// This field is optional.
        /// </summary>
        public static SaleCycleCookieBehavior CookieBehavior;

        /// <summary>
        /// SaleCycle state for the current request.
        /// </summary>
        public static SaleCycle Current
        {
            get
            {
                var current = StateStorage.Get<SaleCycle>(StateKey);
                if (current == null)
                {
                    current = new SaleCycle();                    
                    StateStorage.Set(StateKey, current);
                }
                return current;
            }
        }

        /// <summary>
        /// Renders SaleCycle javascript tag to the page.
        /// Recommended to include this inside the body element at the bottom of the page.
        /// </summary>
        public static IHtmlString Render()
        {
            addSessionId();
            addStaticValuesToPageVariables();
            ensureThatMandatoryFieldsAreSet();
          
            var script = new StringBuilder();
            script.AppendLine(@"<script type=""text/javascript""> "
                            + "var __sc = new Array();");

            foreach(var variable in Current.PageVariables)
            {
                script.AppendLine(getVariableString(variable.Key, variable.Value));
            }

            script.AppendLine("try { var __scS = document.createElement(\"script\"); __scS.type = \"text/javascript\"; "
                            + "__scS.src = \"https://app.salecycle.com/salecycle.js\"; document.getElementsByTagName(\"head\")[0].appendChild(__scS); } catch (e) { } "
                            + "</script>");

            return new HtmlString(script.ToString());
        }

        static void addSessionId()
        {
            if (SessionResolver != null)
                Current.PageVariables.Add(SessionIdVariable, SessionResolver());
        }

        static void ensureThatMandatoryFieldsAreSet()
        {
            if (ClientId == null)
                throw new ArgumentException("SaleCycle requires ClientId to be set");

            if (!Current.PageVariables.ContainsKey(CartStatusVariable))
                throw new ArgumentException("SaleCycle requires cart status to be defined");

            if (CookieBehavior == SaleCycleCookieBehavior.CookiesAreEnabledAndSessionIdIsRequired && !Current.PageVariables.ContainsKey(SessionIdVariable))
                throw new ArgumentException("SaleCycle requires session id to be set when cookie behavior is default");
        }

        static void addStaticValuesToPageVariables()
        {
            Current.PageVariables.Add(ClientIdVariable, ClientId);

            if (Currency != null)
                Current.PageVariables.Add(CurrencyVariable, Currency);

            if (CookieBehavior != SaleCycleCookieBehavior.CookiesAreEnabledAndSessionIdIsRequired)
                Current.PageVariables.Add(CookieBehaviorVariable, ((short) CookieBehavior).ToString());
        }

        static string getVariableString(string variableName, string value)
        {
            return String.Format("__sc[\"{0}\"]=\"{1}\";", variableName, value);
        }

        /// <summary>
        /// Sets customer name.
        /// If you want to specify customer name you have to specify first name - only 
        /// last name and / or title is not enough and customer name is not going to be rendered.
        /// This field is optional.
        /// </summary>
        /// <param name="firstName">First name of a customer</param>
        /// <param name="lastName">Last name of a customer</param>
        /// <param name="title">Title of a customer</param>
        public static void SetCustomerName(string firstName, string lastName, string title)
        {
            if (firstName != null)
                Current.PageVariables.Add(CustomerNameVariable, pipelineItems(new[] { firstName, lastName, title }));
        }

        /// <summary>
        /// Sets customer email address.
        /// This field is optional.
        /// </summary>
        /// <param name="email"></param>
        public static void SetCustomerEmail(string email)
        {
            Current.PageVariables.Add(CustomerEmailVariable, email);
        }

        /// <summary>
        /// Sets customer phone number.
        /// This field is optional.
        /// </summary>
        /// <param name="phoneNumber"></param>
        public static void SetCustomerPhoneNumber(string phoneNumber)
        {
            Current.PageVariables.Add(CustomerPhoneNumberVariable, phoneNumber);
        }

        /// <summary>
        /// Sets cart status.
        /// This field is mandatory.
        /// </summary>
        /// <param name="cartStatus">Status of a cart</param>
        public static void SetCartStatus(SaleCycleCartStatus cartStatus)
        {
            Current.PageVariables.Add(CartStatusVariable, ((short)cartStatus).ToString());
        }

        /// <summary>
        /// Sets total value of all items in the basket. This value should be without delivery charges, but including potential discounts.
        /// This field is mandatory.
        /// </summary>
        /// <param name="totalValue">Total value of a cart</param>
        public static void SetTotalValue(decimal totalValue)
        {
            Current.PageVariables.Add(TotalValueVariable, totalValue.ToString("0.00"));
        }

        /// <summary>
        /// Sets custom field one - it can take many values.
        /// This field is optional.
        /// </summary>
        public static void SetCustomFieldOne(IEnumerable<string> values)
        {
            Current.PageVariables.Add(CustomFieldOneVariable, pipelineItems(values));
        }

        /// <summary>
        /// Sets custom field two - it can take many values.
        /// This field is optional.
        /// </summary>
        public static void SetCustomFieldTwo(IEnumerable<string> values)
        {
            Current.PageVariables.Add(CustomFieldTwoVariable, pipelineItems(values));
        }

        /// <summary>
        /// Sets web page name (tracks page name customer is currently on)
        /// This field is optional.
        /// </summary>
        public static void SetPageName(string pageName)
        {
            Current.PageVariables.Add(PageNameVariable, pageName);
        }

        /// <summary>
        /// Adds item to the cart.
        /// Items are optional - cart can be empty.
        /// </summary>
        /// <param name="id">Item id</param>
        /// <param name="name">Name of an item</param>
        /// <param name="value">Value of an item</param>
        /// <param name="quantity">Quanitity of an item</param>
        /// <param name="imageUrl">Optional url path to item image</param>
        public static void AddCartItem(string id, string name, decimal value, ushort quantity, string imageUrl)
        {
            appendToPageVariable(id ?? String.Empty, CartItemIdsVariable);
            appendToPageVariable(name ?? String.Empty, CartItemNamesVariable);
            appendToPageVariable(value.ToString("0.00"), CartItemValuesVariable);
            appendToPageVariable(quantity.ToString(), CartItemQuantitiesVariable);
            appendToPageVariable(imageUrl, CartItemImageUrlsVariable);
        }

        static void appendToPageVariable(string toAppend, string pageVariable)
        {            
            if (toAppend == null) return;

            var currentVariableValue = Current.PageVariables.ContainsKey(pageVariable) ? Current.PageVariables[pageVariable] : null;
            Current.PageVariables[pageVariable] = appendPipeLimitedString(toAppend, currentVariableValue);
        }

        static string appendPipeLimitedString(string toAppend, string stringToAppendTo)
        {
            return stringToAppendTo == null
                ? toAppend
                : String.Format("{0}|{1}", stringToAppendTo, toAppend);
        }

        static string pipelineItems(IEnumerable<string> items)
        {
            return String.Join("|",
                items.Select(x => x != null ? encode(x) : String.Empty));
        }        

        static string encode(string toEncode)
        {
            return HttpUtility.JavaScriptStringEncode(toEncode.Replace('|', ' '));
        }
    }
}