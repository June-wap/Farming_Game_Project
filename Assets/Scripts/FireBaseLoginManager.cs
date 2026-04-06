using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class FireBaseLoginManager : MonoBehaviour
{

    //Register

    [Header("Register")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;

    public Button buttonRegister;

    //Login
    [Header("Login")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;
    public Button buttonLogin;


    // Firebase Autherization --> Dang ki , Dang nhap 
    private FirebaseAuth auth;

    //Chuyen doi qua lai giua dang ki , dang nhap

    [Header("Switch Forms")]
    public Button buttonMoveToLogin;
    public Button buttonMoveToRegister;

    public GameObject LoginForm;
    public GameObject RegisterForm;


    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        buttonRegister.onClick.AddListener(RegisterAccountWithFireBase);

        buttonLogin.onClick.AddListener(LoginAccountWithFireBase);

        buttonMoveToRegister.onClick.AddListener(SwitchForm);

        buttonMoveToLogin.onClick.AddListener(SwitchForm);
    }

    public void RegisterAccountWithFireBase()
    {
        string email = ipRegisterEmail.text;
        string password = ipRegisterPassword.text;
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
           if (task.IsCanceled)
           {
               Debug.LogError("Dang ki that bai");
               return;
           }
           if (task.IsFaulted)
           {
               Debug.LogError("Dang ki that bai" + task.Exception);
               return;
           }
           if (task.IsCompleted)
           {
               Debug.LogFormat("Dang ki thanh cong voi email: {0}", email);
           }
        });
    }
    public void LoginAccountWithFireBase()
    {
        string email = ipLoginEmail.text;
        string password = ipLoginPassword.text;
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Dang nhap that bai");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Dang nhap that bai" + task.Exception);
                return;
            }
            if (task.IsCompleted)
            {
                Debug.LogFormat("Dang nhap thanh cong voi email: {0}", email);
                FirebaseUser user = task.Result.User;

                //Chuyen sang scene game sau khi dang nhap thanh cong

                SceneManager.LoadScene("Home");
            }
        });
    }

    public void SwitchForm()
    {
        if (LoginForm.activeSelf)
        {
            LoginForm.SetActive(!LoginForm.activeSelf);
            RegisterForm.SetActive(!RegisterForm.activeSelf);
        }
        else
        {
            LoginForm.SetActive(!LoginForm.activeSelf);
            RegisterForm.SetActive(!RegisterForm.activeSelf);
        }
    }
        
}
