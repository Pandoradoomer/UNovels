using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class EventManager 
{
    
    private static Dictionary<IEvent, Action<IEventPacket>> eventDictionary = new Dictionary<IEvent, Action<IEventPacket>>();

    //private static EventManager eventManager;
    //public static EventManager Instance
    //{
    //    get
    //    {
    //        if(!eventManager)
    //        {
    //            eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
    //        }
    //        if(!eventManager)
    //        {
    //            Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene!");
    //        }
    //        else
    //        {
    //            eventManager.Init();
    //            DontDestroyOnLoad(eventManager);
    //        }
    //        return eventManager;
    //    }
    //}
    //
    //void Init()
    //{
    //    if(eventDictionary == null)
    //    {
    //        eventDictionary = new Dictionary<IEvent, Action<IEventPacket>>();
    //    }
    //}

    public static void StartListening(IEvent e, Action<IEventPacket> listener)
    {
        Action<IEventPacket> thisEvent;
        if(eventDictionary.TryGetValue(e, out thisEvent))
        {
            thisEvent += listener;
            eventDictionary[e] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            eventDictionary.Add(e, thisEvent);
        }
    }

    public static void StopListening(IEvent e, Action<IEventPacket> listener)
    {
        //if (eventManager == null)
        //    return;
        Action<IEventPacket> thisEvent;
        if(eventDictionary.TryGetValue(e, out thisEvent))
        {
            thisEvent -= listener;
            eventDictionary[e] = thisEvent;
        }
    }

    public static void TriggerEvent(IEvent e, IEventPacket packet)
    {
        Action<IEventPacket> thisEvent = null;
        if(eventDictionary.TryGetValue(e, out thisEvent))
        {
            thisEvent.Invoke(packet);
        }
    }




}
