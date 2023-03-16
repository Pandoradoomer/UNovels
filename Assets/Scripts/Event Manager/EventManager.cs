using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class EventManager 
{
    
    private static Dictionary<IEvent, Action<IEventPacket>> eventDictionary = new Dictionary<IEvent, Action<IEventPacket>>();

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
