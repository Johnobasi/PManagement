using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace PermissionManagement.Utility
{
       public class RegularExpressions
    {


        private Dictionary<string, Regex> expressions;

        public RegularExpressions()
        {
            LoadExpressions();

        }

        public Regex GetExpression(string expressionName)
        {
            return expressions[expressionName];
        }

        public bool IsMatch(string expressionName, string input)
        {
            Regex expression = expressions[expressionName];

            return expression.IsMatch(input);
        }

        public string Clean(string expressionName, string input)
        {
            Regex expression = expressions[expressionName];

            return expression.Replace(input, "");
        }


        protected void LoadExpressions()
        {
            expressions = new Dictionary<string, Regex>();

            //expressions.Add("IsEmail", New Regex("^[a-z0-9]+([-+\.]*[a-z0-9]+)*@[a-z0-9]+([-\.][a-z0-9]+)*$", RegexOptions.IgnoreCase Or RegexOptions.Compiled Or RegexOptions.Multiline))
            expressions.Add("IsEmail", new Regex("^[a-zA-Z][\\w\\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\\w\\.-]*[a-zA-Z0-9]\\.[a-zA-Z][a-zA-Z\\.]*[a-zA-Z]$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline));
            expressions.Add("IsUrl", new Regex("^https?://(?:[^./\\s'\"<)\\]]+\\.)+[^./\\s'\"<\")\\]]+(?:/[^'\"<]*)*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline));
            expressions.Add("Username", new Regex("[^a-z0-9]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline));

        }
    }

}
