﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;

using GAPIT.MKT.Helpers;
using GAPIT.MKT.Framework.Core.JSON;


namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// Provides an easy way for server code to publish strings into client script 
    /// code. This object basically provides a mechanism for adding key value pairs
    ///  and embedding those values into an object that is hosted on the client.
    /// 
    /// This component supports:&lt;&lt;ul&gt;&gt;
    /// &lt;&lt;li&gt;&gt; Creating individual client side variables
    /// &lt;&lt;li&gt;&gt; Dynamic values that are 'evaluated' in OnPreRender to 
    /// pick up a value
    /// &lt;&lt;li&gt;&gt; Creating properties of ClientIDs for a given container
    /// &lt;&lt;li&gt;&gt; Changing the object values and POSTing them back on 
    /// Postback
    /// &lt;&lt;/ul&gt;&gt;
    /// 
    /// You create a script variables instance and add new keys to it:
    /// &lt;&lt;code lang="C#"&gt;&gt;
    /// ScriptVariables scriptVars = new ScriptVariables(this,"scriptVars");
    /// 
    /// // Simple value
    /// scriptVars.Add("userToken", UserToken);
    /// 
    /// AmazonBook tbook = new AmazonBook();
    /// tbook.Entered = DateTime.Now;
    /// 
    /// // Complex value marshalled
    /// scriptVars.Add("emptyBook", tbook);
    /// 
    /// scriptVars.AddDynamic("author", txtAuthor,"Text");
    /// 
    /// // Cause all client ids to be rendered as scriptVars.formFieldId vars (Id 
    /// postfix)
    /// scriptVars.AddClientIds(Form,true);
    /// &lt;&lt;/code&gt;&gt;
    /// 
    /// In client code you can then access these variables:
    /// &lt;&lt;code lang="JavaScript"&gt;&gt;$(document).ready( function() {
    /// 	alert(scriptVars.book.Author);
    /// 	alert(scriptVars.author);
    /// 	alert( $("#" + scriptVars.txtAmazonUrlId).val() );
    /// });&lt;&lt;/code&gt;&gt;
    /// </summary>
    public class ScriptVariables
    {

        /// <summary>Edit
        /// Internally holds all script variables declared
        /// </summary>
        Dictionary<string, object> ScriptVars = new Dictionary<string, object>();


        /// <summary>
        /// Internally tracked reference to the Page object
        /// </summary>
        Page Page = null;


        /// <summary>
        /// The name of the object generated in client script code
        /// </summary>
        public string ClientObjectName
        {
            get { return _ClientObjectName; }
            set { _ClientObjectName = value; }
        }
        private string _ClientObjectName = "serverVars";

        /// <summary>
        /// Determines whether the output object script is rendered
        /// automatically as part of Page PreRenderComplete. If false
        /// you can manually call the GetClientScript() method to
        /// retrieve the script as a string and embed it yourself.
        /// </summary>        
        public bool AutoRenderClientScript
        {
            get { return _AutoRenderClientScript; }
            set { _AutoRenderClientScript = value; }
        }
        private bool _AutoRenderClientScript = true;


        /// <summary>
        /// Determines how updates to the server from the client are performed.
        /// If enabled changes to the client side properties post back to the
        /// server on a full Postback. 
        /// 
        /// Options allow for none, updating the properties only or updating
        /// only the Items collection (use .add() on the client to add new items)
        /// </summary>
        public AllowUpdateTypes UpdateMode
        {
            get { return _UpdateMode; }
            set { _UpdateMode = value; }
        }
        private AllowUpdateTypes _UpdateMode = AllowUpdateTypes.None;


        /// <summary>
        /// Internal string of the postback value for the field values
        /// if AllowUpdates is true
        /// </summary>
        private string PostBackValue
        {
            get
            {
                if (_PostBackValue == null)
                    _PostBackValue = HttpContext.Current.Request.Form["__" + ClientObjectName];

                return _PostBackValue;
            }
        }
        private string _PostBackValue = null;


        /// <summary>
        /// Internal instance of the Json Serializer used to serialize
        /// the object and deserialize the updateable fields
        /// </summary>
        private JSONSerializer JsonSerializer;


        /// <summary>
        /// Internally tracked prefix code
        /// </summary>
        private StringBuilder sbPrefixScriptCode = new StringBuilder();

        private StringBuilder sbPostFixScriptCode = new StringBuilder();


        /// <summary>
        /// Internal counter for submit script embedded
        /// </summary>
        private int SubmitCounter = 0;

        /// <summary>
        /// Full constructor that receives an instance of any control object
        /// and the client name of the generated script object that contains
        /// the specified properties.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="clientObjectName"></param>
        public ScriptVariables(Control control, string clientObjectName)
        {
            if (control == null)
                // Note: this will fail if called from Page Contstructor
                //       ie. wwScriptVariables scripVars = new wwScriptVariables();
                Page = HttpContext.Current.Handler as Page;
            else
                Page = control.Page;

            if (Page != null)
            {
                // Force RenderClientScript to be called before the page renders
                Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
            }

            if (!string.IsNullOrEmpty(clientObjectName))
                ClientObjectName = clientObjectName;

            // we have to use the West Wind parser since dates use new Date() formatting as embedded JSON 'date string'
            JsonSerializer = new JSONSerializer(SupportedJsonParserTypes.WestWindJsonSerializer);
            JsonSerializer.DateSerializationMode = JsonDateEncodingModes.NewDateExpression;
        }



        /// <summary>
        /// This constructor only takes an instance of a Control. The name of the
        /// client object will be serverVars.
        /// </summary>
        /// <param name="control"></param>
        public ScriptVariables(Control control)
            : this(control, "serverVars")
        {
        }

        /// <summary>
        /// This constructor can only be called AFTER a page instance has been created.
        /// This means OnInit() or later, but not in the constructor of the page.
        /// 
        /// The name of the client object will be serverVars.
        /// </summary>
        public ScriptVariables()
            : this(null, "serverVars")
        {
        }


        /// <summary>
        /// Implemented after Page's OnPreRender() has fired to ensure all
        /// page code has a chance to write script variables.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (AutoRenderClientScript)
                RenderClientScript();
        }

        /// <summary>
        /// Adds a property and value to the client side object to be rendered into 
        /// JavaScript code. VariableName becomes a property on the object and the 
        /// value will be properly converted into JavaScript Compatible text.
        /// <seealso>Class ScriptVariables</seealso>
        /// </summary>
        /// <param name="variableName">
        /// The name of the property created on the client object.
        /// </param>
        /// <param name="value">
        /// The value that is to be assigned. Can be any simple type and most complex 
        /// objects that can be serialized into JSON.
        /// </param>
        /// <example>
        /// &amp;lt;&amp;lt;code 
        /// lang=&amp;quot;C#&amp;quot;&amp;gt;&amp;gt;ScriptVariables scriptVars = new
        ///  ScriptVariables(this,&amp;quot;serverVars&amp;quot;);
        /// 
        /// // Add simple values
        /// scriptVars.Add(&amp;quot;name&amp;quot;,&amp;quot;Rick&amp;quot;);
        /// scriptVars.Add(&amp;quot;pageLoadTime&amp;quot;,DateTime.Now);
        /// 
        /// // Add objects
        /// AmazonBook amazon = new AmazonBook();
        /// bookEntity book = amazon.New();
        /// 
        /// scripVars.Add(&amp;quot;book&amp;quot;,book);
        /// &amp;lt;&amp;lt;/code&amp;gt;&amp;gt;
        /// </example>
        public void Add(string variableName, object value)
        {
            ScriptVars.Add(variableName, value);
        }

        /// <summary>
        /// Adds the dynamic value of a control or any object's property that is picked
        ///  up just before rendering.
        /// 
        /// This allows you to specify a given control or object's value to added to 
        /// the client object with the specified property value set on the JavaScript 
        /// object and have that value be picked up just before rendering. This is 
        /// useful so you can do all client object declarations up front in one place 
        /// regardless of where the values are actually set.
        /// 
        /// Dynamic values are retrieved with Reflection so this method is necessarily 
        /// slower than direct assignment.
        /// <seealso>Class ScriptVariables</seealso>
        /// </summary>
        /// <param name="variableName">
        /// Name of the property created on the client object.
        /// </param>
        /// <param name="control">
        /// Object or Control reference that is to be evaluated. Note this object needs
        ///  to be protected or public in order to be serialized depending on trust 
        /// settings in ASP.NET (medium trust can't look at protected members).
        /// </param>
        /// <param name="property">
        /// The name of the property that is to be evaluated as a string.
        /// </param>
        /// <example>
        /// &amp;lt;&amp;lt;code 
        /// lang=&amp;quot;C#&amp;quot;&amp;gt;&amp;gt;ScriptVariables scriptVars = new
        ///  ScriptVariables(this,&amp;quot;serverVars&amp;quot;);
        /// 
        /// // Add control values
        /// scriptVars.AddDynamic(&amp;quot;name&amp;quot;,txtName,&amp;quot;Text&
        /// amp;quot;);
        /// 
        /// // Add an object's value
        /// scriptVars.AddDynamic(&amp;quot;ItemTotal&amp;quot;,Invoice,&amp;quot;
        /// ItemTotal&amp;quot;)
        /// &amp;lt;&amp;lt;/code&amp;gt;&amp;gt;
        /// </example>
        public void AddDynamicValue(string variableName, object control, string property)
        {
            // Special key syntax: .varName.Property syntax to be picked up by parser
            ScriptVars.Add("." + variableName + "." + property, control);
        }

        /// <summary>
        /// Adds all the client ids for a container as properties of the client object.
        ///  The name of the property is the ID + "Id" Example: txtNameId
        /// 
        /// Note that there's no attempt made to  resolve naming conflicts in different
        ///  naming containers. If there's a naming conflict last one wins.
        /// <seealso>Class ScriptVariables</seealso>
        /// </summary>
        /// <param name="container">
        /// The container from which to retrieve Client IDs. You can use Form or 
        /// this for the top level.
        /// </param>
        /// <param name="recursive">
        /// Determines whether ClientIDs are retrieved recursively by drilling into 
        /// containers. Use with care - large pages with many controls may take a long 
        /// time to find and serialize all control Ids. It's best to focus on the 
        /// controls you are interested and if necesary use multiple AddClientIds() 
        /// calls.
        /// </param>
        public void AddClientIds(Control container, bool recursive)
        {
            foreach (Control control in container.Controls)
            {
                string id = control.ID + "Id";
                if (string.IsNullOrEmpty(id))
                    continue;

                if (!ScriptVars.ContainsKey(id))
                    ScriptVars.Add(id, control.ClientID);
                else
                    ScriptVars[id] = control.ClientID;

                // Drill into the hierarchy
                if (recursive)
                    AddClientIds(control, true);
            }
        }

        /// <summary>
        /// Adds all the client ids for a container as properties of the client object.
        ///  The name of the property is the ID + "Id" Example: txtNameId This version 
        /// only retrieves ids for the specified container level - no hierarchical 
        /// recursion of controls is performed.
        /// <seealso>Class ScriptVariables</seealso>
        /// </summary>
        /// <param name="container">
        /// The container for which to retrieve client IDs.
        /// </param>
        public void AddClientIds(Control container)
        {
            AddClientIds(container, false);
        }

        /// <summary>
        /// Any custom JavaScript code that is to immediately preceed the
        /// client object declaration. This allows setting up of namespaces
        /// if necesary for scoping.
        /// </summary>
        /// <param name="scriptCode"></param>
        public void AddScriptBefore(string scriptCode)
        {
            sbPrefixScriptCode.AppendLine(scriptCode);
        }

        /// <summary>
        /// Any custom JavaScript code that is to immediately follow the
        /// client object declaration. This allows setting up of namespaces
        /// if necesary for scoping.
        /// </summary>
        /// <param name="scriptCode"></param>
        public void AddScriptAfter(string scriptCode)
        {
            sbPostFixScriptCode.AppendLine(scriptCode);
        }

        /// <summary>
        /// Returns a value that has been updated on the client 
        /// 
        /// Note this method will throw if it is not called
        /// during PostBack or if AllowUpdates is false.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TType GetValue<TType>(string key)
        {
            HttpRequest Request = HttpContext.Current.Request;

            if (UpdateMode == AllowUpdateTypes.None || UpdateMode == AllowUpdateTypes.ItemsOnly)
                throw new InvalidOperationException("Can't get values if AllowUpdates is not set to true");

            if (Request.HttpMethod != "POST")
                throw new InvalidOperationException("GetValue can only be called during postback");

            // Get the postback value which is __ + ClientObjectName
            string textValue = PostBackValue;
            if (textValue == null)
                return default(TType);

            // Retrieve individual Url encoded value from the bufer
            textValue = WebUtils.GetUrlEncodedKey(textValue, key);
            if (textValue == null)
                return default(TType);

            // And deserialize as JSON
            object value = JsonSerializer.Deserialize(textValue, typeof(TType));

            return (TType)value;
        }

        /// <summary>
        /// Returns a value from the client Items collection
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TType GetItemValue<TType>(string key)
        {
            HttpRequest Request = HttpContext.Current.Request;

            if (UpdateMode == AllowUpdateTypes.None || UpdateMode == AllowUpdateTypes.PropertiesOnly)
                throw new InvalidOperationException("Can't get values if AllowUpdates is not set to true");

            if (Request.HttpMethod != "POST")
                return default(TType); // throw new InvalidOperationException("GetValue can only be called during postback");

            // Get the postback value which is __ + ClientObjectName
            string textValue = PostBackValue;
            if (string.IsNullOrEmpty(textValue))
                return default(TType);

            // Retrieve individual Url encoded value from the buffer
            textValue = WebUtils.GetUrlEncodedKey(textValue, "_Items");
            if (string.IsNullOrEmpty(textValue))
                return default(TType);

            textValue = WebUtils.GetUrlEncodedKey(textValue, key);
            if (textValue == null)
                return default(TType);

            // And deserialize as JSON
            object value = JsonSerializer.Deserialize(textValue, typeof(TType));

            return (TType)value;
        }


        /// <summary>
        /// Returns the rendered JavaScript for the generated object and name. 
        /// Note this method returns only the generated object, not the 
        /// related code to save updates.
        /// 
        /// You can use this method with MVC Views to embedd generated JavaScript
        /// into the the View page.
        /// <param name="addScriptTags">If provided wraps the script text with script tags</param>
        /// </summary>
        public string GetClientScript(bool addScriptTags)
        {

            if (!AutoRenderClientScript || ScriptVars.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (addScriptTags)
                sb.AppendLine("<script type=\"text/javascript\">");

            // Check for any prefix code and inject it
            if (sbPrefixScriptCode.Length > 0)
                sb.Append(sbPrefixScriptCode.ToString());

            // If the name includes a . assignment is made to an existing
            // object or namespaced reference - don't create var instance.
            if (!ClientObjectName.Contains("."))
                sb.Append("var ");

            sb.AppendLine(ClientObjectName + " = {");


            foreach (KeyValuePair<string, object> entry in ScriptVars)
            {
                if (entry.Key.StartsWith("."))
                {
                    // It's a dynamic key
                    string[] tokens = entry.Key.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    string varName = tokens[0];
                    string property = tokens[1];


                    object propertyValue = null;
                    if (entry.Value != null)
                        propertyValue = ReflectionUtils.GetPropertyEx(entry.Value, property);

                    sb.AppendLine("\t" + varName + ": " + JsonSerializer.Serialize(propertyValue) + ",");
                }
                else
                    sb.AppendLine("\t" + entry.Key + ": " + JsonSerializer.Serialize(entry.Value) + ",");
            }

            if (UpdateMode != AllowUpdateTypes.None)
            {
                sb.AppendLine("\t_Items:{},");
                sb.AppendLine("\tadd: function(key,value) { _Items[key] = value; },");
            }

            // Strip off last comma plus CRLF
            if (sb.Length > 0)
                sb.Length -= 3;

            sb.AppendLine("\r\n};");

            if (sbPostFixScriptCode.Length > 0)
                sb.AppendLine(sbPostFixScriptCode.ToString());

            if (addScriptTags)
                sb.AppendLine("</script>\r\n");



            if (UpdateMode != AllowUpdateTypes.None)
            {
                string clientID = "__" + ClientObjectName;

                string script = string.Format(@"$(document.forms[0]).submit(function() {{ __submitServerVars({0},'__{0}'); }});",
                                         ClientObjectName);

                sb.AppendLine("<script>\r\n" + STR_SUBMITSCRIPT + "\r\n" + script + "\r\n</script>");
                sb.AppendFormat(@"<input type=""hidden"" id=""{0}"" name=""{0}""  value="""" />" + "\r\n", clientID);
            }


            return sb.ToString();
            // Use ClientScriptProxy to be MS Ajax compatible - otherwise use ClientScript
            //scriptProxy.RegisterClientScriptBlock(Page, typeof(ControlResources), "ClientObject_" + ClientObjectName, sb.ToString(), true);
        }

        /// <summary>
        /// Explicitly forces the client script to be rendered into the page.
        /// This code is called automatically by the configured event handler that
        /// is hooked to Page_PreRenderComplete
        /// </summary>
        private void RenderClientScript()
        {
            ClientScriptProxy scriptProxy = ClientScriptProxy.Current;
            string script = GetClientScript(false);

            // TODO: This has to be fixed for ww.jquery.js
            if (UpdateMode != AllowUpdateTypes.None)
            {
                ControlResources.LoadjQuery(Page);
                ControlResources.LoadwwjQuery(Page);

                scriptProxy.RegisterClientScriptBlock(Page, typeof(ControlResources), "submitServerVars", STR_SUBMITSCRIPT, true);
                scriptProxy.RegisterHiddenField(Page, "__" + ClientObjectName, "");


                script += string.Format(@"$(document.forms['{1}']).submit(function() {{ __submitServerVars({0},'__{0}'); }});",
                                         ClientObjectName, Page.Form.ClientID, SubmitCounter++);
            }

            scriptProxy.RegisterClientScriptBlock(Page, typeof(ControlResources),
                                "ClientObject_" + ClientObjectName, script,
                                 true);
        }

        const string STR_SUBMITSCRIPT =
@"
function __submitServerVars(inst,hiddenId)
 {
     var output = '';
     for(var prop in inst)
     {        
        if (prop == '_Items') 
        {
            var out = '';
            for(var p in inst._Items)            
                out += p + '=' + encodeURIComponent(JSON.stringifyWithDates(inst._Items[p]) ) + '&';
            output += '_Items=' + encodeURIComponent(out) + '&';
        } else
        output += prop + '=' + encodeURIComponent(JSON.stringifyWithDates(inst[prop])) + '&';
     }  
     $('#' + hiddenId).val(output);
 };
";

    }

    public enum AllowUpdateTypes
    {
        None,
        ItemsOnly,
        PropertiesOnly,
        All
    }
}
