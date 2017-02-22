using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WixToolset.Dtf.WindowsInstaller;
using Env = System.Environment;

namespace Aims.Sdk.Installer.Actions
{
    public class ParameterActions
    {
        [CustomAction]
        public static ActionResult ValidatePaths(Session session)
        {
            try
            {
                session.Log("Begin ValidatePaths");

                string[] paths = session["AIMS_PATHS_MULTILINE"]
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim(' ', '\t'))
                    .Select(p => p.Trim('"'))
                    .Where(p => !String.IsNullOrWhiteSpace(p))
                    .Select(p => p.EndsWith(@"\") ? p : p + @"\")
                    .ToArray();
                string[] invalidPaths = paths
                    .Where(path => !Directory.Exists(path))
                    .ToArray();
                if (invalidPaths.Any())
                {
                    MessageBox.Show(String.Join(Env.NewLine,
                        new[] { "The specified paths were not found:" }.Concat(invalidPaths)),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    session["AIMS_PATHS_VALID"] = "0";
                    return ActionResult.Success;
                }

                session["AIMS_PATHS"] = String.Join(";", paths.Select(p => "\"" + p + "\""));
                session["AIMS_PATHS_VALID"] = "1";

                session.Log("End ValidatePaths");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("ValidatePaths, exception: {0}", ex.Message);
                return ActionResult.Failure;
            }
        }
    }
}