
namespace Enrich.Payment.MxMerchant.Models
{
    public class QueryParameter
    {
        private string name = string.Empty;
        private string value = string.Empty;
        public QueryParameter(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        public string Name
        {
            get { return name; }
        }
        public string Value
        {
            get { return value; }
        }
    }
    public class QueryParameterComparer : IComparer<QueryParameter>
    {
        #region IComparer<QueryParameter> Members
        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return string.Compare(x.Value, y.Value);
            }
            else
            {
                return string.Compare(x.Name, y.Name);
            }
        }
        #endregion
    }
}
