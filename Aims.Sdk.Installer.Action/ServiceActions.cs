using System;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
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

				if (flagConverFailed || value1 <= 0.0 || value2 <= 0.0)
				{
					MessageBox.Show("Incorrect days count. Please, input positive integer value.", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					session["AIMS_SSL_WARNING_OPTIONS_VALID"] = "0";
				}
				else
				{
					if (value1 < value2)
					{
						session["AIMS_SSL_CERT_FIRST_WARNING"] = warning2;
						session["AIMS_SSL_CERT_SECOND_WARNING"] = warning1;
					}
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

		[CustomAction]
		public static ActionResult RegisterIisModule(Session session)
		{
			try
			{
				//session["DEBUG_CUSTOM_LOG"] += "Begin RegisterModule\n";
				//session["DEBUG_CUSTOM_LOG"] += $"RegisterModule, IISDIR:{session["TARGET_IIS_DIR"]}#";
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().Name + '#';
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().ImpersonationLevel.ToString() + '#';
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().IsSystem.ToString() + '#';
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().AuthenticationType + '#';
				string arguments =
					$"add module /name:{IISAgent.AgentConstants.InstallConstatnts.IisModuleName} /type:\"{IISAgent.AgentConstants.InstallConstatnts.IisModuleType}\"";
				//session["DEBUG_CUSTOM_LOG"] += "Arguments:" + arguments + '#';
				string answer;
				int exitCode;
				if (!IisHelper.TryRunAppcmd(arguments, out answer, out exitCode))
				{
					if (exitCode != 183)
						MessageBox.Show($"RegisterModule, wrong or null answer:\n{answer}\n", "Error",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					else
					{
						RemoveIisModule(session);
						RegisterIisModule(session);
					}
				}
				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"RegisterModule:\n{ex.Message}\n", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				//session["DEBUG_CUSTOM_LOG"] += "RegisterModule, exception: " + ex;
			}
			return ActionResult.Failure;
		}

		[CustomAction]
		public static ActionResult RemoveIisModule(Session session)
		{
			try
			{
				//session["DEBUG_CUSTOM_LOG"] += "Begin RegisterModule\n";
				//session["DEBUG_CUSTOM_LOG"] += $"RegisterModule, IISDIR:{session["TARGET_IIS_DIR"]}#";
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().Name + '#';
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().ImpersonationLevel.ToString() + '#';
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().IsSystem.ToString() + '#';
				//session["DEBUG_CUSTOM_LOG"] += WindowsIdentity.GetCurrent().AuthenticationType + '#';
				string arguments =
					$"delete module /module.name:{IISAgent.AgentConstants.InstallConstatnts.IisModuleName}";
				//session["DEBUG_CUSTOM_LOG"] += "Arguments:" + arguments + '#';
				IisHelper.RunAppcmd(arguments);
				//session["DEBUG_CUSTOM_LOG"] += "End RegisterModule" + '#';
			}
			catch (Exception ex)
			{
				//MessageBox.Show($"RemoveIisModule, wrong or null answer:\n{ex.Message}\n", "Error",
				//	MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return ActionResult.Success;
		}
	}
}