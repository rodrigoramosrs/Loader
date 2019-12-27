using System;
 
namespace Loader.Helper
{
    public class JSONSerializer
    {
        /// <summary>
        /// Uses reflection to determine the public fields, properties, and methods of an object.
        /// The current values of the fields and properties are used.
        /// This JSON serializer only recognizes simple types.
        /// </summary>
        /// <param name="obj">Object to serialize into JSON text.</param>
        /// <returns>JavaScript Object Notation string representation of the serialized object.</returns>
        /// <example>
        /// using System;
        ///
        /// public partial class _Default : System.Web.UI.Page{
        ///     protected void Page_Load(object sender, EventArgs e){
        ///         CPerson oPerson = new CPerson();
        ///
        ///         string JSONString = JSONSerializer.JSONSerializer.ToJavaScriptObjectNotation(oPerson);
        ///
        ///         ClientScript.RegisterClientScriptBlock(this.GetType(),
        ///             "JSON",
        ///             "var oJSPerson = " + JSONString + ";",
        ///             true);
        ///     }
        /// }
        ///
        /// public class CPerson{
        ///     public string Name = "Harold Green";
        ///     public int Age = 26;
        ///     public string Residence = "Possum Lodge";
        /// }
        /// </example>
        public static string ToJavaScriptObjectNotation(object obj)
        {
            #region "Variables"
            //Whenever a comma is added to the StringBuilder, this will be set to true.
            //If this is equal to true then the last comma will have nothing after it.
            //If this is true, then I will remove the last comma from the StringBuilder.
            bool hasCommaAtEnd = false;

            //This StringBuilder will contain the actual JSON text. Since we don't know how many times we'll be
            // adding to the string, the StringBuilder is a logical choice for performance reasons.
            System.Text.StringBuilder sbJSON = new System.Text.StringBuilder("{");

            //The .NET reflection methods will need to know the type of object that is being serialized.
            Type objType = obj.GetType();
            #endregion

            #region "reflect Fields"
            //Iterate through the public fields.
            //Public fields are variables in the object that act like properties only because they are public.
            //Notice that FieldInfo is used to contain the information reflected from the object.
            foreach (System.Reflection.FieldInfo fieldInfo in
             objType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                //Remember: in JSON the variable and it's value are separated by a (:) colon.
                sbJSON.Append("\"" + fieldInfo.Name + "\" : ");

                GetFieldOrPropertyValue(ref sbJSON, fieldInfo.GetValue(obj));   //Retrieves the currently assigned value of this field in the object.
                sbJSON.Append(", ");

                hasCommaAtEnd = true;   //A comma was added, set to true.
            }
            #endregion

            #region "reflect Properties"
            //Iterate through the public properties.
            //Notice that PropertyInfo is used instead of FieldInfo. Properties, Methods, and Fields all behave differently,
            // therefore they all have different Info classes to describe them. Also, all the Info classes inherit from MemberInfo.
            foreach (System.Reflection.PropertyInfo propertyInfo in
                   objType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                //Skip this property if it doesn't have a GET method. Why bother if the property can't be read from.
                if (!propertyInfo.CanRead)
                    continue;

                //Again, the JSON Name/Value pair is: Name, colon, value, and then comma.
                //It looks like:     "NAME" : "VALUE" ,
                sbJSON.Append("\"" + propertyInfo.Name + "\" : ");

                //Something is different here. If this were a collection, then propertyInfo.GetValue would need to know
                // how deep (which index) in the collection to get the value.
                //However, this is a starter example, so for now we're getting the only and first item regardless of it being
                // a collection or not.
                //ALSO, fields and properties can be treated the same in JavaScript. Therefore, the same
                // "GetFieldOrPropertyValue()" function is used to get and serialize the value.
                GetFieldOrPropertyValue(ref sbJSON, propertyInfo.GetValue(obj, new object[0]));

                sbJSON.Append(", ");

                hasCommaAtEnd = true;   //A comma was added, set to true.
            }
            #endregion

            #region "reflect Methods"
            //Iterate through the public methods.
            //Why bother converting a method to JavaScript? Later on, you may want to add code
            // that actually makes an XMLHTTP call to the server when the method is invoked in JavaScript.
            // You might instantiate the class on the server and execute the method… cool eh?
           /* foreach (System.Reflection.MethodInfo methodInfo in
                objType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                //There are some methods that are "strange". For example, properties are actually methods!
                //The boolean property MethodInfo.isSpecialName is true if this method is anything but
                // a normal function.
                if (methodInfo.DeclaringType == objType && !methodInfo.IsSpecialName)
                {
                    //Again, append the Name/Value pair to the StringBuilder.
                    sbJSON.Append("\"" + methodInfo.Name + "\" : ");

                    //Getting the value for a method is different. We can't simply determine it's current value.
                    //Sometimes methods don't return anything at all, they DO stuff. Therefore, we will
                    // build a JavaScript function as a placeholder for the public method.
                    GetJsonStringForMethodValue(ref sbJSON, methodInfo);

                    sbJSON.Append(", ");

                    hasCommaAtEnd = true;   //A comma was added, set to true.
                }
            }*/
            #endregion

            //If there is a comma at the end of the StringBuilder, then remove it by shortening the length
            // of the StringBuilder by two characters. (the comma and the space after it.)
            if (hasCommaAtEnd)
                sbJSON.Length -= 2;

            sbJSON.Append("}"); //Close up the JSON string.

            return sbJSON.ToString();
        }

        /// <summary>
        /// Creates a placeholder function to act as a method. It will create a JavaScript function with the same name and number of
        /// parameters as the method being reflected.
        /// </summary>
        /// <param name="sbJSON">StringBuilder passed in by reference. No need to copy the whole string to the stack.</param>
        /// <param name="method">The MethodInfo of the method being reflected.</param>
        private static void GetJsonStringForMethodValue(ref System.Text.StringBuilder sbJSON, System.Reflection.MethodInfo method)
        {
            //This will be used to remove the last comma from the list of parameters.
            bool hasCommaAtEnd = false;

            //The JavaScript equivalent of a method is a function.
            sbJSON.Append("function(");

            //Iterate through the parameters so this JavaScript function will take the same name and number
            // of parameters as the method being reflected.
            foreach (System.Reflection.ParameterInfo parameterInfo in method.GetParameters())
            {
                sbJSON.Append(parameterInfo.Name +
                      ", ");
                hasCommaAtEnd = true;   //A comma was added, set to true.
            }

            if (hasCommaAtEnd)
                sbJSON.Length -= 2;

            //For now, the body of this function is a placeholder.
            //It is up to YOU to write the JavaScript to handle this method.
            sbJSON.Append("){ alert ('A method was called.'); }");
        }

        /// <summary>
        /// Determines the current value of a field or property and appends it to the StringBuilder.
        /// This will handle only a handful or data types. I have not written the code to handle
        /// nested objects.
        /// </summary>
        /// <param name="sbJSON">StringBuilder passed in by reference. No need to copy the whole string to the stack.</param>
        /// <param name="value">The actual value of the field or property as an Object.</param>
        private static void GetFieldOrPropertyValue(ref System.Text.StringBuilder sbJSON, object value)
        {
            //First off, determine WHAT type of value we're dealing with.
            Type type = value.GetType();

            //Handle the type variable.
            if (type == typeof(System.DateTime))
            {
                //The JavaScript equivalent of a DateTime is a Date.
                //In JavaScript, a new Date is created by telling it how many milliseconds have passed
                // between now and the beginning of 1970.
                //Ahh yes, 1970: When the high of the 60's finally cleared and life began for some JAVA creator.
                TimeSpan ts = (DateTime)value - DateTime.Parse("1/1/1970");
                sbJSON.Append("new Date(" + ts.TotalMilliseconds.ToString() + ")");
            }
            else if (type == typeof(System.String))
            {
                //The equivalent of a string in JavaScript is a String.
                //Using "New String()" isn't necessary, but I like to be consistent.
                //sbJSON.Append("new String(\"" + EscapeStringForJavaScript((string)value) + "\")");
                sbJSON.Append("\"" + EscapeStringForJavaScript((string)value) + "\"");
            }
            else if (type == typeof(System.Int16) ||
              type == typeof(System.Int32) ||
              type == typeof(System.Int64) ||
              type == typeof(System.Decimal) ||
              type == typeof(System.Double) ||
              type == typeof(System.Single))
            {
                //The JavaScript equivalent for all of .NET's number types is simple Number.
               // sbJSON.Append("new Number(" + value.ToString() + ")");
                sbJSON.Append(value.ToString());
            }
            else if (type.IsArray)
            {
                //If this is an array, then iterate through each item and recursively call this function to determine
                // the type and serialize each item of the array.

                bool hasCommaAtEnd = false;

                //JavaScript arrays can be created and filled all at once by putting all the items between brackets.
                sbJSON.Append("[");

                //this value is an array, but we don't know what type of array. (Int32[], String[], Byte[], etc)
                //Cast it as a generic Array so we can iterate over it's items.
                foreach (object o in (Array)value)
                {
                    //Recursively call this function to determine and serialize each item of the array.
                    GetFieldOrPropertyValue(ref sbJSON, o);
                    sbJSON.Append(", ");

                    hasCommaAtEnd = true;   //A comma was added, set to true.
                }

                //Get rid of that last comma.
                if (hasCommaAtEnd)
                    sbJSON.Length -= 2;

                sbJSON.Append("]"); //Close the array.
            }
            else
            {
                //Any unhandled datatype will simply have it's toString called.
                //It's up to YOU to handle more exotic datatypes such as nested objects and
                // .NET objects that don't have a JavaScript equivalent.
                sbJSON.Append(EscapeStringForJavaScript(value.ToString()));
            }
        }

        /// <summary>
        /// Prepares plaintext to be used as the text in a JavaScript string.
        /// </summary>
        /// <param name="input">String to escape.</param>
        /// <returns>Escaped version of the input string.</returns>
        private static string EscapeStringForJavaScript(string input)
        {
            //input = input.Replace("", @"");
            input = input.Replace("\b", @"\b");
            input = input.Replace("\t", @"\t");
            input = input.Replace("\n", @"\n");
            input = input.Replace("\f", @"\f");
            input = input.Replace("\r", @"\r");
            input = input.Replace("\"", @"""");
                  return input;
        }
    }
}