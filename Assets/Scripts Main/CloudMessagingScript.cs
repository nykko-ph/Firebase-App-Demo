using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Database;

public class CloudMessagingScript : MonoBehaviour
{
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;
    private string topic = "appDemo_TestTopic";

    [SerializeField]
    GameObject sanity_obj;
    Text sanity;


    [SerializeField]
    GameObject FCMDisplayPanel_prefab;
    [SerializeField]
    GameObject FCMDisplayPanelSpawnPt_prefab;
    [SerializeField]
    Canvas mainCanvas;

    //database vars
    ArrayList animalBoard = new ArrayList();
    const int MaxLogs = 5; //up to 5 app instances
    destroyer destroyerScr = GameObject.Find("Destroyer").GetComponent<destroyer>();//will also count animals
    int animalCount = 0;

    void Start()
    {
        //db init
        animalBoard.Clear();
        animalBoard.Add("# of animals leaving pet cemetery:");
        //
        sanity = sanity_obj.GetComponent<Text>();
        CheckFirebaseDependancy();
        DebugLog("Welcome!");
    }

    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string errorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    errorCode = String.Format("Error.{0}: ",
                      ((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString());
                }
                DebugLog(errorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }

    public void DebugLog(string s)
    {
        sanity.text += s + "\n";
    }

    private void CheckFirebaseDependancy()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
                Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(
                    task => {
                        LogTaskCompletion(task, "SubscribeAsync");
                    }
                  );
                string message = "Subscribed to " + topic;
                Debug.Log(message);
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(task => {
            LogTaskCompletion(task, "SubscribeAsync");
        });
        DebugLog("Firebase Messaging Initialized");

        // This will display the prompt to request permission to receive
        // notifications if the prompt has not already been displayed before. (If
        // the user already responded to the prompt, thier decision is cached by
        // the OS and can be changed in the OS settings).
        Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
          task => {
              LogTaskCompletion(task, "RequestPermissionAsync");
          }
        );

        //initialize database listener
        StartDBListener();

        isFirebaseInitialized = true;
    }

    private void StartDBListener()
    {
        FirebaseDatabase.DefaultInstance
        .GetReference("Animals").OrderByChild("count")
        .ValueChanged += (object sender2, ValueChangedEventArgs e2) => {
            if (e2.DatabaseError != null)
            {
                Debug.LogError(e2.DatabaseError.Message);
                return;
            }
            Debug.Log("Received values for animal count.");
            string title = animalBoard[0].ToString();
            animalBoard.Clear();
            animalBoard.Add(title);
            if (e2.Snapshot != null && e2.Snapshot.ChildrenCount > 0)
            {
                foreach (var childSnapshot in e2.Snapshot.Children)
                {
                    if (childSnapshot.Child("count") == null
                        || childSnapshot.Child("count").Value == null)
                    {
                        Debug.LogError("Bad data in sample.");
                        break;
                    }
                    else
                    {
                        Debug.Log("Leaders entry : " +
                        childSnapshot.Child("user").Value.ToString() + " - " +
                        childSnapshot.Child("count").Value.ToString());
                        animalBoard.Insert(1, childSnapshot.Child("count").Value.ToString()
                        + "  " + childSnapshot.Child("user").Value.ToString());
                    }
                }
            }
        };
    }

    //db transaction handler
    TransactionResult AddAnimalCountTransaction(MutableData mutableData)
    {
        //load animals list
        List<object> animals_old = mutableData.Value as List<object>;
        if (animals_old == null)
        {
            animals_old = new List<object>();
        }
        List<object> animals_new = new List<object>();

        //insert newest first
        
        Dictionary<string, object> newAnimalCountMap = new Dictionary<string, object>();
        newAnimalCountMap["count"] = animalCount;
        newAnimalCountMap["user"] = "testDB_User";
        animals_new.Add(newAnimalCountMap);

        //insert the 4 most recent counts
        for (int i = 0; i < MaxLogs - 1; i++)
        {
            animals_new.Add(animals_old[i]);
        }

        // complete transaction
        mutableData.Value = animals_new;
        return TransactionResult.Success(mutableData);
    }

    public void AddAnimalCount()
    {
        animalCount = destroyerScr.GetAnimalCount();
        if (animalCount == 0)
        {
            DebugLog("invalid animal count (must be >0).");
            return;
        }

        DebugLog(String.Format("Attempting to add count {0} {1}",
          "testDB_User", animalCount.ToString()));

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Animals");

        DebugLog("Running Transaction...");
        // Use a transaction to ensure that we do not encounter issues with
        // simultaneous updates that otherwise might create more than MaxScores top scores.
        reference.RunTransaction(AddAnimalCountTransaction)
          .ContinueWithOnMainThread(task => {
              if (task.Exception != null)
              {
                  DebugLog(task.Exception.ToString());
              }
              else if (task.IsCompleted)
              {
                  DebugLog("Transaction complete.");
              }
          });
    }

    public virtual void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        DebugLog("Received a new message");
        var notification = e.Message.Notification;
        if (notification != null)
        {
            DebugLog("title: " + notification.Title);
            DebugLog("body: " + notification.Body);
            var android = notification.Android;
            if (android != null)
            {
                DebugLog("android channel_id: " + android.ChannelId);
                DisplayFCM(notification.Body);
            }
        }
        if (e.Message.From.Length > 0)
            DebugLog("from: " + e.Message.From);
        if (e.Message.Link != null)
        {
            DebugLog("link: " + e.Message.Link.ToString());
        }
        if (e.Message.Data.Count > 0)
        {
            DebugLog("data:");
            foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
                     e.Message.Data)
            {
                DebugLog("  " + iter.Key + ": " + iter.Value);
            }
        }
    }

    public virtual void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        DebugLog("Received Registration Token: " + token.Token);
    }

    public void ToggleTokenOnInit()
    {
        bool newValue = !Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled;
        Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
        DebugLog("Set TokenRegistrationOnInitEnabled to " + newValue);
    }

    void DisplayFCM(string message)
    {
        // Log an event with an int parameter.
        FirebaseAnalytics.LogEvent("Achievment", FirebaseAnalytics.ParameterAchievementId, "Received Foreground Message for the First Time.");

        GameObject messagePanel = new GameObject();
        messagePanel = Instantiate(FCMDisplayPanel_prefab, FCMDisplayPanelSpawnPt_prefab.transform.position, FCMDisplayPanelSpawnPt_prefab.transform.rotation, mainCanvas.transform) as GameObject;
        messagePanel.transform.GetChild(0).GetComponent<Text>().text = message;
    }
}
