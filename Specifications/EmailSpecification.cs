using System.Net.Mail;

namespace codetest.Specifications
{
    public class EmailSpecification : CompositeSpecification<string>
    {
        public override bool IsSatisfiedBy(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}