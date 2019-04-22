using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VRCModLoader;
using VRCTools;
using static UnityEngine.UI.Button;

namespace AutoRelog
{
    [VRCModInfo("AutoRelog", "1.0", "Slaynash")]
    public class AutoRelog : VRCMod
    {
        private void OnApplicationStart()
        {
            ModManager.StartCoroutine(VRCToolsSetup());
        }

        private IEnumerator VRCToolsSetup()
        {
            yield return VRCUiManagerUtils.WaitForUiManagerInit();

            VRCModLogger.Log("[AutoRelog] Overwriting login button event");
            VRCUiPageAuthentication[] authpages = Resources.FindObjectsOfTypeAll<VRCUiPageAuthentication>();
            VRCUiPageAuthentication loginPage = authpages.First((page) => page.gameObject.name == "LoginUserPass");
            if (loginPage != null)
            {
                Button loginButton = loginPage.transform.Find("ButtonDone (1)")?.GetComponent<Button>();
                if (loginButton != null)
                {
                    ButtonClickedEvent bce = loginButton.onClick;
                    loginButton.onClick = new ButtonClickedEvent();
                    loginButton.onClick.AddListener(() => {
                        SecurePlayerPrefs.SetString("autorelog_login", GetTextFromUiInputField(loginPage.loginUserName), "vl9u1grTnvXA");
                        SecurePlayerPrefs.SetString("autorelog_password", GetTextFromUiInputField(loginPage.loginPassword), "vl9u1grTnvXA");
                        bce?.Invoke();
                    });
                    
                    Transform useprebiousTransform = UnityUiUtils.DuplicateButton(loginButton.transform, "Use Last\nCredentials", new Vector2(440, 0));
                    useprebiousTransform.GetComponent<RectTransform>().sizeDelta *= 1.8f;
                    Button useprebiousButton = useprebiousTransform.GetComponent<Button>();
                    useprebiousButton.onClick = new ButtonClickedEvent();
                    useprebiousButton.onClick.AddListener(() => {
                        SetTextToUiInputField(loginPage.loginUserName, SecurePlayerPrefs.GetString("autorelog_login", "vl9u1grTnvXA"));
                        SetTextToUiInputField(loginPage.loginPassword, SecurePlayerPrefs.GetString("autorelog_password", "vl9u1grTnvXA"));
                    });
                    if (!SecurePlayerPrefs.HasKey("autorelog_login"))
                    {
                        useprebiousButton.interactable = false;
                    }
                }
                else
                    VRCModLogger.Log("[VRCTools] Unable to find login button in login page");
                
            }
            else
                VRCModLogger.Log("[VRCTools] Unable to find login page");
        }



        private string GetTextFromUiInputField(UiInputField field)
        {
            /*
            FieldInfo textField = typeof(UiInputField).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(f => f.FieldType == typeof(string) && f.Name != "placeholderInputText");
            return textField.GetValue(field) as string;
            */
            MethodInfo getTextMethod = typeof(UiInputField).GetMethod("get_text", BindingFlags.Public | BindingFlags.Instance);
            return getTextMethod.Invoke(field, new object[0]) as string;
        }

        private void SetTextToUiInputField(UiInputField field, string text)
        {
            MethodInfo setTextMethod = typeof(UiInputField).GetMethod("set_text", BindingFlags.Public | BindingFlags.Instance);
            setTextMethod.Invoke(field, new object[] { text });
            field.GetComponent<InputFieldValidator>()?.Validate(text);
        }
    }
}
