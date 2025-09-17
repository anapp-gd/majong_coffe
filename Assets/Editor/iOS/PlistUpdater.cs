using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class PlistUpdater : MonoBehaviour
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
        {
            return;
        }
        
        string plistPath = pathToBuiltProject + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        PlistElementDict rootDict = plist.root;
        rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
        rootDict.SetString("NSUserTrackingUsageDescription", "Allow tracking to receive more relevant ads and support the app’s development.");
    
        /*** To add more keys :
         ** rootDict.SetString("<your key>", "<your value>");
         ***/

        File.WriteAllText(plistPath, plist.WriteToString());

        Debug.Log("Info.plist updated!");
    }
}