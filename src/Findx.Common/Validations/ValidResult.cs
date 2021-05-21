using System.Collections.Generic;

namespace Findx.Validations
{
    public class ValidResult
    {
        public List<ErrorMember> ErrorMembers { get; set; }
        public bool IsVaild { get; set; }
    }
}
