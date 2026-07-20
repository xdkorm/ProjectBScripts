using System;
using UnityEngine;

namespace ZigdarkS.ProjectB.Core
{
    public class InstanceProvider<T> where T : class
    {
        public T Instance { get; private set; }
        public event Action<T> OnSpawned;
        public event Action OnDespawned;

        public void Register(T instance) 
        { 
            Instance = instance; 
            OnSpawned?.Invoke(instance); 
        }
        public void Unregister() 
        { 
            Instance = null; 
            OnDespawned?.Invoke(); 
        }
    }
}