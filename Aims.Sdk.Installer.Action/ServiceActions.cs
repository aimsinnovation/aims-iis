using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Windows.Forms;
using WixToolset.Dtf.WindowsInstaller;
using Env = System.Environment;

namespace Aims.Sdk.Installer.Actions
{
    public class ServiceActions
    {
        [CustomAction]
        public static ActionResult ValidateServiceAccount(Session session)
        {
            session.Log("Begin ValidateServiceAccount");

            try
            {
                string account = session["AIMS_SERVICE_USER"];
                if (!account.Contains(@"\"))
                {
                    session["AIMS_SERVICE_USER"] = account = @".\" + account;
                }
                string[] parts = account.Split('\\');
                if (parts.Length > 2)
                {
                    session["AIMS_SERVICE_ACCOUNT_VALID"] = "0";
                    session.Log("End ValidateServiceAccount");
                    return ActionResult.Success;
                }
                using (var pc = parts[0] == "."
                    || parts[0].ToUpperInvariant() == Env.MachineName
                    ? new PrincipalContext(ContextType.Machine)
                    : new PrincipalContext(ContextType.Domain, parts[0]))
                {
                    if (pc.ValidateCredentials(parts.Last(), session["AIMS_SERVICE_PASSWORD"]))
                    {
                        session["AIMS_SERVICE_ACCOUNT_VALID"] = "1";
                    }
                    else
                    {
                        session["AIMS_SERVICE_ACCOUNT_VALID"] = "0";
                        MessageBox.Show("Incorrect username or password.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            session.Log("End ValidateServiceAccount");
            return ActionResult.Success;
        }

	    [CustomAction]
	    public static ActionResult ValidateSslCertTimeOptions(Session session)
	    {
		    session.Log("Begin ValidateSslCertTimeOptions");

		    try
		    {
			    string warning1 = session["AIMS_SSL_CERT_FIRST_WARNING"];
			    string warning2 = session["AIMS_SSL_CERT_SECOND_WARNING"];
				double value1;
			    double value2;
			    bool flagConverFailed = !Double.TryParse(warning1, out value1);
			    flagConverFailed = !Double.TryParse(warning2, out value2) || flagConverFailed;

				if (flagConverFailed || value1 <= 0.0 || value2 <= 0.0 || value1 <= value2)
				{
					MessageBox.Show("Incorrect months count.", "Error",
					    MessageBoxButtons.OK, MessageBoxIcon.Error);
					session["AIMS_SSL_WARNING_OPTIONS_VALID"] = "0";
				}
			    else
			    {
					session["AIMS_SSL_WARNING_OPTIONS_VALID"] = "1";
				    session.Log("End ValidateSslCertTimeOptions");
				    return ActionResult.Success;
				}
		    }
		    catch (Exception ex)
		    {
			    MessageBox.Show(ex.Message, "Error",
				    MessageBoxButtons.OK, MessageBoxIcon.Error);
		    }

		    session.Log("End ValidateServiceAccount");
		    return ActionResult.Success;
	    }

	}
}