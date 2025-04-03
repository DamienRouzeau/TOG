using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using TMPro;

public class registerpage : MonoBehaviour
{
    public GameObject mastercanvas;
    public GameObject register_object;
    public GameObject join_object;
    public Button registerButton;
    public GameObject registerButtonMask;

    RectTransform rtregister_object;
    RectTransform rtjoin_object;
    Vector2 rtregister_object_pos;
    Vector2 rtjoin_object_pos;

    multiplayerlobby _mpl = null;
    TMP_InputField _sallename;
    TMP_InputField _email;
    TMP_InputField _phone;
    TMP_InputField _name;
    TMP_InputField _unique;
    Text _errortext;

    private System.Action _okCbk = null;

    private void Awake()
    {
        Debug.Log("REGISTERPAGE AWAKE");
        rtregister_object = register_object.GetComponent<RectTransform>();
        rtjoin_object = join_object.GetComponent<RectTransform>();
        rtregister_object_pos = rtregister_object.anchoredPosition;
        rtjoin_object_pos = rtjoin_object.anchoredPosition;
        register_object.SetActive(false);
        join_object.SetActive(false);
    }

    private void Start()
    {
        Debug.Log("REGISTERPAGE START");
        _sallename = gameObject.FindInChildren("sallename").GetComponent<TMP_InputField>();
        _email = gameObject.FindInChildren("email").GetComponent<TMP_InputField>();
        _phone = gameObject.FindInChildren("phone").GetComponent<TMP_InputField>();
        _name = gameObject.FindInChildren("name").GetComponent<TMP_InputField>();
        _unique = gameObject.FindInChildren("unique").GetComponent<TMP_InputField>();
        _errortext = gameObject.FindInChildren("errortext").GetComponent<Text>();
        _errortext.text = "";

        _unique.onValueChanged.AddListener(OnPassCodeChanged);
        registerButton.enabled = false;
        registerButtonMask.SetActive(true);

        _mpl = (multiplayerlobby)mastercanvas.GetComponent<multiplayerlobby>();
    }

    public void Connect(System.Action cbk)
	{
        _okCbk = cbk;
        StopAllCoroutines();
        StartCoroutine(ConnectEnum());
	}

    private IEnumerator ConnectEnum()
	{
        while (string.IsNullOrEmpty(apicalls.machineid))
            yield return null;

        string json = "{\"id_machine\":\"" + apicalls.machineid + "\"}";

        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(apicalls.server_url + "register", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
            ShowPasscode();
        }
        else
        {
            Debug.Log(wwwtext);
            if (apicalls.ErrorTestRegister(wwwtext))
            {
                register_object.SetActive(true);
                join_object.SetActive(true);
                rtregister_object.anchoredPosition = rtregister_object_pos;
                rtjoin_object.anchoredPosition = rtjoin_object_pos;

                IDictionary res = (IDictionary)Json.Deserialize(wwwtext);
                if ((string)res["result"] == "error")
                {
                    string message = (string)res["message"];
                    switch (message)
                    {
                        case "CLIENT_NOT_FOUND":
                            ShowRegister();
                            break;
                        default:
                        case "IP_MISMATCH":
                            ShowPasscode();
                            break;
                    }
                }
                else
                {
                    ShowPasscode();
                }
            }
            else
            {
                AllOk();
			}
		}
    }

    private void OnPassCodeChanged(string text)
    {
        string result = _unique.text.Trim();
        if (result.Length < 12)
        {
            registerButton.enabled = false;
            registerButtonMask.SetActive(true);
            _unique.textComponent.color = Color.red * 0.75f;
        }
        else
        {
            registerButton.enabled = true;
            registerButtonMask.SetActive(false);
            _unique.textComponent.color = Color.white;
        }
    }

    private void ShowRegister()
	{
        register_object.SetActive(true);
        join_object.SetActive(false);
        rtregister_object.anchoredPosition = new Vector2(0, rtregister_object_pos.y);
    }

    private void ShowPasscode()
	{
        register_object.SetActive(false);
        join_object.SetActive(true);
        rtjoin_object.anchoredPosition = new Vector2(0, rtjoin_object_pos.y);
    }

    void AllOk()
    {
        _mpl.clientprefix = apicalls.id_salle;
        mastercanvas.SetActive(true);
        gameObject.SetActive(false);
        _okCbk?.Invoke();
    }

    IEnumerator Subscribe()
    {
        string code = _unique.text;
        string salle = _sallename.text;
        string mail = _email.text;
        string tel = _phone.text;
        string nm = _name.text;

        PlayerPrefs.SetString("license_passcode", code);
        PlayerPrefs.Save();

        string json = "{" +
            "\"id_machine\":\"" + apicalls.machineid + "\"," +
            "\"codeaccess\":\"" + code + "\"," +
            "\"sallename\":\"" + salle + "\"," +
            "\"email\":\"" + mail + "\"," +
            "\"phone\":\"" + tel + "\"," +
            "\"contactname\":\"" + nm + "\"" +
            "}";

        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(apicalls.server_url + "subscribe", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
        }
        else
        {
            Debug.Log("Subscribe:" + wwwtext);
            if (apicalls.ErrorTestRegister(wwwtext))
            {
                StartCoroutine("CleanCode");
                _errortext.text = "SUBSCRIBE ERROR: " + apicalls.last_error_message;
            }
            else
                AllOk();
        }
    }

    IEnumerator RegisterWithPasscode(string code,string error)
    {
        string json = "{" +
            "\"id_machine\":\"" + apicalls.machineid + "\"," +
            "\"codeaccess\":\"" + code + "\"" +
            "}";

        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(apicalls.server_url + "register", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
        }
        else
        {
            Debug.Log("Register:" + wwwtext);
            if (apicalls.ErrorTestRegister(wwwtext))
            {
                StartCoroutine("CleanCode");
                _errortext.text = error + "\nREGISTER ERROR: " + apicalls.last_error_message;
            }
            else
                AllOk();
        }
    }

    IEnumerator CleanCode()
    {
        yield return new WaitForSeconds(5);
        _errortext.text = "";
    }

    public void RegisterMe()
    {
        string code = _unique.text;
        string salle = _sallename.text;
        string mail = _email.text;
        string tel = _phone.text;
        string nm = _name.text;

        StartCoroutine("CleanCode");
        if (code == "")
        {
            _errortext.text = "MISSING CODE";
            return;
        }

        if (salle == "")
        {
            StartCoroutine(RegisterWithPasscode(code, "MISSING ARCADE NAME"));
            return;
        }
        if (mail == "")
        {
            StartCoroutine(RegisterWithPasscode(code, "MISSING MAIL"));
            return;
        }
        if (tel == "")
        {
            StartCoroutine(RegisterWithPasscode(code, "MISSING TELPEHONE"));
            return;
        }
        if (nm == "")
        {
            StartCoroutine(RegisterWithPasscode(code, "MISSING NAME"));
            return;
        }
        StartCoroutine(Subscribe());
    }


    public void JoinArcade()
    {
        Debug.Log("JoinArcade requested");
        StartCoroutine("_JoinArcade");
    }

    IEnumerator _JoinArcade()
    {
        InputField unique = join_object.FindInChildren("unique").GetComponent<InputField>();

        apicalls.myself.StartCoroutine(apicalls.myself.RegisterWithPasscode(unique.text));
        while (apicalls.myself.registercode_return == 0)
            yield return null;
        if (apicalls.myself.registercode_return == 2)
        {
            gameObject.SetActive(false);
        }
    }
}
