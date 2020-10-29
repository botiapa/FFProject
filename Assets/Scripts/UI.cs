using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Button ServerButton;
    public Button ClientButton;

    public GameObject Server;
    public GameObject Client;

    public void OnServerButtonClicked() 
    {
        Server.SetActive(true);
        Destroy(this);
    }

    public void OnClientButtonClicked()
    {
        Client.SetActive(true);
        Destroy(this);
    }
}
