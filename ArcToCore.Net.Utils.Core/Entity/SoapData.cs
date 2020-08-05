namespace ArcToCore.Net.Utils.Core.Entity
{
    #region public

    public class SoapData
    {
        public int Id { get; set; }
        public string RequestXml { get; set; }
        public string ResponseXml { get; set; }
        public string NameSpace { get; set; }
        public string OperationName { get; set; }
    }

    #endregion public
}